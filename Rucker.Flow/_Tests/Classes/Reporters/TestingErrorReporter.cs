using System;
using System.Linq;
using System.Collections.Generic;

namespace Rucker.Flow.Tests.Classes
{
    public class TestingErrorReporter: IErrorReporter
    {
        public IEnumerable<Exception> ReportedExceptions { get; private set; }

        public TestingErrorReporter()
        {
            ReportedExceptions = Enumerable.Empty<Exception>();
        }

        public void Report(Exception exception)
        {
            if (ReportedExceptions.Contains(exception))
            {
                throw new Exception("Exception already reported once");
            }

            ReportedExceptions = ReportedExceptions.Concat(new[]{exception}).ToArray();
        }
    }
}