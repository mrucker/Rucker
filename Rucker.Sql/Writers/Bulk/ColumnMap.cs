namespace Rucker.Sql
{
    public class ColumnMap
    {
        public ColumnMap(string source, string target)
        {
            Source = source;
            Target = target;
        }

        public ColumnMap(string column): this(column, column)
        { }

        public string Source { get; set; }
        public string Target { get; set; }
    }
}