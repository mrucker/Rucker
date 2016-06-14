using System.Collections.Generic;

namespace Rucker.Data
{
    public interface IBulk
    {
        string Schema { get; }
        string Table { get; }
        DataReader DataReader { get; }
        IEnumerable<ColumnMap> ColumnMaps { get; } 
    }
}