using System;
using Rucker.Data;
using NUnit.Framework;

namespace Rucker.Tests
{
    [TestFixture]
    public class TokenBucketTests
    {
        [TestCase(10, 100, 1)]
        [TestCase(20, 100, 1)]
        [TestCase(10, 100, 2)]
        [TestCase(20, 100, 2)]
        //This test fails occasionally depending on concurrency and resource issues on the running machine. Run the test again in case of a fail and see if it passes.
        //Despite the inconsistency in the test outcome I still have found it useful for code coverage purposes. 
        //95% of the time the test gives a valid pass so if it fails three times in a row there is probably a real problem.
        public void TokenBucketThrottlesAppropriately(int tokenRequests, int tokenMilliseconds, int threadCount)
        {
            using (var tokenBucket = new TokenBucket(3, 1, TimeSpan.FromMilliseconds(tokenMilliseconds)))
            {
                tokenBucket.Start();

                var actualExecutionTime   = Testing.Test.ExecutionTime(threadCount, tokenRequests, () => tokenBucket.RequestTokens(1) );
                var expectedExecutionTime = TimeSpan.FromMilliseconds(tokenRequests * tokenMilliseconds);

                tokenBucket.Stop();

                Assert.That(actualExecutionTime, Is.EqualTo(expectedExecutionTime).Within(tokenMilliseconds).Milliseconds);
            }
        }

        [TestCase(200, 10, 1)]
        [TestCase(10, 100, 10)]
        [Ignore("If we set our refill time too low (e.g. 10 miliseconds) or run in a high threaded environment (e.g. 10 threads) the current TokenBucket implementation adds tokens at a slower than desired rate.")]
        public void TokenBucketFailsToThrottleAppropriately(int tokenRequests, int tokenMilliseconds, int threadCount)
        {
            using (var tokenBucket = new TokenBucket(3, 1, TimeSpan.FromMilliseconds(tokenMilliseconds)))
            {
                tokenBucket.Start();

                var actualExecutionTime = Testing.Test.ExecutionTime(threadCount, tokenRequests, () => tokenBucket.RequestTokens(1));
                var expectedExecutionTime = TimeSpan.FromMilliseconds(tokenRequests * tokenMilliseconds);

                tokenBucket.Stop();

                Assert.That(actualExecutionTime, Is.EqualTo(expectedExecutionTime).Within(tokenMilliseconds).Milliseconds);
            }
        }
    }
}