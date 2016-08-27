using System;
using System.Linq;
using NUnit.Framework;
using Rucker.Core;
using Rucker.Core.Testing;


namespace Rucker.Flow.Tests
{
    [TestFixture]
    public class EtlCodeStepTests
    {
        #region Fields
        private readonly Func<EtlCodeStep<IRows, IRows>> _etlFactory;
        #endregion

        #region Constructors
        public EtlCodeStepTests(): this(() => new EtlCodeStep<IRows, IRows>())
        {
            
        }

        public EtlCodeStepTests(Func<EtlCodeStep<IRows, IRows>> etlFactory)
        {
            _etlFactory = etlFactory;
        }
        #endregion

        [Test]
        public void MultiThreadedEtlStepTest()
        {
            var oneSecond = new TimeSpan(0, 0, 0, 1);                        

            using (var etl = _etlFactory())
            {
                etl.Reader = new ReadDelay(500);
                etl.Writer = new WriteDelay(500);

                etl.Setting = new Setting { MaxDegreeOfParallelism = 3, MaxPageSize = etl.Reader.Size() / 3 };

                var executionTime = Test.ExecutionTime(etl.Process);

                Console.WriteLine();
                Console.WriteLine("Actual ETLStep Time: " + executionTime);

                Assert.That(executionTime, Is.EqualTo(oneSecond).Within(250).Milliseconds);
            }
        }

        [Test]
        public void SingleThreadedEtlStepTest()
        {
            var threeSeconds = new TimeSpan(0, 0, 0, 3);

            using (var etl = _etlFactory())
            {
                etl.Reader = new ReadDelay(500);
                etl.Writer = new WriteDelay(500);

                etl.Setting = new Setting { MaxDegreeOfParallelism = 1, MaxPageSize = etl.Reader.Size() / 3 };

                var executionTime = Test.ExecutionTime(etl.Process);

                Console.WriteLine();
                Console.WriteLine("Actual ETLStep Time: " + executionTime);

                Assert.That(executionTime, Is.EqualTo(threeSeconds).Within(500).Milliseconds);
            }
        }

        [Test]
        public void FailedEtlStepTest()
        {
            var errorReporter = new TestingErrorReporter();

            using (var etl = _etlFactory())
            {
                etl.Tracker.ErrorReporters.Add(errorReporter);
                etl.Reader = new ReadMemory();
                etl.Writer = new WriteFailed();

                var thrownException = Assert.Throws<AggregateException>(etl.Process);
                var reportedException = errorReporter.ReportedExceptions.Single();

                Assert.AreEqual(thrownException, reportedException);
            }
        }
    }
}