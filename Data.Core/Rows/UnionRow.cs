using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.Core
{
    public class UnionRow:BaseRow
    {
        #region Fields
        private IRow _row1;
        private IRow _row2;
        private IRow _row3;
        #endregion

        #region Constructors
        public UnionRow(IRow row1, IRow row2)
        {
            _row1 = row1;
            _row2 = row2;
        }
        #endregion

        #region IRow Implementation
        public override IEnumerable<string> Values
        {
            get { return Columns.Select(column => this[column]); }
        }

        public override IEnumerable<string> Columns
        { 
            get 
            {   
                var row1Columns = _row1.Columns;
                var row2Columns = _row2.Columns;
                var thisColumns = (_row3 == null) ? Enumerable.Empty<string>() : _row3.Columns;

                return row1Columns.Concat(row2Columns).Concat(thisColumns).Distinct();
            }
        }

        public override string this[string column]
        {
            get
            {
                if (_row1.Columns.Contains(column))
                {
                    return _row1[column];
                }

                if (_row2.Columns.Contains(column))
                {
                    return _row2[column];
                }

                if (_row3 != null && _row3.Columns.Contains(column))
                {
                    return _row3[column];
                }
                
                throw new ColumnNotFoundException(column);
            }
            set
            {
                if (_row1.Columns.Contains(column))
                {
                    _row1[column] = value;
                }

                else if (_row2.Columns.Contains(column))
                {
                   _row2[column] = value;
                }

                else if (_row3 != null && _row3.Columns.Contains(column))
                {
                    _row3[column] = value;
                }
                else
                {
                    throw new ColumnNotFoundException(column);
                }                
            }
        }

        public override void Add(string column, object value)
        {
            if (Columns.Contains(column))
            {
                throw new ArgumentException("An element with the same key already exists in the Row.");
            }

            _row3 = _row3 ?? new DictionaryRow();

            _row3.Add(column, value);
        }

        public override void Remove(string column)
        {
            _row1 = new WithoutColumnsRow(_row1, new[] {column});
            _row2 = new WithoutColumnsRow(_row2, new[] {column});

            _row3?.Remove(column);
        }
        #endregion
    }
}