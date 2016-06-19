using System;
using System.Collections.Generic;

namespace Rucker.Flow
{
    public class FirstLambdaPipe<P>: IFirstPipe<P>
    {
        #region Fields
        private bool _stop;
        #endregion

        #region Constructor
        public FirstLambdaPipe(Func<IEnumerable<P>> produces)
        {
            Produces = Produce(produces);
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
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Terrifying but necessary to handle exceptions correctly taken from here (http://stackoverflow.com/a/12060223/1066291)
        /// </summary>
        private IEnumerable<P> Produce(Func<IEnumerable<P>> produces)
        {
            if(_stop) { throw new Exception("This pipe has been stopped");}

            _stop = false;
            Status = PipeStatus.Working;

            IEnumerator<P> enumerator = null;

            while (true)
            {
                P ret;
                try
                {
                    enumerator = enumerator ?? produces().GetEnumerator();

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
        #endregion
    }

    public class MidLambdaPipe<C, P> : IMidPipe<C, P>
    {
        #region Fields
        private readonly Func<IEnumerable<C>, IEnumerable<P>> _maps;
        #endregion

        #region Constructor
        public MidLambdaPipe(Func<IEnumerable<C>, IEnumerable<P>> maps)
        {
            _maps = maps;
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

            IEnumerator<P> enumerator = null;

            while (true)
            {
                P ret;
                try
                {
                    enumerator = enumerator ?? _maps(Consumes).GetEnumerator();

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
        #endregion
    }

    public class LastLambdaPipe<C>: ILastPipe<C>
    {
        #region Private Method
        private readonly Action<IEnumerable<C>> _consumes;
        #endregion

        #region Constructor
        public LastLambdaPipe(Action<IEnumerable<C>> consumes)
        {
            _consumes = consumes;
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
                _consumes(Consumes);
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