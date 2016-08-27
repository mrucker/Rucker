using System.Linq;
using System.Collections.Generic;
using Rucker.Data;

namespace Rucker.Sql
{
    public class Bulk: IBulk
    {
        public Bulk(string name, IEnumerable<IRow> rows): this(new Table(name, rows))
        { }

        public Bulk(string schema, string name, IEnumerable<IRow> rows): this(new Table(schema, name, rows))
        { }

        public Bulk(ITable table)
        {
            Schema     = table.Schema;
            Table      = table.Name;
            DataReader = new TableDataReader(table);
            ColumnMaps = table.Columns.Select(c => new ColumnMap(c));
        }

        public string Schema { get; }
        public string Table { get; }
        public DataReader DataReader { get; }
        public IEnumerable<ColumnMap> ColumnMaps { get; }
    }
}