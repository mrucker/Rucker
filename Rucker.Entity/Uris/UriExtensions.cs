using Rucker.Core;

namespace Rucker.Entities
{
    public static class UriExtensions
    {
        public static DbQuerier ToDbQuerier(this DatabaseUri databaseUri)
        {
            return new DbQuerier(databaseUri);
        }
    }
}