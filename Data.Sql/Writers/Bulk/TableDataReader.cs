using System;
using System.Linq;
using Data.Core;

namespace Data.Sql
{
    public sealed class TableDataReader: DataReader
    {
        #region Consts
        private static readonly string[] BooleanTextRepresentations = { "1", "0", "-1" };
        #endregion

        #region Fields
        private bool _isClosed;
        private int _currentRow = -1;        
        #endregion

        #region Properties
        public IRow[] Rows { get; }
        public string[] Cols { get; }
        #endregion

        #region Constructor
        public TableDataReader(ITable table)
        {
            Rows = table.Rows.ToArray();
            Cols = table.Columns.ToArray();
        }
        #endregion

        #region DataReader Implementation
        public override int Count => Rows.Length;
        #endregion

        #region IDataReader Implementation
        public override bool Read()
        {
            return ++_currentRow < Rows.Length;
        }

        public override void Close()
        {
            Dispose();
        }

        public override int GetOrdinal(string name)
        {
            return Array.IndexOf(Cols, name);
        }

        public override bool IsClosed => _isClosed;

        public override int FieldCount => Cols.Length;

        public override object GetValue(int currentCol)
        {
            var row = Rows[_currentRow];
            var col = Cols[currentCol];
            var val = (row as ObjectRow)?.RawValue(col) ?? row[col];
            
            if (BooleanTextRepresentations.Contains(val as string))
            {
                return val.To<int>();
            }

            return val;
        }
        #endregion

        #region Dispose Pattern
        protected override void Dispose(bool disposing)
        {
            _isClosed = true;

            base.Dispose(disposing);
        }
        #endregion
    }
}