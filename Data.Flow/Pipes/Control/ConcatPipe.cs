using System;
using System.Linq;
using System.Collections.Generic;
using Data.Core;

namespace Data.Flow
{
    internal sealed class ConcatFirstLastPipe<P, C> : ConcatPipe, IClosedPipe where P : C
    {
        #region Fields
        private readonly IFirstPipe<P> _first;
        private readonly ILastPipe<C> _last;
        #endregion

        #region Constructors
        internal ConcatFirstLastPipe(IFirstPipe<P> first, ILastPipe<C> last) : base(first, last)
        {
            _first = first;
            _last  = last;

            last.Consumes = first.Produces.Cast<C>();
        }
        #endregion

        #region Public Methods
        public void Start()
        {
            _last.Start();
        }

        public void Stop()
        {
            _first.Stop();
        }
        #endregion
    }

    internal sealed class ConcatFirstMidPipe<P1, C1, P2> : ConcatPipe, IFirstPipe<P2> where P1 : C1
    {
        #region Fields
        private readonly IFirstPipe<P1> _first;
        private readonly IMidPipe<C1, P2> _mid;
        #endregion

        #region Properties
        public IEnumerable<P2> Produces => _mid.Produces;
        #endregion

        #region Constructor
        public ConcatFirstMidPipe(IFirstPipe<P1> first, IMidPipe<C1, P2> mid) : base(first, mid)
        {            
            mid.Consumes = first.Produces.Cast<C1>();

            _first = first;
            _mid   = mid;
        }
        #endregion

        #region Public Methods
        public void Stop()
        {
            _first.Stop();
        }
        #endregion
    }

    internal sealed class ConcatMidMidPipe<C1, P1, C2, P2> : ConcatPipe, IMidPipe<C1, P2> where P1 : class, C2
    {
        #region Fields
        private readonly IMidPipe<C1, P1> _mid1;
        private readonly IMidPipe<C2, P2> _mid2;
        #endregion

        #region Properties
        public IEnumerable<P2> Produces => _mid2.Produces;

        public IEnumerable<C1> Consumes { set { _mid1.Consumes = value; } }
        #endregion

        #region Constructors
        public ConcatMidMidPipe(IMidPipe<C1, P1> mid1, IMidPipe<C2, P2> mid2) : base(mid1, mid2)
        {
            mid2.Consumes = mid1.Produces;

            _mid1 = mid1;
            _mid2 = mid2;
        }
        #endregion
    }

    internal sealed class ConcatMidLastPipe<C1, P1, C2>: ConcatPipe, ILastPipe<C1> where P1 : class, C2
    {
        #region Fields
        private readonly IMidPipe<C1, P1> _mid;
        private readonly ILastPipe<P1> _last;
        #endregion

        #region Properties
        public IEnumerable<C1> Consumes { set { _mid.Consumes = value; } }
        #endregion

        #region Constructors
        internal ConcatMidLastPipe(IMidPipe<C1, P1> mid, ILastPipe<C2> last) : base(mid, last)
        {
            last.Consumes = mid.Produces;

            _mid  = mid;
            _last = last;
        }
        #endregion

        #region Public Methods
        public void Start()
        {
            _last.Start();
        }
        #endregion
    }    

    public class ConcatPipe: Disposable, IPipe
    {
        #region Fields
        private readonly IEnumerable<IPipe> _pipes;
        #endregion

        #region Properties
        public PipeStatus Status
        {
            get
            {
                var statuses = _pipes.Select(p => p.Status).ToArray();

                if (statuses.Any(s => s == PipeStatus.Errored)) return PipeStatus.Errored;
                if (statuses.Any(s => s == PipeStatus.Working)) return PipeStatus.Working;
                if (statuses.Any(s => s == PipeStatus.Stopped)) return PipeStatus.Stopped;
                if (statuses.All(s => s == PipeStatus.Finished)) return PipeStatus.Finished;
                if (statuses.All(s => s == PipeStatus.Created)) return PipeStatus.Created;

                throw new Exception($"The pipeline has an invalid status chain: {_pipes.Select(p => p.Status.ToString()).Cat("->")}");
            }
        }

        protected IEnumerable<PipeStatus> Statuses => _pipes.Select(p => p.Status);
        #endregion

        #region Constructors
        internal ConcatPipe(IPipe one, IPipe two)
        {
            _pipes = new [] { one, two }.SelectMany(p => (p as ConcatPipe)?._pipes ?? new[] { p } ).ToArray();
        }
        #endregion

        #region Disposable Pattern
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var pipe in _pipes)
                {
                    pipe.Dispose();
                }    
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}