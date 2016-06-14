using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

namespace Rucker.Flow.Tests
{
    public class TestingProgressReporter: IStateReporter
    {
        public List<TestingProgressReport> ProgressReports { get; }
 
        public TestingProgressReporter()
        {
            ProgressReports = new List<TestingProgressReport>();
        }

        public void ReportStart(string message, decimal percent, bool errored)
        {
            
        }

        public void ReportFinish(string message, decimal percent, bool errored)
        {
            if( ProgressReports.Any() && percent < ProgressReports.Last().Percent)
            {
                throw new AssertionException("Latest percent completed is less than the previous reported percentage");
            }

            ProgressReports.Add(new TestingProgressReport { Message = message, Percent = percent});
        }
    }
}