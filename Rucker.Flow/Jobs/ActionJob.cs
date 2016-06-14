using System;

namespace Rucker.Flow
{
    public class ActionJob: Job
    {
        #region Properties
        public Action Action { get; }
        #endregion

        #region Constructor
        public ActionJob(Action action)
        {
            Action = action;
        }
        #endregion

        #region Overrides
        protected override void Initializing()
        {

        }

        protected override void Processing()
        {
            using (Tracker.Whole(1))
            using (Tracker.Piece())
            {
                Action();
            }
        }
        #endregion
    }
}