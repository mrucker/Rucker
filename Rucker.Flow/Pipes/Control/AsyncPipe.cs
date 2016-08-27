using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Rucker.Core;

namespace Rucker.Flow
{
    /// <summary>
    /// A pipe that consumes and produces in the background. It will only start one additional thread.
    /// </summary>
    /// <remarks>
    /// The difference between ThreadedMidPipe(1) and AsyncPipe() is that ThreadedMidPipe(1) will only allow one thread to come out no matter how many threads go in.
    /// AsyncPipe on the other hand doesn't collapse incoming threads. It will simply start a new background thread for each incoming thread.
    /// </remarks>
    internal sealed class AsyncPipe<T> : Disposable, IMidPipe<T, T>
    {
        #region Properties
        public PipeStatus Status { get; private set; }
        public IEnumerable<T> Produces => Asynchronous();
        public IEnumerable<T> Consumes { private get; set; }
        #endregion

        #region Private Methods
        private IEnumerable<T> Asynchronous()
        {
            var block = new BlockingCollection<T>();
            
            var task = Task.Run(() =>
            {
                try
                {
                    foreach (var produce in Consumes)
                    {                       
                        block.Add(produce);
                    }
                }
                finally
                {
                    block.CompleteAdding();
                }               
            });

            Status = PipeStatus.Working;

            try
            {
                foreach (var item in block.GetConsumingEnumerable())
                {
                    yield return item;
                }

                try
                {
                    task.Wait();
                    Status = PipeStatus.Finished;
                }
                catch (Exception)
                {
                    Status = PipeStatus.Errored;
                    throw;
                }
            }
            finally
            {
                Status = PipeStatus.Finished;
                block.CompleteAdding();
                try { task.Wait(); } catch { /* Because we completed adding before the async pipe was done it's going to blow */ }
            }
        }
        #endregion
    }

    internal sealed class AsyncClosedPipe : Disposable, IClosedPipe
    {
        #region Fields
        [ThreadStatic]
        private static Task _task;
        private readonly IClosedPipe _closed;
        #endregion

        #region Constructor
        public AsyncClosedPipe(IClosedPipe closed)
        {
            _closed = closed;            
        }
        #endregion

        #region Properties
        public PipeStatus Status => _closed.Status;
        #endregion

        #region Public Methods
        public void Start()
        {
            _task = _task ?? Task.Run((Action)_closed.Start);
        }

        public void Stop()
        {
            _closed.Stop();
        }
        #endregion
    }
}