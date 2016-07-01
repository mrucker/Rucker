using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Rucker.Dispose;

namespace Rucker.Flow
{
    /// <summary>
    /// A pipe that consumes and produces in the background. It will only start one additional thread.    
    /// </summary>
    internal sealed class AsyncMidPipe<T> : Disposable, IMidPipe<T, T>
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

            Task.Run(() =>
            {
                try
                {
                    Status = PipeStatus.Working;

                    foreach (var produce in Consumes)
                    {
                        block.Add(produce);
                    }
                }
                catch (Exception)
                {
                    Status = PipeStatus.Errored;
                    throw;
                }

                Status = PipeStatus.Finished;
            });

            return block.GetConsumingEnumerable();
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