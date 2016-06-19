using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Rucker.Flow
{
    /// <summary>
    /// A pipe that runs in the background. It will only ever start at most one background thread.
    /// If you want multiple threads you attach a threaded pipe upstream from this background pipe
    /// </summary>
    internal sealed class AsyncMidPipe<T> : IMidPipe<T, T>
    {
        #region Fields
        [ThreadStatic]
        private static BlockingCollection<T> _blockingCollection;
        #endregion

        #region Properties
        public PipeStatus Status { get; private set; }
        public IEnumerable<T> Produces => Asynchronous();
        public IEnumerable<T> Consumes { private get; set; }
        #endregion

        #region Private Methods
        private IEnumerable<T> Asynchronous()
        {            
            if (_blockingCollection == null || _blockingCollection.IsCompleted)
            {
                if (_blockingCollection != null && !_blockingCollection.IsCompleted)
                {
                    return _blockingCollection; 
                }

                _blockingCollection = new BlockingCollection<T>();
            }

            Task.Run(() =>
            {
                try
                {
                    Status = PipeStatus.Working;

                    foreach (var produce in Consumes)
                    {
                        _blockingCollection.Add(produce);
                    }
                }
                catch (Exception)
                {
                    Status = PipeStatus.Errored;
                    throw;
                }

                Status = PipeStatus.Finished;
            });            

            return _blockingCollection;
        }
        #endregion
    }

    internal sealed class AsyncDonePipe : IDonePipe
    {
        #region Fields
        [ThreadStatic]
        private static Task _task;
        private readonly IDonePipe _done;
        #endregion

        #region Constructor
        public AsyncDonePipe(IDonePipe done)
        {
            _done = done;            
        }
        #endregion

        #region Properties
        public PipeStatus Status => _done.Status;
        #endregion

        #region Public Methods
        public void Start()
        {
            _task = _task ?? Task.Run((Action)_done.Start);
        }

        public void Stop()
        {
            _done.Stop();
        }
        #endregion
    }
}