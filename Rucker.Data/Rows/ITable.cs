using System.Collections.Generic;

namespace Rucker.Data
{
    public interface ITable
    {
        /// <summary>
        /// The schema of the Table
        /// </summary>
        string Schema { get; }

        /// <summary>
        /// The name of the Table
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The collection of IRows contained by the Table
        /// </summary>
        IRows Rows { get; }

        /// <summary>
        /// The columns of the first row in the table (It is assumed all rows have the same columns).
        /// </summary>
        IEnumerable<string> Columns { get; } 
    }
}