using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Rucker.Extensions;

namespace Rucker.Flow
{
    public class JobsJob: Job
    {
        #region Properties
        protected IEnumerable<Job> Jobs { get; set; }
        #endregion

        #region Constructors
        public JobsJob() : this(Enumerable.Empty<Job>())
        { }

        public JobsJob(IEnumerable<Job> jobs)
        {
            Jobs = jobs;
        }
        #endregion

        #region Overrides
        protected override void Initializing()
        {
            Jobs = Jobs.ToArray();
        }

        protected sealed override void Processing()
        {
            Jobs = Jobs.ToArray();

            using (Tracker.Whole(Jobs.Select(j => j.Tracker), ToString()))
            {
                Parallel.ForEach(Jobs, new ParallelOptions { MaxDegreeOfParallelism = 1 }, j => j.Process());
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Jobs != null)
            {
                foreach (var job in Jobs)
                {
                    job.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        public override string ToString()
        {
            return Jobs.None() ? "JobsJob.Empty()" : Jobs.Select(j => j.ToString()).Cat(", ");
        }
        #endregion
    }
}