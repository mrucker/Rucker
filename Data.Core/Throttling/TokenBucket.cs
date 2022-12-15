using System;
using System.Threading;

namespace Data.Core
{
    public class TokenBucket: Disposable
    {
        #region Fields
        private long _tokens;
        private bool _started;

        private readonly long _maxTokens;        
        private readonly Timer _refillTimer;
        private readonly int _refillCount;
        private readonly TimeSpan _refillSpan;
        private readonly EventWaitHandle _refillSignal;
        #endregion

        #region Constructors
        public TokenBucket(int maxTokens, int refillCount, TimeSpan refillSpan)
        {
            _tokens = 0;
            _started = false;

            _maxTokens   = maxTokens;            
            _refillCount = refillCount;
            _refillSpan  = refillSpan;

            _refillTimer  = new Timer(Refill);
            _refillSignal = new ManualResetEvent(false);
        }
        #endregion

        #region Public Methods
        public double BurstSeconds(double maxTokensPerSecond)
        {
            DisposeCheck();

            var refillPerSecond = _refillCount/_refillSpan.TotalSeconds;

            return refillPerSecond > maxTokensPerSecond ? double.PositiveInfinity: _maxTokens / (maxTokensPerSecond - refillPerSecond);
        }

        public double BurstTokens(double maxTokensPerSecond)
        {
            DisposeCheck();

            return BurstSeconds(maxTokensPerSecond) * maxTokensPerSecond;
        }

        public void Start()
        {
            DisposeCheck();

            _started = true;

            _refillTimer.Change(TimeSpan.Zero, _refillSpan);
        }

        public void Stop()
        {
            DisposeCheck();

            _started = false;

            _refillTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
        }

        public long RequestTokens(long requestSize)
        {
            DisposeCheck();

            if(!_started) throw new Exception("The TokenBucket must be started first before Requesting Tokens. Otherwise, the Request will never return");

            do
            {
                if(_tokens != 0)
                {                 
                    lock (this)
                    {
                        if (_tokens != 0)
                        {                            
                            var tokensGiven = Math.Min(requestSize, _tokens);

                            _tokens -= tokensGiven;

                            return tokensGiven;
                        }
                    }
                }                

                _refillSignal.WaitOne();
            } while (true);
        }

        #endregion

        #region Dispose Pattern
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refillTimer.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Private Methods
        private void Refill(object state)
        {            
            if (_tokens < _maxTokens)
            {
                Interlocked.Add(ref _tokens, Math.Min(_refillCount, _maxTokens - _tokens));
            }

            _refillSignal.Set();
        }
        #endregion
    }
}