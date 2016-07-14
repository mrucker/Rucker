using System;
using System.Collections.Generic;
using Rucker.Dispose;

namespace Rucker.Flow
{
    public class LambdaFirstPipe<P>: Disposable, IFirstPipe<P>
    {
        #region Fields
        private bool _stop;
        #endregion

        #region Constructor
        public LambdaFirstPipe(Func<IEnumerable<P>> lambda)
        {
            Produces = Produce(lambda);
        }
        #endregion

        #region Properties
        public PipeStatus Status { private set; get; }

        public IEnumerable<P> Produces { get; }
        #endregion

        #region Public Methods
        public void Stop()
        {
            _stop = true;
            Status = PipeStatus.Stopped;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Terrifying but necessary to handle exceptions correctly. Taken from here (http://stackoverflow.com/a/12060223/1066291)
        /// </summary>
        private IEnumerable<P> Produce(Func<IEnumerable<P>> produce)
        {
            if(_stop) { throw new Exception("This pipe has been stopped");}
            
            Status = PipeStatus.Working;

            try
            {
                IEnumerator<P> enumerator = null;

                while (true)
                {
                    P ret;
                    try
                    {
                        enumerator = enumerator ?? produce().GetEnumerator();

                        if (!enumerator.MoveNext())
                        {
                            Status = PipeStatus.Finished;
                            break;
                        }

                        if (_stop)
                        {
                            Status = PipeStatus.Stopped;
                            break;
                        }
                        ret = enumerator.Current;
                    }
                    catch (Exception)
                    {
                        Status = PipeStatus.Errored;
                        throw;
                    }

                    yield return ret;
                }
            }
            finally
            {
                //this handles the case where a coller disposes the enumerator
                if (Status != PipeStatus.Errored && Status != PipeStatus.Stopped)
                {
                    Status = PipeStatus.Finished;
                }
            }
        }        
        #endregion
    }

    public class LambdaMidPipe<C, P> : Disposable, IMidPipe<C, P>
    {
        #region Fields
        private readonly Func<IEnumerable<C>, IEnumerable<P>> _lambda;
        #endregion

        #region Constructor
        public LambdaMidPipe(Func<IEnumerable<C>, IEnumerable<P>> lambda)
        {
            _lambda = lambda;
        }
        #endregion

        #region Properties
        public PipeStatus Status { private set; get; }
        public IEnumerable<C> Consumes { private get; set; }
        public IEnumerable<P> Produces => Produce();
        #endregion

        #region Private Methods
        /// <summary>
        /// Terrifying but necessary to handle exceptions correctly taken from here (http://stackoverflow.com/a/12060223/1066291)
        /// </summary>
        private IEnumerable<P> Produce()
        {
            Status = PipeStatus.Working;

            try
            {
                IEnumerator<P> enumerator = null;

                while (true)
                {
                    P ret;
                    try
                    {
                        enumerator = enumerator ?? _lambda(Consumes).GetEnumerator();

                        if (!enumerator.MoveNext())
                        {
                            Status = PipeStatus.Finished;
                            break;
                        }

                        ret = enumerator.Current;
                    }
                    catch (Exception)
                    {
                        Status = PipeStatus.Errored;
                        throw;
                    }

                    yield return ret;
                }
            }
            finally
            {
                //this handles the case where a coller disposes the enumerator
                if (Status != PipeStatus.Errored && Status != PipeStatus.Stopped)
                {
                    Status = PipeStatus.Finished;
                }
            }
        }
        #endregion
    }

    public class LambdaLastPipe<C>: Disposable, ILastPipe<C>
    {
        #region Private Method
        private readonly Action<IEnumerable<C>> _lambda;
        #endregion

        #region Constructor
        public LambdaLastPipe(Action<IEnumerable<C>> lambda)
        {
            _lambda = lambda;
        }
        #endregion

        #region Properties
        public PipeStatus Status { get; private set; }
        public IEnumerable<C> Consumes { get; set; }
        #endregion

        #region Public Method
        public void Start()
        {
            Status = PipeStatus.Working;

            try
            {
                _lambda(Consumes);
            }
            catch (Exception)
            {
                Status = PipeStatus.Errored;
                throw;
            }

            Status = PipeStatus.Finished;
        }
        #endregion
    }
}