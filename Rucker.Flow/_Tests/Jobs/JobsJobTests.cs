using System;
using Rucker.Flow;
using NUnit.Framework;
using Rucker.Flow.Tests.Classes;
using Rucker.Testing;

namespace Rucker.Flow.Tests.Jobs
{
    [TestFixture]
    public class JobsJobTests
    {
        [Test]
        public void JobsJobRunsAllJobsOnce()
        {
                using (var job = new JobsJob(new[] { new DelayedEtlJob(500), new DelayedEtlJob(500) }))
                {
                    var expExecutionTime = new TimeSpan(0, 0, 0, 2);
                    var actExecutionTime = Test.ExecutionTime(job.Process);

                    Assert.That(actExecutionTime, Is.EqualTo(expExecutionTime).Within(500).Milliseconds);
            }
        }
    }
}