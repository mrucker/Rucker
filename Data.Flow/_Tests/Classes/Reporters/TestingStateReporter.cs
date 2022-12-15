using System;

namespace Data.Flow.Tests
{
    public class TestingStateReporter: IStateReporter
    {
        public bool FinishedErrored { get; private set; }
        public bool StartedErrored { get; private set; }
        
        public bool Finished { get; private set; }
        public bool Started { get; private set; }

        public void ReportStart(string message, decimal percent, bool errored)
        {
            StartedErrored = errored;
            
            if (errored) return;            
            if (Started) throw new Exception("Already started once");

            Started = true;
        }

        public void ReportFinish(string message, decimal percent, bool errored)
        {
            FinishedErrored = errored;

            if (errored) return;
            if (Finished) throw new Exception("Already finished once");

            Finished = true;
        }
    }
}