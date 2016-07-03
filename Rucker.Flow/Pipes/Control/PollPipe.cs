using System;
using System.Threading;
using System.Collections.Generic;

namespace Rucker.Flow
{
    public class PollPipe<T> : LambdaMidPipe<T, T>
    {
        public PollPipe(TimeSpan startTime, TimeSpan cycleTime) : base(consumes => Polled(consumes, startTime, cycleTime))
        {

        }

        public PollPipe(TimeSpan cycleTime) : base((consumes) => Polled(consumes, null, cycleTime))
        {

        }

        private static IEnumerable<T> Polled(IEnumerable<T> consumes, TimeSpan? startTime, TimeSpan cycleTime)
        {
            while (true)
            {
                Thread.Sleep(TimeTillEndOfNextCycle(startTime, cycleTime));

                foreach (var item in consumes)
                {
                    yield return item;
                }                
            }
        }

        private static TimeSpan TimeTillEndOfNextCycle(TimeSpan? startTime, TimeSpan cycleTime)
        {
            if (startTime == null) return cycleTime;
            if (cycleTime == TimeSpan.Zero) return TimeSpan.Zero;

            var cycleTicks = cycleTime.Ticks;
            var startTicks = startTime.Value.Ticks;

            //positive means startTime is in the future, negative means it is in the past.
            var ticksTillStart = startTicks - DateTime.Now.TimeOfDay.Ticks;

            //whether positive or negative this will always be in the future in regards to ticksTillStart
            var nextCycleIndex = Math.Floor((double)ticksTillStart / cycleTicks);

            //if we are 10 ticks into a 30 tick cycle this will return 20
            var ticksToNextCycle = ticksTillStart - ((long)nextCycleIndex * cycleTicks);

            //if ticksToNextCycle is less than a second then wait until the cycle after next otherwise just wait until the next cycle
            return (ticksToNextCycle < TimeSpan.TicksPerSecond / 3) ? TimeSpan.FromTicks(cycleTicks + ticksToNextCycle) : TimeSpan.FromTicks(ticksToNextCycle);
        }
    }
}