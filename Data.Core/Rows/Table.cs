using System.Linq;
using System.Collections.Generic;

namespace Data.Core
{
    /// <summary>
    /// Contains a collection of IRows and a Name
    /// </summary>
    public class Table: ITable
    {
        #region Constructors
        public Table(string name, IEnumerable<IRow> rows) : this("dbo", name, rows)
        {
            
        }

        public Table(string schema, string name, IEnumerable<IRow> rows)
        {
            rows = rows.ToArray();

            Schema  = schema;
            Name    = name;
            Rows    = rows.ToRows();
            Columns = rows.FirstOrDefault()?.Columns ?? Enumerable.Empty<string>();
        }
        #endregion

        #region ITable Implementation
        /// <summary>
        /// The schema of the Table
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// The name of the Table
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The collection of IRows contained by the Table
        /// </summary>
        public IRows Rows { get; }

        /// <summary>
        /// The columns of the first row in the table (It is assumed all rows have the same columns).
        /// </summary>
        public IEnumerable<string> Columns { get; }
        #endregion
    }
}
