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
    internal sealed class BackgroundMidPipe<T> : IMidPipe<T, T>
    {
        #region Fields        
        private BlockingCollection<T> _blockingCollection;
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
                lock (this)
                {
                    if (_blockingCollection != null && !_blockingCollection.IsCompleted)
                    {
                        return _blockingCollection; 
                    }

                    _blockingCollection = new BlockingCollection<T>();
                }
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
}