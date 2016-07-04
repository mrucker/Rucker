using System;

namespace Rucker.Flow
{
    public struct PollLimit
    {
        public static PollLimit Forever => new PollLimit();

        public int PollCount { get; }
        public TimeSpan PollTime { get; }

        public PollLimit(int pollCount)
        {
            if (pollCount <= 0)
            {
                throw new ArgumentException("Must be greater than 0", nameof(pollCount));
            }

            PollCount = pollCount;
            PollTime = TimeSpan.Zero;
        }

        public PollLimit(TimeSpan pollTime)
        {
            if (pollTime <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be greater than 0", nameof(pollTime));
            }

            PollCount = 0;
            PollTime = TimeSpan.Zero;
        }

        public PollLimit(int pollCount, TimeSpan pollTime)
        {
            if (pollCount <= 0)
            {
                throw new ArgumentException("Must be greater than 0", nameof(pollCount));
            }

            if (pollTime <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be greater than 0", nameof(pollTime));
            }

            PollCount = 0;
            PollTime = TimeSpan.Zero;
        }

        public bool Reached(int pollCount, TimeSpan pollTime)
        {
            if (Equals(Forever)) return false;

            return (pollCount >= PollCount && pollCount != 0) || (pollTime >= PollTime && PollTime != TimeSpan.Zero);
        }
    }
}