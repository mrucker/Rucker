using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Rucker.Extensions;

namespace Rucker.Flow
{
    public class MultiStep: Step
    {
        #region Properties
        protected IEnumerable<Step> Steps { get; set; }
        #endregion

        #region Constructors
        public MultiStep() : this(Enumerable.Empty<Step>())
        { }

        public MultiStep(IEnumerable<Step> steps)
        {
            Steps = steps;
        }
        #endregion

        #region Overrides
        protected override void Initializing()
        {
            Steps = Steps.ToArray();
        }

        protected sealed override void Processing()
        {
            Steps = Steps.ToArray();

            using (Tracker.Whole(Steps.Select(j => j.Tracker), ToString()))
            {
                Parallel.ForEach(Steps, new ParallelOptions { MaxDegreeOfParallelism = 1 }, j => j.Process());
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Steps != null)
            {
                foreach (var step in Steps)
                {
                    step.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        public override string ToString()
        {
            return Steps.None() ? "MultiStep.Empty()" : Steps.Select(j => j.ToString()).Cat(", ");
        }
        #endregion
    }
}