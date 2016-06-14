using System;
using Rucker.Testing;
using NUnit.Framework;


namespace Rucker.Flow.Tests
{
    [TestFixture]
    public class JobsJobTests
    {
        [Test]
        public void JobsJobRunsAllJobsOnce()
        {
                using (var job = new JobsJob(new[] { new DelayedEtlJob(500), new DelayedEtlJob(500) }))
                {
                    var expExecutionTime = new TimeSpan(0, 0, 0, 1);
                    var actExecutionTime = Test.ExecutionTime(job.Process);

                    Assert.That(actExecutionTime, Is.EqualTo(expExecutionTime).Within(250).Milliseconds);
            }
        }
    }
}