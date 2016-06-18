using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;


namespace Rucker.Flow
{
    public class AsyncFirstPipe<P>: IFirstPipe<P>
    {
        #region Fields
        private readonly IFirstPipe<P> _firstPipe;
        #endregion

        #region Properties
        public PipeStatus Status => _firstPipe.Status;
        public IEnumerable<P> Produces => Start();
        #endregion

        #region Constructor
        public AsyncFirstPipe(IFirstPipe<P> firstPipe)
        {
            _firstPipe = firstPipe;
        }
        #endregion

        #region Public Methods
        public void Stop()
        {
            _firstPipe.Stop();   
        }
        #endregion

        #region Private Methods
        private IEnumerable<P> Start()
        {
            var blockingCollection = new BlockingCollection<P>();

            Task.Run(() =>
            {
                foreach (var produce in _firstPipe.Produces)
                {
                    blockingCollection.Add(produce);
                }

                blockingCollection.CompleteAdding();
            });

            return blockingCollection;
        }
        #endregion
    }

    public class AsyncMidPipe<C, P>: IMidPipe<C, P>
    {
        #region Fields
        private readonly IMidPipe<C, P> _midPipe;
        #endregion

        #region Properties
        public PipeStatus Status => _midPipe.Status;
        public IEnumerable<P> Produces => Start();
        public IEnumerable<C> Consumes { set { _midPipe.Consumes = value; } }
        #endregion

        #region Constructor
        public AsyncMidPipe(IMidPipe<C, P> midPipe)
        {
            _midPipe = midPipe;
        }
        #endregion    

        #region Private Methods
        private IEnumerable<P> Start()
        {
            var blockingCollection = new BlockingCollection<P>();

            Task.Run(() =>
            {
                foreach (var produce in _midPipe.Produces)
                {
                    blockingCollection.Add(produce);
                }

                blockingCollection.CompleteAdding();
            });

            return blockingCollection;
        }
        #endregion
    }

    public class AsyncLastPipe<P> : ILastPipe<P>
    {
        #region Fields
        private readonly ILastPipe<P> _lastPipe;
        #endregion

        #region Properties
        public PipeStatus Status => _lastPipe.Status;
        public IEnumerable<P> Consumes { set { _lastPipe.Consumes = value; } }
        #endregion

        #region Constructor
        public AsyncLastPipe(ILastPipe<P> lastPipe)
        {
            _lastPipe = lastPipe;
        }
        #endregion

        #region Public Methods
        public void Start()
        {
            Task.Run((Action)_lastPipe.Start);
        }
        #endregion
    }
}