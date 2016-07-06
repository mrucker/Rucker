using System;
using System.Diagnostics.Contracts;

namespace Rucker.Flow
{
    public struct PollLimit
    {
        #region Public Static
        public static PollLimit Forever => new PollLimit();
        #endregion

        #region Properties
        public int PollCount { get; set; }

        public TimeSpan PollTime { get; set; }

        public DateTime PollUntil { get; set; }
        #endregion

        #region Public Methods
        [Pure] //This is confusing to me but basically Pure just means we don't change anything (http://stackoverflow.com/q/9927434/1066291)
        public bool Reached(int pollCount, TimeSpan pollTime, DateTime dateTime)
        {
            if (Equals(Forever)) return false;
            
            return (pollCount >= PollCount && pollCount != default(int)) || (pollTime >= PollTime && PollTime != default(TimeSpan)) || (dateTime >= PollUntil && PollUntil != default(DateTime));
        }
        #endregion
    }
}