namespace Rucker.Flow
{
    /// <summary>
    /// Settings used to control how a Step is processed
    /// </summary>
    public class Setting: ISetting
    {
        private const int OneHour = 3600;

        /// <summary>
        /// Maximum number of threads that can be used to process an ETL step
        /// </summary>
        /// <remarks>A value of -1 would allow an infinite number of threads</remarks>
        /// <remarks>A value of 0 or less than -1 would throw an exception</remarks>
        public int MaxDegreeOfParallelism { get; set; }

        /// <summary>
        /// Maximum page size that an ETL thread will have in memory at one time.
        /// </summary>
        /// <remarks>A value of -1 would allow an infinite page size </remarks>
        /// <remarks>A value of 0 or less than -1 would throw an exception</remarks>
        public int MaxPageSize { get; set; }

        /// <summary>
        /// Maximum number of timeout exceptions allowed over the life of a step before aborting.
        /// When a timeout occurs the failed query will be retried until it works.
        /// </summary>
        /// <remarks>A value of -1 would allow infinite timeouts </remarks>
        /// <remarks>A value of 0 or less than -1 would throw an exception</remarks>
        public int MaximumTimeoutsAllowed { get; set; }

        /// <summary>
        /// Maximum number of timeout exceptions allowed for a particular query before aborting.
        /// When a timeout occurs the failed query will be retried until it works.
        /// </summary>
        /// <remarks>A value of -1 would allow infinite timeouts per query </remarks>
        /// <remarks>A value of 0 or less than -1 would throw an exception</remarks>
        public int MaximumTimeoutsAllowedPerQuery { get; set; }

        /// <summary>
        /// Maximum number of seconds allowed before timing out
        /// </summary>
        public int MaximumTimeout { get; set; }

        /// <summary>
        /// Creates a new Instance of StepSettings
        /// </summary>
        public Setting()
        {
            MaxDegreeOfParallelism         = 1;
            MaxPageSize                    = -1;
            MaximumTimeoutsAllowed         = -1;
            MaximumTimeoutsAllowedPerQuery = 3;
            MaximumTimeout                 = 10 * OneHour;
        }
    }
}
