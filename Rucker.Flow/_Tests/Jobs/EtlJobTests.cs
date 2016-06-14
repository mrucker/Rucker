using System;
using System.Linq;
using NUnit.Framework;
using Rucker.Flow.Tests.Classes;
using Rucker.Testing;

namespace Rucker.Flow.Tests.Jobs
{
    public class EtlJobTests
    {
        [Test]
        public void MultiThreadedEtlJobTest()
        {
            var oneSecond = new TimeSpan(0, 0, 0, 1);

            using (var job = new DelayedEtlJob(1000))
            {
                job.Setting = new Setting { MaxDegreeOfParallelism = 3, MaxPageSize = job.Reader.Size() / 3 };

                var executionTime = Test.ExecutionTime(job.Process);

                Console.WriteLine();
                Console.WriteLine("Actual ETLJob Time: " + executionTime);

                Assert.That(executionTime, Is.EqualTo(oneSecond).Within(250).Milliseconds);
            }
        }

        [Test]
        public void SingleThreadedEtlJobTest()
        {
            var threeSeconds = new TimeSpan(0, 0, 0, 3);

            using (var job = new DelayedEtlJob(1000))
            {
                job.Setting = new Setting { MaxDegreeOfParallelism = 1, MaxPageSize = job.Reader.Size() / 3 };

                var executionTime = Test.ExecutionTime(job.Process);

                Console.WriteLine();
                Console.WriteLine("Actual ETLJob Time: " + executionTime);

                Assert.That(executionTime, Is.EqualTo(threeSeconds).Within(500).Milliseconds);
            }
        }

        [Test]
        public void FailedEtlJobTest()
        {
            var errorReporter = new TestingErrorReporter();

            using (var job = new FailedEtlJob { Tracker = { ErrorReporters = { errorReporter }}})
            {
                var thrownException = Assert.Throws<AggregateException>(job.Process);
                var reportedException = errorReporter.ReportedExceptions.Single();

                Assert.AreEqual(thrownException, reportedException);
            }
        }
    }
}