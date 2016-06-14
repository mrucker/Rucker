using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rucker.Extensions;

namespace Rucker.Flow
{            
    public sealed class DonePipesPipe<P, C> : PipesPipe where P : class, C
    {
        internal DonePipesPipe(IFirstPipe<P> first, ILastPipe<C> last) : base(first, last)
        {
            last.Consumes = first.Produces;
        }
    }

    public sealed class FirstPipesPipe<P1, C1, P2> : PipesPipe, IFirstPipe<P2> where P1 : class, C1
    {
        #region
        private readonly IMidPipe<C1, P2> _mid;
        #endregion

        #region Properties
        public IEnumerable<P2> Produces => _mid.Produces;
        #endregion

        #region Constructor
        public FirstPipesPipe(IFirstPipe<P1> first, IMidPipe<C1, P2> mid) : base(first, mid)
        {            
            mid.Consumes = first.Produces;

            _mid = mid;
        }
        #endregion
    }

    public sealed class LastPipesPipe<C1, P1, C2>: PipesPipe, ILastPipe<C1> where P1 : class, C2
    {
        #region Fields
        private readonly IMidPipe<C1, P1> _mid;
        #endregion

        #region Properties
        public IEnumerable<C1> Consumes { set { _mid.Consumes = value; } }
        #endregion

        #region Constructors
        internal LastPipesPipe(IMidPipe<C1, P1> mid, ILastPipe<C2> last) : base(mid, last)
        {
            last.Consumes = mid.Produces;

            _mid = mid;
        }
        #endregion
    }

    public sealed class MidPipesPipe<C1, P1, C2, P2> : PipesPipe, IMidPipe<C1, P2> where P1 : class, C2
    {
        #region Fields
        private readonly IMidPipe<C1, P1> _mid1;
        private readonly IMidPipe<C2, P2> _mid2;
        #endregion

        #region Properties
        public IEnumerable<P2> Produces { get { return _mid2.Produces; } }

        public IEnumerable<C1> Consumes { set { _mid1.Consumes = value; } }
        #endregion

        #region Constructors
        public MidPipesPipe(IMidPipe<C1, P1> mid1, IMidPipe<C2, P2> mid2) : base(mid1, mid2)
        {
            mid2.Consumes = mid1.Produces;

            _mid1 = mid1;
            _mid2 = mid2;
        }
        #endregion
    }

    public class PipesPipe: IPipe
    {
        #region Fields
        private readonly IEnumerable<IPipe> _pipes;
        #endregion

        #region Properties
        public PipeStatus PipeStatus
        {
            get
            {
                var statuses = _pipes.Select(p => p.PipeStatus).ToArray();

                if(statuses.Contains(PipeStatus.Errored)) return PipeStatus.Errored;
                if(statuses.Contains(PipeStatus.Working)) return PipeStatus.Working;
                if(statuses.Contains(PipeStatus.Waiting)) return PipeStatus.Waiting;
                
                if(statuses.All(s => s == PipeStatus.Stopped)) return PipeStatus.Stopped;
                if(statuses.All(s => s == PipeStatus.Created)) return PipeStatus.Created;

                throw new Exception($"The pipeline has an invalid status chain: {_pipes.Select(p => p.PipeStatus.ToString()).Cat("->")}");
            }
        }
        #endregion

        #region Constructors
        internal PipesPipe(IPipe one, IPipe two)
        {
            _pipes = new [] { one, two }.SelectMany(p => (p as PipesPipe)?._pipes ?? new[] { p } ).ToArray();
        }
        #endregion

        #region Public Methods
        public Task Start()
        {
            return Task.WhenAll(_pipes.Select(p => p.Start()));
        }
        public void Stop()
        {
            _pipes.FirstOrDefault()?.Stop();
        }
        #endregion
    }
}