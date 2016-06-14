using System;
using System.Linq;
using NUnit.Framework;

namespace Rucker.Flow.Tests
{
    public class ActionJobTests
    {
        [Test]
        public void StartFinishReporterTests()
        {
            var reporter = new TestingStateReporter();

            var actionJob = new ActionJob(() => { })
            {
                Tracker =
                {
                    WholeReporters = { reporter }
                }
            };

            actionJob.Process();

            Assert.IsTrue(reporter.Started);
            Assert.IsTrue(reporter.Finished);
        }

        [Test]
        public void ExceptionReporterTests()
        {
            var reporter  = new TestingErrorReporter();
            var exception = new Exception();

            var actionJob = new ActionJob(() => { throw exception; })
            {
                Tracker =
                {
                    ErrorReporters = { reporter }
                }
            };

            try { actionJob.Process(); } catch { }

            Assert.Contains(exception, reporter.ReportedExceptions.ToArray());
            Assert.AreEqual(1, reporter.ReportedExceptions.Count());
        }

        [Test]
        public void ProgressReporterTests()
        {
            var reporter  = new TestingProgressReporter();

            var actionJob = new ActionJob(() => { })
            {
                Tracker =
                {
                    PieceReporters = { reporter }
                }
            };

            actionJob.Process();

            Assert.AreEqual(1, reporter.ProgressReports.Count());
            Assert.AreEqual(100, reporter.ProgressReports.Single().Percent);
        }
    }
}