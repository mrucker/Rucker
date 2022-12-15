using System.Collections.Generic;
using System.Linq;

namespace Data.Core
{
    public class WithoutColumnsRow: BaseRow
    {
        private readonly IRow _row;
        private readonly IEnumerable<string> _withoutColumns;

        public WithoutColumnsRow(IRow row, IEnumerable<string> withoutColumns)
        {
            withoutColumns = withoutColumns.ToArray();

            _withoutColumns = (row as WithoutColumnsRow)?._withoutColumns.Concat(withoutColumns).ToArray() ?? withoutColumns.ToArray();
            _row            = (row as WithoutColumnsRow)?._row ?? row;
        }

        #region IRow Implementation
        public override IEnumerable<string> Values { get { return Columns.Select(column => this[column]); } }
        public override IEnumerable<string> Columns => _row.Columns.Except(_withoutColumns);

        public override string this[string column]
        {
            get
            {
                AssertHasColumn(column);

                return _row[column];
            }
            set
            {
                AssertHasColumn(column);

                _row[column] = value;
            }
        }

        public override void Add(string column, object value)
        {
            _row.Add(column, value);
        }

        public override void Remove(string column)
        {
            _row.Remove(column);
        }
        #endregion
    }
}