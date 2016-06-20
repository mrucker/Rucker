using System;
using System.Linq;
using NUnit.Framework;

namespace Rucker.Flow.Tests
{
    [TestFixture]
    public class ProgressTrackerTests
    {
        [Test]
        public void CompletionReporterStarts()
        {
            var reporter = new TestingStateReporter();
            var tracker  = new Tracker
            {
                WholeReporters = { reporter }
            };

            using (tracker.Whole(1))
            using (tracker.Piece())
            {

            }

            Assert.IsTrue(reporter.Started);
        }

        [Test]
        public void CompletionReporterFinishes()
        {
            var reporter = new TestingStateReporter();
            var tracker  = new Tracker
            {
                WholeReporters = { reporter }
            };

            using (tracker.Whole(1))
            using (tracker.Piece())
            {

            }

            Assert.IsTrue(reporter.Started);
            Assert.IsTrue(reporter.Finished);
        }

        [Test]
        public void ExceptionReported()
        {
            var reporter = new TestingErrorReporter();
            var exception = new Exception();

            var tracker = new Tracker
            {
                ErrorReporters = { reporter }
            };

            tracker.Error(exception, "");

            Assert.Contains(exception, reporter.ReportedExceptions.ToArray());
            Assert.AreEqual(1, reporter.ReportedExceptions.Count());
        }

        [Test]
        public void ExceptionReportedAfterFinish()
        {
            var reporter  = new TestingErrorReporter();
            var exception = new Exception();

            var tracker = new Tracker(false)
            {
                ErrorReporters = { reporter }
            };

            try
            {
                using (tracker.Whole(2))
                {
                    using (tracker.Piece())
                    {
                        throw exception;
                    }
                }
            }
            catch (Exception ex)
            {
                tracker.Error(ex, "");
            }

            Assert.Contains(exception, reporter.ReportedExceptions.ToArray());
            Assert.AreEqual(1, reporter.ReportedExceptions.Count());
        }

        [Test]
        public void CompletionReporterIndicatesError()
        {
            var wholeReporter = new TestingStateReporter();
            var pieceReporter = new TestingStateReporter();            
            var errorReporter = new TestingErrorReporter();
            
            var exception = new Exception();

            var tracker = new Tracker(false)
            {
                WholeReporters = { wholeReporter },
                PieceReporters = { pieceReporter },                
                ErrorReporters = { errorReporter }
            };

            try
            {
                using (tracker.Whole(2))
                {
                    using (tracker.Piece())
                    {
                        throw exception;
                    }
                }
            }
            catch (Exception ex)
            {
                tracker.Error(ex, "");
            }

            Assert.IsTrue(wholeReporter.Started);
            Assert.IsTrue(pieceReporter.Started);

            Assert.IsFalse(wholeReporter.StartedErrored);
            Assert.IsFalse(pieceReporter.StartedErrored);

            Assert.IsFalse(wholeReporter.Finished);
            Assert.IsFalse(pieceReporter.Finished);

            Assert.IsTrue(wholeReporter.FinishedErrored);
            Assert.IsTrue(pieceReporter.FinishedErrored);
            
            Assert.AreEqual(exception, errorReporter.ReportedExceptions.Single());
        }

        [Test]
        public void ChildExceptionNotReportedInParent()
        {
            var reporter  = new TestingErrorReporter();
            var exception = new Exception();

            var parent = new Tracker();
            var child  = new Tracker();

            parent.ErrorReporters.Add(reporter);

            using (parent.Whole(new[]{child}))
            {
                using (child.Whole(4))
                {
                    using (child.Piece()) { }

                    child.Error(exception);
                }
            }

            Assert.AreEqual(0, reporter.ReportedExceptions.Count());
        }

        [Test]
        public void OnePartReported()
        {
            var reporter = new TestingProgressReporter();

            var tracker = new Tracker(false)
            {
                Message           = "Testing...",
                PieceReporters = { reporter }
            };

            using (tracker.Whole(1))
            using (tracker.Piece())
            {
                
            }
            
            Assert.That(reporter.ProgressReports.Single().Percent, Is.EqualTo(100));
            Assert.That(reporter.ProgressReports.Single().Message, Is.EqualTo("Testing..."));
        }

        [Test]
        public void FourPartsReported()
        {
            var tracker = new Tracker(false);
            var testingCompletionReporter = new TestingStateReporter();
            var testingProgressReporter = new TestingProgressReporter();

            tracker.WholeReporters.Add(testingCompletionReporter);
            tracker.PieceReporters.Add(testingProgressReporter);


            using (tracker.Whole(4))
            {
                Assert.That(testingCompletionReporter.Started, Is.True);

                using (tracker.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(25));

                using (tracker.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(50));

                using (tracker.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(75));

                using (tracker.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(100));
            }

            Assert.That(testingCompletionReporter.Finished, Is.True);
        }

        [Test]
        public void OneChildFourPartsReportedInParent()
        {
            var testingCompletionReporter = new TestingStateReporter();
            var testingProgressReporter = new TestingProgressReporter();

            var parent = new Tracker(false);
            var child1 = new Tracker(false);

            parent.WholeReporters.Add(testingCompletionReporter);
            parent.PieceReporters.Add(testingProgressReporter);

            using (parent.Whole(new[]{child1}))
            using (child1.Whole(4))
            {
                Assert.That(testingCompletionReporter.Started, Is.True);

                using (child1.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(25));

                using (child1.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(50));

                using (child1.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(75));

                using (child1.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(100));
            }

            Assert.That(testingCompletionReporter.Finished, Is.True);
        }

        [Test]
        public void TwoChildTwoPartReportedInParent()
        {
            var testingCompletionReporter = new TestingStateReporter();
            var testingProgressReporter   = new TestingProgressReporter();

            var parent = new Tracker(false);
            var child1 = new Tracker(false);
            var child2 = new Tracker(false);

            parent.WholeReporters.Add(testingCompletionReporter);
            parent.PieceReporters.Add(testingProgressReporter);

            using (parent.Whole(new[]{child1, child2}))
            using (child1.Whole(2))
            using (child2.Whole(2))
            {
                Assert.That(testingCompletionReporter.Started, Is.True);

                using (child1.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(25));

                using (child2.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(50));

                using (child1.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(75));

                using (child2.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(100));
            }

            Assert.That(testingCompletionReporter.Finished, Is.True);
        }

        [Test]
        public void FourChildOnePartReportedInParent()
        {
            var testingCompletionReporter = new TestingStateReporter();
            var testingProgressReporter   = new TestingProgressReporter();

            var parent = new Tracker(false);
            var child1 = new Tracker(false);
            var child2 = new Tracker(false);
            var child3 = new Tracker(false);
            var child4 = new Tracker(false);

            parent.WholeReporters.Add(testingCompletionReporter);
            parent.PieceReporters.Add(testingProgressReporter);

            using (parent.Whole(new[]{child1, child2, child3, child4}))
            {
                Assert.That(testingCompletionReporter.Started, Is.True);

                using (child1.Whole(1))
                using (child1.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(25));

                using (child2.Whole(1))
                using (child2.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(50));

                using (child3.Whole(1))
                using (child3.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(75));

                using (child4.Whole(1))
                using (child4.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(100));
            }

            Assert.That(testingCompletionReporter.Finished, Is.True);
        }

        [Test]
        public void OneChildFourPartReportedToParentAsync()
        {
            var testingCompletionReporter = new TestingStateReporter();
            var testingProgressReporter = new TestingProgressReporter();

            var parent = new Tracker(false);
            var child1 = new Tracker(false);

            parent.WholeReporters.Add(testingCompletionReporter);
            parent.PieceReporters.Add(testingProgressReporter);

            using (parent.Whole(new[]{child1}))
            using (child1.Whole(4))
            {
                using (child1.Piece()) { }
                using (child1.Piece()) { }
                using (child1.Piece()) { }
                using (child1.Piece()) { }
            }

            Assert.That(testingCompletionReporter.Started, Is.True);
            Assert.That(testingProgressReporter.ProgressReports[0].Percent, Is.EqualTo(25));
            Assert.That(testingProgressReporter.ProgressReports[1].Percent, Is.EqualTo(50));
            Assert.That(testingProgressReporter.ProgressReports[2].Percent, Is.EqualTo(75));
            Assert.That(testingProgressReporter.ProgressReports[3].Percent, Is.EqualTo(100));
            Assert.That(testingCompletionReporter.Finished, Is.True);
        }

        [Test]
        public void TwoChildTwoPartReportedToParentAsync()
        {
            var testingCompletionReporter = new TestingStateReporter();
            var testingProgressReporter   = new TestingProgressReporter();

            var parent = new Tracker(false);
            var child1 = new Tracker(false) { Message = "Child1"};
            var child2 = new Tracker(false) { Message = "Child2"};

            parent.WholeReporters.Add(testingCompletionReporter);
            parent.PieceReporters.Add(testingProgressReporter);

            using (parent.Whole(new[]{child1, child2}))
            using (child1.Whole(2))
            using (child2.Whole(2))
            {
                using (child1.Piece()) { }
                using (child2.Piece()) { }
                using (child1.Piece()) { }
                using (child2.Piece()) { }
            }

            Assert.That(testingCompletionReporter.Started, Is.True);
            Assert.That(testingProgressReporter.ProgressReports[0].Percent, Is.EqualTo(25));
            Assert.That(testingProgressReporter.ProgressReports[1].Percent, Is.EqualTo(50));
            Assert.That(testingProgressReporter.ProgressReports[2].Percent, Is.EqualTo(75));
            Assert.That(testingProgressReporter.ProgressReports[3].Percent, Is.EqualTo(100));

            Assert.That(testingProgressReporter.ProgressReports[0].Message, Is.EqualTo("Child1"));
            Assert.That(testingProgressReporter.ProgressReports[1].Message, Is.EqualTo("Child2"));
            Assert.That(testingProgressReporter.ProgressReports[2].Message, Is.EqualTo("Child1"));
            Assert.That(testingProgressReporter.ProgressReports[3].Message, Is.EqualTo("Child2"));
            Assert.That(testingCompletionReporter.Finished, Is.True);
        }

        [Test]
        public void TwoChildTwoGrandReportsToParent()
        {
            var testingCompletionReporter = new TestingStateReporter();
            var testingProgressReporter   = new TestingProgressReporter();

            var parent = new Tracker(false);
            var child1 = new Tracker(false) { Message = "Child1"};
            var child2 = new Tracker(false);
            var grand1 = new Tracker(false);
            var grand2 = new Tracker(false) { Message = "Grand2"};

            parent.WholeReporters.Add(testingCompletionReporter);
            parent.PieceReporters.Add(testingProgressReporter);

            Assert.That(testingCompletionReporter.Started, Is.False);
            using (parent.Whole(new[]{child1, child2}))
            {
                Assert.That(testingCompletionReporter.Started, Is.True);
                Assert.That(testingProgressReporter.ProgressReports, Is.Empty);

                using (child1.Whole(new[]{grand1}))
                using (grand1.Whole(1))
                using (grand1.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(50));
                Assert.That(testingProgressReporter.ProgressReports.Last().Message, Is.EqualTo("Child1"));

                using (child2.Whole(new[]{grand2}))
                using (grand2.Whole(1))
                using (grand2.Piece()) { }

                Assert.That(testingProgressReporter.ProgressReports.Last().Percent, Is.EqualTo(100));
                Assert.That(testingProgressReporter.ProgressReports.Last().Message, Is.EqualTo("Grand2"));
            }

            Assert.That(testingCompletionReporter.Finished, Is.True);
        }
    }
}