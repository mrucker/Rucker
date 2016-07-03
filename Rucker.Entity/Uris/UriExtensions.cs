namespace Rucker.Data
{
    public static class UriExtensions
    {
        public static DbQuerier ToDbQuerier(this DatabaseUri databaseUri)
        {
            return new DbQuerier(databaseUri);
        }
    }
}