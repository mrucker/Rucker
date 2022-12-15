using System;
using System.Linq;
using NUnit.Framework;

namespace Data.Flow.Tests
{
    [TestFixture]
    public class ActionStepTests
    {
        [Test]
        public void StartFinishReporterTests()
        {
            var reporter = new TestingStateReporter();

            var actionStep = new ActionStep(() => { })
            {
                Tracker =
                {
                    WholeReporters = { reporter }
                }
            };

            actionStep.Process();

            Assert.IsTrue(reporter.Started);
            Assert.IsTrue(reporter.Finished);
        }

        [Test]
        public void ExceptionReporterTests()
        {
            var reporter  = new TestingErrorReporter();
            var exception = new Exception();

            var actionStep = new ActionStep(() => { throw exception; })
            {
                Tracker =
                {
                    ErrorReporters = { reporter }
                }
            };

            try { actionStep.Process(); } catch { }

            Assert.Contains(exception, reporter.ReportedExceptions.ToArray());
            Assert.AreEqual(1, reporter.ReportedExceptions.Count());
        }

        [Test]
        public void ProgressReporterTests()
        {
            var reporter  = new TestingProgressReporter();

            var actionStep = new ActionStep(() => { })
            {
                Tracker =
                {
                    PieceReporters = { reporter }
                }
            };

            actionStep.Process();

            Assert.AreEqual(1, reporter.ProgressReports.Count());
            Assert.AreEqual(100, reporter.ProgressReports.Single().Percent);
        }
    }
}