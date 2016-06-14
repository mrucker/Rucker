using System;
using System.Collections.Generic;
using System.Linq;
using Rucker.Exceptions;
using Rucker.Extensions;

namespace Rucker.Data
{
    public interface IRow: IEnumerable<KeyValuePair<string,string>>, IComparable
    {
        IEnumerable<string> Values { get; }
        IEnumerable<string> Columns { get; }

        /// <summary>
        /// Gets or Sets the value in the column, for the row
        /// </summary>
        /// <param name="column">Column Name</param>
        /// <returns>Value in Column</returns>
        string this[string column] { get; set; }
        
        /// <summary>
        /// Add a column/value to a Row. Any object can be stored but it will be ToString()'ed when retrieved unless it is accessed using GetRawValue()
        /// </summary>
        void Add(string column, object value);

        /// <summary>
        /// Remove a column/value from a Row.
        /// </summary>
        void Remove(string column);
    }

    public static class IRowExtensions
    {
        public static void Rename(this IRow row, string oldColumn, string newColumn)
        {
            var oldValue = row[oldColumn];
            
            row.Remove(oldColumn);
            row.Add(newColumn, oldValue);
        }

        public static void SafeAdd(this IRow row, string column, object value)
        {
            if (!row.Columns.Contains(column))
            {
                row.Add(column, value);
            }
        }

        public static string Caseless(this IRow row, string column)
        {
            var caselessMatch = row.Where(kv => kv.Key.ToLower() == column.ToLower()).ToArray();

            if(caselessMatch.None()) throw new ColumnNotFoundException(column);
            if(caselessMatch.Many()) throw new Exception($"Multiple columns matched the casless column ({caselessMatch.Select(k => k.Key).Cat(", ")})");

            return caselessMatch.Single().Value;
        }

        public static IRow Union(this IRow row1, IRow row2)
        {
            return new UnionRow(row1, row2);
        }

        #region With or Without
        public static IRow WithoutColumns(this IRow row, IEnumerable<string> withoutColumns)
        {
            return new WithoutColumnsRow(row, withoutColumns);
        }

        public static IRow WithColumns(this IRow row, IEnumerable<string> withColumns)
        {
            withColumns = withColumns.ToArray();

            var missingWithColumns = withColumns.Except(row.Columns).ToArray();

            if (missingWithColumns.Any())
            {
                throw new UserMessageException($"Expected columns ({missingWithColumns.Cat(", ")}) were missing");
            }

            var withoutColumns = row.Columns.Except(withColumns).ToArray();

            return new WithoutColumnsRow(row, withoutColumns);
        }
        #endregion
    }
}