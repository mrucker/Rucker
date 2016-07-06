using System;
using System.Threading;
using System.Collections.Generic;

namespace Rucker.Flow
{
    /// <summary>
    /// This pipe is unique. It is the only pipe I've built that works by consuming everything that comes before it.
    /// Consumption was necessary for me to stop an Infinite poll by checking the status of the previous pipe.
    /// </summary>
    internal sealed class PollPipe<T> : IFirstPipe<T>
    {
        #region Fields
        private readonly IFirstPipe<T> _pipeToPoll;
        private readonly TimeSpan? _startTime;
        private readonly TimeSpan _cycleTime;
        private readonly PollLimit _pollLimit;

        #endregion

        #region Properties
        public PipeStatus Status => _pipeToPoll.Status;
        public IEnumerable<T> Produces => Poll();
        #endregion

        #region Constructor
        public PollPipe(IFirstPipe<T> pipeToPoll, TimeSpan startTime, TimeSpan cycleTime, PollLimit pollLimit)
        {
            _pipeToPoll = pipeToPoll;
            _startTime  = startTime;
            _cycleTime  = cycleTime;
            _pollLimit  = pollLimit;
        }

        public PollPipe(IFirstPipe<T> pipeToPoll, TimeSpan cycleTime, PollLimit pollLimit)
        {
            _pipeToPoll = pipeToPoll;
            _cycleTime  = cycleTime;
            _pollLimit  = pollLimit;
            _startTime  = null;
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            _pipeToPoll.Dispose();
        }

        public void Stop()
        {
            _pipeToPoll.Stop();
        }
        #endregion        

        #region Private Methods
        private IEnumerable<T> Poll()
        {
            var beginTime = DateTime.Now;

            var pollTime  = TimeSpan.Zero;
            var pollCount = 0;

            do
            {
                Thread.Sleep(TimeTillEndOfNextCycle());
                
                foreach (var item in _pipeToPoll.Produces)
                {
                    yield return item;
                }

                pollCount++;
                pollTime = DateTime.Now - beginTime;
            }
            while (_pipeToPoll.Status != PipeStatus.Stopped && _pipeToPoll.Status != PipeStatus.Errored && !_pollLimit.Reached(pollCount, pollTime, DateTime.Now));
        }

        private TimeSpan TimeTillEndOfNextCycle()
        {
            if (_startTime == null) return _cycleTime;
            if (_cycleTime == TimeSpan.Zero) return TimeSpan.Zero;

            var cycleTicks = _cycleTime.Ticks;
            var startTicks = _startTime.Value.Ticks;

            //positive means startTime is in the future, negative means it is in the past.
            var ticksTillStart = startTicks - DateTime.Now.TimeOfDay.Ticks;

            //whether positive or negative this will always be in the future in regards to ticksTillStart
            var nextCycleIndex = Math.Floor((double)ticksTillStart / cycleTicks);

            //if we are 10 ticks into a 30 tick cycle this will return 20
            var ticksToNextCycle = ticksTillStart - ((long)nextCycleIndex * cycleTicks);

            //if ticksToNextCycle is less than a second then wait until the cycle after next otherwise just wait until the next cycle
            return (ticksToNextCycle < TimeSpan.TicksPerSecond / 3) ? TimeSpan.FromTicks(cycleTicks + ticksToNextCycle) : TimeSpan.FromTicks(ticksToNextCycle);
        }
        #endregion
    }
}