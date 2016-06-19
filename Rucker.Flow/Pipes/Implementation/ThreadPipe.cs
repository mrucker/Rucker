using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Rucker.Flow
{    
    internal sealed class ThreadMidPipe<T>: IMidPipe<T, T>
    {
        #region Fields
        private readonly int _maxDegreeOfParallelism;
        private BlockingCollection<T> _blockingCollection;
        #endregion

        #region Constructor
        public ThreadMidPipe(int maxDegreeOfParallelism)
        {
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        #endregion

        #region Properties
        public PipeStatus Status { get; private set; }
        public IEnumerable<T> Produces => Threading();
        public IEnumerable<T> Consumes { private get; set; }
        #endregion

        #region Private Methods
        private IEnumerable<T> Threading()
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

            Status = PipeStatus.Working;

            try
            {                    
                Parallel.ForEach(Consumes, new ParallelOptions {MaxDegreeOfParallelism = _maxDegreeOfParallelism}, produce => _blockingCollection.Add(produce));
            }
            catch (Exception)
            {
                Status = PipeStatus.Errored;
                throw;
            }
            finally
            {
                _blockingCollection.CompleteAdding();
            }

            Status = PipeStatus.Finished;

            return _blockingCollection;
        }
        #endregion
    }
}