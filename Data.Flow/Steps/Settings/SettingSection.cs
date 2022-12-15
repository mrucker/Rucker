using System.Configuration;

namespace Data.Flow
{
    public class SettingSection : ConfigurationSection, ISetting
    {
        /// <summary>
        /// Maximum number of threads that can be used to process an ETL step
        /// </summary>
        /// <remarks>A value of -1 would allow an infinite number of threads</remarks>
        /// <remarks>A value of 0 or less than -1 would throw an exception</remarks>
        [ConfigurationProperty("maxDegreeOfParallelism", DefaultValue = 1, IsRequired = false)]
        public int MaxDegreeOfParallelism => (int)this["maxDegreeOfParallelism"];

        /// <summary>
        /// Maximum page size that an ETL thread will have in memory at one time
        /// </summary>
        /// <remarks>A value of -1 would allow an infinite page size </remarks>
        /// <remarks>A value of 0 or less than -1 would throw an exception</remarks>
        [ConfigurationProperty("maxPageSize", DefaultValue = -1, IsRequired = false)]
        public int MaxPageSize => (int)this["maxPageSize"];

        /// <summary>
        /// Maximum number of timeout exceptions allowed over the life of a step before aborting.
        /// When a timeout occurs the failed query will be retried until it works.
        /// </summary>
        /// <remarks>A value of -1 would allow infinite timeouts </remarks>
        /// <remarks>A value of 0 or less than -1 would throw an exception</remarks>
        [ConfigurationProperty("maximumTimeoutsAllowed", DefaultValue = -1, IsRequired = false)]
        public int MaximumTimeoutsAllowed => (int)this["maximumTimeoutsAllowed"];

        /// <summary>
        /// Maximum number of seconds allowed before timing out
        /// </summary>
        [ConfigurationProperty("maximumTimeout", DefaultValue = 36000, IsRequired = false)]
        public int MaximumTimeout => (int)this["maximumTimeout"];

        /// <summary>
        /// Maximum number of timeout exceptions allowed for a particular query before aborting.
        /// When a timeout occurs the failed query will be retried until it works.
        /// </summary>
        /// <remarks>A value of -1 would allow infinite timeouts per query </remarks>
        /// <remarks>A value of 0 or less than -1 would throw an exception</remarks>
        [ConfigurationProperty("maximumTimeoutsAllowedPerQuery", DefaultValue = 3, IsRequired = false)]
        public int MaximumTimeoutsAllowedPerQuery => (int)this["maximumTimeoutsAllowedPerQuery"];
    }
}
