using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Rucker.Flow
{    
    internal sealed class ThreadedMidPipe<T>: IMidPipe<T, T>
    {
        #region Fields
        private readonly int _maxDegreeOfParallelism;
        private BlockingCollection<T> _blockingCollection;
        #endregion

        #region Constructor
        public ThreadedMidPipe(int maxDegreeOfParallelism)
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

    internal sealed class ThreadedDonePipe : IDonePipe
    {
        #region Fields
        private readonly IDonePipe _done;
        private readonly int _maxDegreeOfParallelism;
        #endregion

        #region Constructor
        public ThreadedDonePipe(IDonePipe done, int maxDegreeOfParallelism)
        {
            _done = done;
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        #endregion

        #region Properties
        public PipeStatus Status { get; private set; }
        #endregion

        #region Public Methods
        public void Start()
        {
            if (Status != PipeStatus.Working)
            {
                lock (this)
                {
                    if (Status == PipeStatus.Working)
                    {
                        return;
                    }

                    Status = PipeStatus.Working;
                }
            }

            var actions = Enumerable.Repeat<Action>(_done.Start, _maxDegreeOfParallelism).ToArray();

            try
            {
                Parallel.Invoke(new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, actions);
            }
            catch (Exception)
            {
                Status = PipeStatus.Errored;
                throw;
            }

            Status = PipeStatus.Finished;
        }

        public void Stop()
        {
            _done.Stop();
        }
        #endregion
    }
}