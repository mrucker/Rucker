using System.Collections.Generic;

namespace Data.Sql
{
    public interface IBulk
    {
        string Schema { get; }
        string Table { get; }
        DataReader DataReader { get; }
        IEnumerable<ColumnMap> ColumnMaps { get; } 
    }
}