using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Rucker.Dispose;

namespace Rucker.Flow
{        
    /// <remarks>
    /// The difference between ThreadedMidPipe(1) and AsyncPipe() is that ThreadedMidPipe(1) will only allow one thread to come out no matter how many threads go in.
    /// AsyncPipe on the other hand doesn't collapse incoming threads. It will simply start a new background thread for each incoming thread.
    /// </remarks>
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
                    return YieldReturnOrException(block);
                }

                lock (@lock)
                {
                    if (block != null && !block.IsCompleted)
                    {
                        return YieldReturnOrException(block);
                    }

                    block = new BlockingCollection<T>();
                }   

                var task = Task.Run(() =>
                {
                    try
                    {
                        if (maxDegreeOfParallelism == 1)
                        {
                            foreach (var item in consumes) { block.Add(item); }
                        }
                        else
                        {
                            //For some strange reason this doesn't always get the return of our consumes enumerator. I have no idea why. 
                            //I've only tested in single thread mode, so I don't know if the same issue exists in a multi-thread environment.
                            //To fix I added the above single thread implementation in the cases when maxDegreeOfParallelism is equal to 1.
                            Parallel.ForEach(consumes, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, item => { block.Add(item); });
                        }
                    }
                    finally
                    {                        
                        block.CompleteAdding();
                    }
                });

                return YieldReturnOrException(block, task);
            };
        }

        private static IEnumerable<T> YieldReturnOrException(BlockingCollection<T> block, Task task = null)
        {
            foreach (var item in block.GetConsumingEnumerable())
            {
                yield return item;
            }
            
            try
            {
                task?.Wait();
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }
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
                    Parallel.For(0, _maxDegreeOfParallelism, i => _closedPipe.Start() );
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