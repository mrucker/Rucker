using System.Linq;
using System.Data.SqlClient;
using System.Collections.Generic;
using Data.Core;

namespace Data.Sql
{
    public class DatabaseService: Disposable
    {
        #region Public Methods
        public IEnumerable<string> GetDatabases(string server, string database)
        {
            if (server.IsNullOrEmpty()) return Enumerable.Empty<string>();

            using (var repository = new SqlQuerierConnection(server, database ?? "master"))
            {
                return repository.SqlQuery<string>("SELECT name FROM sys.databases WHERE name NOT IN ('master','tempdb','model','msdb','mssqlweb') ORDER BY name");
            }
        }

        public IEnumerable<string> GetDatabases(string nameOrConnectionString)
        {
            if (nameOrConnectionString.IsNullOrEmpty()) return Enumerable.Empty<string>();

            using (var repository = new SqlQuerierConnection(nameOrConnectionString))
            {
                return repository.SqlQuery<string>("SELECT name FROM sys.databases WHERE name NOT IN ('master','tempdb','model','msdb','mssqlweb') ORDER BY name");
            }
        }

        public IEnumerable<string> GetSchemas(string server, string database)
        {
            if (server.IsNullOrEmpty() || database.IsNullOrEmpty()) return Enumerable.Empty<string>();

            using (var connection = new SqlQuerierConnection(server, database))
            {
                return connection.SqlQuery<string>("SELECT DISTINCT TABLE_SCHEMA FROM INFORMATION_SCHEMA.COLUMNS");
            }
        }

        public IEnumerable<string> GetTables(string server, string database, string schema)
        {
            if (server.IsNullOrEmpty() || database.IsNullOrEmpty() || schema.IsNullOrEmpty()) return Enumerable.Empty<string>();
            
            using (var connection = new SqlQuerierConnection(server, database))
            {
                return connection.SqlQuery<string>("SELECT Distinct TABLE_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @schemaName", new [] {new SqlParameter("@schemaName", schema)});
            }
        }

        public IEnumerable<string> GetTables(string nameOrConnectionString, string schema)
        {
            if (nameOrConnectionString.IsNullOrEmpty()) return Enumerable.Empty<string>();

            using (var connection = new SqlQuerierConnection(nameOrConnectionString))
            {
                return connection.SqlQuery<string>("SELECT Distinct TABLE_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @schemaName", new[] { new SqlParameter("@schemaName", schema) });
            }
        }

        public IEnumerable<MetaColumn> GetColumns(TableUri tableUri)
        {
            return tableUri == null ? Enumerable.Empty<MetaColumn>() : GetColumns(tableUri.DatabaseUri.ServerName, tableUri.DatabaseUri.DatabaseName, tableUri.TableName);
        }

        public IEnumerable<MetaColumn> GetColumns(string server, string database, string table)
        {
            if (server.IsNullOrEmpty() || database.IsNullOrEmpty() || table.IsNullOrEmpty()) return Enumerable.Empty<MetaColumn>();

            using (var connection = new SqlQuerierConnection(server, database))
            {
                return connection.SqlQuery<MetaColumn>(TableColumnMetaQuery(table) + " ORDER BY ORDINAL_POSITION", new[] { new SqlParameter("@tableName", table) });
            }
        }

        public IEnumerable<MetaColumn> GetColumns(string nameOrConnectionString, string table)
        {
            if (nameOrConnectionString.IsNullOrEmpty()) return Enumerable.Empty<MetaColumn>();

            using (var connection = new SqlQuerierConnection(nameOrConnectionString))
            {
                return connection.SqlQuery<MetaColumn>(TableColumnMetaQuery(table) + " ORDER BY ORDINAL_POSITION", new[] { new SqlParameter("@tableName", table) });
            }
        }

        #endregion

        #region Private Methods
        private static string TableColumnMetaQuery(string table)
        {
            const string longTermTableQuery = @"SELECT COLUMN_NAME AS Name, DATA_TYPE AS TSqlType, CASE WHEN IS_NULLABLE = 'YES' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS Nullable
                                                 FROM INFORMATION_SCHEMA.COLUMNS
                                                WHERE TABLE_NAME = @tableName";

            const string localTempTableQuery = @"SELECT COLUMN_NAME AS Name, DATA_TYPE AS TSqlType, CASE WHEN IS_NULLABLE = 'YES' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS Nullable
                                                   FROM TEMPDB.INFORMATION_SCHEMA.COLUMNS
                                                  WHERE TABLE_NAME = @tableName";

            const string globalTempTableQuery = @"SELECT COLUMN_NAME AS Name, DATA_TYPE AS TSqlType, CASE WHEN IS_NULLABLE = 'YES' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS Nullable
                                                    FROM TEMPDB.INFORMATION_SCHEMA.COLUMNS
                                                   WHERE TABLE_NAME = @tableName";

            return table.StartsWith("##") ? globalTempTableQuery : table.StartsWith("#") ? localTempTableQuery : longTermTableQuery;
        }
        #endregion
    }
}