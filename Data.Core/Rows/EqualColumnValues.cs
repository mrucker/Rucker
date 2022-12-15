using System;
using System.Linq;
using System.Collections.Generic;

namespace Data.Core
{
    /// <summary>
    /// Checks to see if two rows contain matching values for a collection of columns
    /// </summary>
    public class EqualColumnValues : IEqualityComparer<IRow>
    {
        private readonly IEnumerable<string> _equalityColumns;

        public EqualColumnValues(IEnumerable<string> equalityColumns)
        {
            _equalityColumns = equalityColumns.ToArray();
        }

        public bool Equals(IRow row1, IRow row2)
        {            
            try
            {
                return _equalityColumns.All(c => row1[c]?.ToLower().Trim() == row2[c]?.ToLower().Trim());
            }
            catch (KeyNotFoundException)
            {
                if (RowIsMissingEqualityColumns(row1)) throw MissingColumnsException(row1);
                if (RowIsMissingEqualityColumns(row2)) throw MissingColumnsException(row2);

                throw;
            }
        }

        public int GetHashCode(IRow row)
        {
            //taken from http://stackoverflow.com/questions/892618/create-a-hashcode-of-two-numbers
            unchecked { return _equalityColumns.Select(c => row[c]?.Trim()).Select(o => o?.GetHashCode() ?? 0).Aggregate(23, (c, h) => c * 31 + h); }
        }

        private bool RowIsMissingEqualityColumns(IRow row)
        {
            return _equalityColumns.Except(row.Columns).Any();
        }

        private Exception MissingColumnsException(IRow row)
        {
            return new Exception($"One of the rows did not contain one of the equality columns. Row.Columns({row.Columns.Cat(", ")}) -- Distinct.Columns({_equalityColumns.Cat(", ")}).");
        }
    }
}