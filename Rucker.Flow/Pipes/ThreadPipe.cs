using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Rucker.Dispose;
using Rucker.Testing;

namespace Rucker.Flow
{    
    internal sealed class ThreadedMidPipe<T>: LambdaMidPipe<T, T>
    {
        #region Constructor
        public ThreadedMidPipe(int maxDegreeOfParallelism): base(Threading(maxDegreeOfParallelism))
        { }
        #endregion

        #region Private Methods
        private static Func<IEnumerable<T>, IEnumerable<T>> Threading(int maxDegreeOfParallelism)
        {
            var block = null as BlockingCollection<T>;
            var @lock = new object();

            return consumes =>
            {
                if (block != null && !block.IsCompleted)
                {
                    return block.GetConsumingEnumerable();
                }

                lock (@lock)
                {
                    if (block == null || block.IsCompleted)
                    {
                        block = new BlockingCollection<T>();

                        Task.Run(() =>
                        {
                            Parallel.ForEach(consumes, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, produce => block.Add(produce));

                            block.CompleteAdding();
                        });
                    }                    
                }

                return block.GetConsumingEnumerable();
            };
        }
        #endregion
    }

    internal sealed class ThreadedClosedPipe: Disposable, IClosedPipe
    {
        #region Fields
        private readonly IClosedPipe _closedPipe;
        private readonly int _maxDegreeOfParallelism;
        #endregion

        #region Constructor
        public ThreadedClosedPipe(IClosedPipe closedPipe, int maxDegreeOfParallelism)
        {
            _closedPipe             = closedPipe;
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }
        #endregion

        #region Properties
        public PipeStatus Status => _closedPipe.Status;
        #endregion

        #region Public Methods
        public void Start()
        {
            if (_closedPipe.Status == PipeStatus.Working)
            {
                return;
            }

            lock (this)
            {
                if (Status != PipeStatus.Working)
                {
                    Parallel.For(0, _maxDegreeOfParallelism, i =>
                    {
                        Test.WriteLine("Starting Pipe"); _closedPipe.Start();
                    });
                }
            }
        }

        public void Stop()
        {
            _closedPipe.Stop();
        }
        #endregion
    }
}