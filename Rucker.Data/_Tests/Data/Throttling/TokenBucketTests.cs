using System;
using NUnit.Framework;
using Rucker.Data;

namespace Rucker.Tests
{
    [TestFixture]
    public class TokenBucketTests
    {
        [TestCase(10, 100, 1)]
        [TestCase(20, 100, 1)]
        [TestCase(10, 100, 2)]
        [TestCase(20, 100, 2)]
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