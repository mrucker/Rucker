using System;

namespace Rucker.Data
{
    public static class UriExtensions
    {
        public static BaseUri ToUri(this string uriString)
        {
            var uri = new BaseUri(uriString);

            if (uri.Scheme.ToLower() == "table")
            {
                return new TableUri(uriString);
            }

            if (uri.Scheme.ToLower() == "database")
            {
                return new DatabaseUri(uriString);
            }

            if (uri.Scheme.ToLower() == "file")
            {
                return new FileUri(uriString);
            }

            return uri;
        }

        public static T ToUri<T>(this string uriString) where T: BaseUri
        {
            if (uriString == null)
            {
                return null;
            }

            if (typeof(T) == typeof(TableUri))
            {
                return new TableUri(uriString) as T;
            }

            if (typeof(T) == typeof(DatabaseUri))
            {
                return new DatabaseUri(uriString) as T;
            }

            if (typeof (T) == typeof(FileUri))
            {
                return new FileUri(uriString) as T;
            }

            if (typeof (T) == typeof (DirectoryUri))
            {
                return new DirectoryUri(uriString) as T;
            }

            throw new Exception("Not a recogiznied Uri type");
        }

        public static void InjectionCheck(this TableUri tableUri)
        {
            using (var connection = tableUri.DatabaseUri.ToSqlQuerier().BeginConnection())
            {
                connection.TableExists(tableUri.SchemaName, tableUri.TableName);
            }
        }

        public static SqlQuerier ToSqlQuerier(this DatabaseUri databaseUri)
        {
            return new SqlQuerier(databaseUri);
        }
    }
}