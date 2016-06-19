using System;
using System.Linq;
using System.Collections.Generic;
using Rucker.Extensions;

namespace Rucker.Flow
{            
    internal sealed class FirstLastCoupling<P, C> : Coupling, IDonePipe where P : class, C
    {
        #region Fields
        private readonly IFirstPipe<P> _first;
        private readonly ILastPipe<C> _last;
        #endregion

        #region Constructors
        internal FirstLastCoupling(IFirstPipe<P> first, ILastPipe<C> last) : base(first, last)
        {
            _first = first;
            _last  = last;

            last.Consumes = first.Produces;
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

    internal sealed class FirstMidCoupling<P1, C1, P2> : Coupling, IFirstPipe<P2> where P1 : class, C1
    {
        #region Fields
        private readonly IFirstPipe<P1> _first;
        private readonly IMidPipe<C1, P2> _mid;
        #endregion

        #region Properties
        public IEnumerable<P2> Produces => _mid.Produces;
        #endregion

        #region Constructor
        public FirstMidCoupling(IFirstPipe<P1> first, IMidPipe<C1, P2> mid) : base(first, mid)
        {            
            mid.Consumes = first.Produces;

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

    internal sealed class MidMidCoupling<C1, P1, C2, P2> : Coupling, IMidPipe<C1, P2> where P1 : class, C2
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
        public MidMidCoupling(IMidPipe<C1, P1> mid1, IMidPipe<C2, P2> mid2) : base(mid1, mid2)
        {
            mid2.Consumes = mid1.Produces;

            _mid1 = mid1;
            _mid2 = mid2;
        }
        #endregion
    }

    internal sealed class MidLastCoupling<C1, P1, C2>: Coupling, ILastPipe<C1> where P1 : class, C2
    {
        #region Fields
        private readonly IMidPipe<C1, P1> _mid;
        private readonly ILastPipe<P1> _last;
        #endregion

        #region Properties
        public IEnumerable<C1> Consumes { set { _mid.Consumes = value; } }
        #endregion

        #region Constructors
        internal MidLastCoupling(IMidPipe<C1, P1> mid, ILastPipe<C2> last) : base(mid, last)
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

    public class Coupling: IPipe
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

                if(statuses.Contains(PipeStatus.Errored)) return PipeStatus.Errored;
                if(statuses.Contains(PipeStatus.Working)) return PipeStatus.Working;
                if(statuses.Contains(PipeStatus.Waiting)) return PipeStatus.Waiting;
                
                if(statuses.All(s => s == PipeStatus.Stopped)) return PipeStatus.Stopped;
                if(statuses.All(s => s == PipeStatus.Created)) return PipeStatus.Created;

                throw new Exception($"The pipeline has an invalid status chain: {_pipes.Select(p => p.Status.ToString()).Cat("->")}");
            }
        }

        protected IEnumerable<PipeStatus> Statuses => _pipes.Select(p => p.Status);
        #endregion

        #region Constructors
        internal Coupling(IPipe one, IPipe two)
        {
            _pipes = new [] { one, two }.SelectMany(p => (p as Coupling)?._pipes ?? new[] { p } ).ToArray();
        }
        #endregion
    }
}