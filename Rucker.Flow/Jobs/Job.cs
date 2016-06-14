using System;
using Rucker.Dispose;
using Rucker.Flow;

namespace Rucker.Flow
{
    public abstract class Job: Disposable
    {
        #region Properties
        public Tracker Tracker { get; }
        #endregion

        #region Constructors
        protected Job()
        {
            Tracker = new Tracker();
        }
        #endregion

        #region Public Methods
        public void Process()
        {
            try
            {
                Initializing();
                Processing();
            }
            catch (Exception ex)
            {
                Tracker.Error(ex, ToString());

                throw;
            }
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Any initialization logic that you want to be tracked should go here.
        /// </summary>
        protected abstract void Initializing();

        /// <summary>
        /// Where the work of the job should actually take place. 
        /// </summary>
        protected abstract void Processing();
        #endregion
    }
}