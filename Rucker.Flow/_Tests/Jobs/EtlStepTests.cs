using System;
using System.Linq;
using NUnit.Framework;
using Rucker.Testing;


namespace Rucker.Flow.Tests
{
    public class EtlStepTests
    {
        [Test]
        public void MultiThreadedEtlStepTest()
        {
            var oneSecond = new TimeSpan(0, 0, 0, 1);

            using (var step = new DelayedEtlStep(1000))
            {
                step.Setting = new Setting { MaxDegreeOfParallelism = 3, MaxPageSize = step.Reader.Size() / 3 };

                var executionTime = Test.ExecutionTime(step.Process);

                Console.WriteLine();
                Console.WriteLine("Actual ETLStep Time: " + executionTime);

                Assert.That(executionTime, Is.EqualTo(oneSecond).Within(250).Milliseconds);
            }
        }

        [Test]
        public void SingleThreadedEtlStepTest()
        {
            var threeSeconds = new TimeSpan(0, 0, 0, 3);

            using (var step = new DelayedEtlStep(1000))
            {
                step.Setting = new Setting { MaxDegreeOfParallelism = 1, MaxPageSize = step.Reader.Size() / 3 };

                var executionTime = Test.ExecutionTime(step.Process);

                Console.WriteLine();
                Console.WriteLine("Actual ETLStep Time: " + executionTime);

                Assert.That(executionTime, Is.EqualTo(threeSeconds).Within(500).Milliseconds);
            }
        }

        [Test]
        public void FailedEtlStepTest()
        {
            var errorReporter = new TestingErrorReporter();

            using (var step = new FailedEtlStep { Tracker = { ErrorReporters = { errorReporter }}})
            {
                var thrownException = Assert.Throws<AggregateException>(step.Process);
                var reportedException = errorReporter.ReportedExceptions.Single();

                Assert.AreEqual(thrownException, reportedException);
            }
        }
    }
}