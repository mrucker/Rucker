using System;
using System.Linq;
using NUnit.Framework;

namespace Data.Flow.Tests
{

    [TestFixture]
    public class StepTests
    {
        [Test]
        public void FailedStepTest1()
        {            
            var errorReporter = new TestingErrorReporter();

            using (var step = new FailInitializingStep { Tracker = { ErrorReporters = { errorReporter } }})
            {
                var thrownException   = Assert.Throws<Exception>(step.Process);
                var reportedException = errorReporter.ReportedExceptions.Single();

                Assert.AreEqual(thrownException, reportedException);
            }
        }

        [Test]
        public void FailedStepTest2()
        {
            var errorReporter = new TestingErrorReporter();

            using (var step = new FailProcessingStep { Tracker = { ErrorReporters = { errorReporter } } })
            {
                var thrownException = Assert.Throws<Exception>(step.Process);
                var reportedException = errorReporter.ReportedExceptions.Single();

                Assert.AreEqual(thrownException, reportedException);
            }
        }
    }
}