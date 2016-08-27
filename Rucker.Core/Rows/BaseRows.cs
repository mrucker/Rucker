using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Rucker.Core
{
    public class BaseRows: IRows
    {
        #region ToRows
        public static IRows JsonsToRows(IEnumerable<string> jsons)
        {
            return jsons.Select(BaseRow.JsonToRow).ToRows();
        }

        public static IRows ObjectsToRows(IEnumerable<object> objects)
        {
            return objects.Select(BaseRow.ObjectToRow).ToRows();
        }

        public static IRows DataTableToRows(DataTable dataTable)
        {
            //this isn't the cleanest implementation but it was done this way primarily for performance
            var colNames       = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).Distinct().ToArray();
            var colDefinitions = colNames.Select(n => new ObjectRow.PropertyDefinition(n, Type(dataTable.Columns[n]))).ToArray();

            return dataTable.Rows.Cast<DataRow>().Select(r => BaseRow.DataRowToRow(r, colDefinitions)).ToRows();
        }

        public static IRows DataReaderToRows(IDataReader reader)
        {
            return ReadDataReader(reader).ToRows();
        }
        #endregion

        #region RowsTo
        public static IEnumerable<T> RowsToObjects<T>(IEnumerable<IRow> rows)
        {
            return rows.Select(BaseRow.RowToObject<T>);
        }

        public static IEnumerable<string> RowsToJsons(IEnumerable<IRow> rows)
        {
            return rows.Select(BaseRow.RowToJson);
        }
        #endregion

        #region Fields
        private readonly List<IRow> _rows;
        #endregion

        #region Constructors
        public BaseRows(params IRow[] rows): this(rows.AsEnumerable())
        { }

        public BaseRows(IEnumerable<IRow> rows)
        {
            _rows = (rows as BaseRows)?._rows.ToList() ?? rows.ToList();
        }
        #endregion

        #region Public Methods
        public void Add(IRow row)
        {
            _rows.Add(row);
        }
        #endregion

        #region IEnumerable<IRow> Implementation
        public IEnumerator<IRow> GetEnumerator()
        {
            return _rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Private Methods
        private static IEnumerable<IRow> ReadDataReader(IDataReader reader)
        {
            using (reader)
            {
                while (reader.Read())
                {
                    yield return BaseRow.DataReaderToRow(reader);
                }
            }
        }
        #endregion

        private static Type Type(DataColumn column)
        {
            return (column.DataType.IsValueType && column.AllowDBNull) ? typeof(Nullable<>).MakeGenericType(column.DataType) : column.DataType;
        }
    }
}
