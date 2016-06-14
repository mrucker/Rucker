using System.Linq;

namespace Rucker.Data
{
    public static class BulkExtensions
    {
        public static IRow CurrentRow(this IBulk bulk)
        {
            var row = new DictionaryRow();

            foreach (var column in bulk.ColumnMaps.Select(m => m.Target))
            {
                row.Add(column, bulk.DataReader.GetValue(bulk.DataReader.GetOrdinal(column)).ToString());
            }

            return row;
        }

        public static int RowCount(this IBulk bulk)
        {
            return bulk.DataReader.Count;
        }
    }
}