using Data.Core;

namespace Data.Entities
{
    public static class UriExtensions
    {
        public static DbQuerier ToDbQuerier(this DatabaseUri databaseUri)
        {
            return new DbQuerier(databaseUri);
        }
    }
}