using System;
using System.Collections.Generic;
using System.Linq;

namespace Rucker.Core
{
    /// <summary>
    /// A collection of columns and their values
    /// </summary>
    public class DictionaryRow: BaseRow
    {
        #region Fields
        private readonly Dictionary<string, object> _values;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of an empty Row
        /// </summary>
        public DictionaryRow()
        {
            _values = new Dictionary<string, object>();
        }
        #endregion

        #region IRow Implementation
        /// <summary>
        /// Gets or Sets the value in the column, for the row
        /// </summary>
        /// <param name="column">Column Name</param>
        /// <returns>Value in Column</returns>
        public override string this[string column]
        {
            get
            {
                AssertHasColumn(Hasher.Hash(column));
                return _values[Hasher.Hash(column)]?.ToString();
            }

            set
            {
                AssertHasColumn(Hasher.Hash(column));
                _values[Hasher.Hash(column)] = value;
            }
        }

        public override IEnumerable<string> Columns => _values.Keys.Any() ? _values.Keys : Enumerable.Empty<string>();

        public override IEnumerable<string> Values
        {
            get { return _values.Values.Any() ? _values.Values.Select(v => v.ToString()) : Enumerable.Empty<string>(); }
        }

        public override void Add(string column, object value)
        {
            var hash = Hasher.Hash(column);

            try
            {
                _values.Add(hash, value);
            }
            catch (OutOfMemoryException ex)
            {
                throw new OutOfMemoryException($"DictionaryRow ran out of memory (value count: {_values.Count})", ex);
            }   
        }

        public override void Remove(string column)
        {
            _values.Remove(Hasher.Hash(column));
        }
        #endregion
    }
}
