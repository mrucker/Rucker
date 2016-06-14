namespace Rucker.Flow
{
    public interface ISetting
    {
        /// <summary>
        /// Maximum number of threads that can be used to process an ETL job
        /// </summary>
        /// <remarks>A value of -1 would allow an infinite number of threads</remarks>
        /// <remarks>A value of 0 or less than -1 would throw an exception</remarks>
        int MaxDegreeOfParallelism { get; }

        /// <summary>
        /// Maximum page size that an ETL thread will have in memory at one time
        /// </summary>
        /// <remarks>A value of -1 would allow an infinite page size </remarks>
        /// <remarks>A value of 0 or less than -1 would throw an exception</remarks>
        int MaxPageSize { get; }

        /// <summary>
        /// Maximum number of seconds allowed before timing out
        /// </summary>
        int MaximumTimeout { get; }

        /// <summary>
        /// Maximum number of timeout exceptions allowed over the life of a job before aborting.
        /// When a timeout occurs the failed query will be retried until it works.
        /// </summary>
        /// <remarks>A value of -1 would allow infinite timeouts </remarks>
        /// <remarks>A value of 0 or less than -1 would throw an exception</remarks>
        int MaximumTimeoutsAllowed { get; }

        /// <summary>
        /// Maximum number of timeout exceptions allowed for a particular query before aborting.
        /// When a timeout occurs the failed query will be retried until it works.
        /// </summary>
        /// <remarks>A value of -1 would allow infinite timeouts per query </remarks>
        /// <remarks>A value of 0 or less than -1 would throw an exception</remarks>
        int MaximumTimeoutsAllowedPerQuery { get; } 
    }
}