using System;
using System.Linq;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Rucker.Core
{
    /// <summary>
    /// A basic interface to run sql queries against a database connection.
    /// </summary>
    public interface IQuerierConnection: IDisposable
    {
        /// <summary>
        /// The name of the Server this Querier will run against.
        /// </summary>
        string Server { get; }

        /// <summary>
        /// The name of the Database this Querier will run against.
        /// </summary>
        string Database { get; }

        /// <summary>
        /// Executes a SQL query and stores the results in the given type T.
        /// </summary>
        IEnumerable<T> SqlQuery<T>(string sql, IEnumerable<object> parameters = null, int timeout = 30);
        
        /// <summary>
        /// Executes a SQL query and stores the results in an IRows interface.
        /// </summary>
        IRows SqlQuery(string sql, IEnumerable<object> parameters = null, int timeout = 30);
        
        /// <summary>
        /// Executes a SQL query that has no returnable output.
        /// </summary>
        void SqlCommand(string sql, IEnumerable<object> parameters = null, int timeout = 30);
    }

    [SuppressMessage("ReSharper", "ConvertPropertyToExpressionBody")]
    public static class IQueryConnectionExtensions
    {
        #region Private Enum
        private enum TableType
        {
            GlobalTemp,
            LocalTemp,
            Default
        }
        #endregion

        #region Fields
        private static readonly List<string> ExistingTables = new List<string>();
        #endregion

        #region Queries
        private static string GlobalTempTableExistsQuery
        {
            get
            {
                return @"SELECT 1
                           FROM tempdb.INFORMATION_SCHEMA.TABLES 
                          WHERE TABLE_SCHEMA = 'dbo' 
                            AND TABLE_NAME = @tableName";
            }
        }

        private static string LocalTempTableExistsQuery
        {
            get
            {
                return @"SELECT 1
                           FROM tempdb.INFORMATION_SCHEMA.TABLES 
                          WHERE TABLE_SCHEMA = 'dbo' 
                            AND TABLE_NAME LIKE @tableName + '_%'";
            }
        }

        private static string DefaultTableExistsQuery
        {
            get
            {
                return @"SELECT 1
                           FROM INFORMATION_SCHEMA.TABLES 
                          WHERE TABLE_SCHEMA = @tableSchema 
                            AND TABLE_NAME = @tableName";
            }
        }

        private static string GlobalTempColumnsExistQuery
        {
            get
            {
                return @"SELECT COLUMN_NAME
                           FROM tempdb.INFORMATION_SCHEMA.COLUMNS
                          WHERE TABLE_SCHEMA = 'dbo' 
                            AND TABLE_NAME = @tableName
                            AND COLUMN_NAME IN ({0})";
            }
        }

        private static string LocalTempColumnsExistQuery
        {
            get
            {
                return @"SELECT COLUMN_NAME
                           FROM tempdb.INFORMATION_SCHEMA.COLUMNS
                          WHERE TABLE_SCHEMA = 'dbo' 
                            AND TABLE_NAME LIKE @tableName + '_%'
                            AND COLUMN_NAME IN ({0})";
            }
        }

        private static string DefaultColumnsExistQuery
        {
            get
            {
                return @"SELECT COLUMN_NAME
                           FROM INFORMATION_SCHEMA.COLUMNS
                          WHERE TABLE_SCHEMA = @tableSchema 
                            AND TABLE_NAME = @tableName
                            AND COLUMN_NAME IN ({0})";
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns true if the table exists. Otherwise, it returns false.
        /// All input values are parameterized so this method is safe against SQL Injection.
        /// </summary>
        /// <param name="connection">The context we are querying</param>
        /// <param name="schema">The schema the table belongs to</param>
        /// <param name="table">The table we want to see if the columns exist on</param>
        public static void TableExists(this IQuerierConnection connection, string schema, string table)
        {
            schema = CleanIdentifier(schema);
            table  = CleanIdentifier(table);

            if (IsExistingTable(connection, schema, table)) return;

            var parameters = new object[]
            {
                new SqlParameter("@tableSchema", schema),
                new SqlParameter("@tableName", table)
            };

            if (connection.SqlQuery<int>(GetTableExistsQuery(GetTableType(table)), parameters).None())
            {
                throw new Exception($"{schema}.{table} couldn't be found in {connection.Database}");
            }

            AddExistingTable(connection, schema, table);
        }

        /// <summary>
        /// Returns true if all columns exist on the given table. Otherwise, it returns false.
        /// All input values are parameterized so this method is safe against SQL Injection.
        /// </summary>
        /// <param name="querierConnection">The context we are querying</param>
        /// <param name="schema">The schema the table belongs to</param>
        /// <param name="table">The table we want to see if the columns exist on</param>
        /// <param name="cols">A collection of columns to look for on the given table</param>        
        public static void ColumnsExist(this IQuerierConnection querierConnection, string schema, string table, params string[] cols)
        {
            schema = CleanIdentifier(schema);
            table  = CleanIdentifier(table);

            var colParameters      = cols.Select((name, i) => new SqlParameter("@col" + i, name.Trim())).ToArray();
            var colParamterNames   = colParameters.Select(p => p.ParameterName);
            var tableParameters    = new[] { new SqlParameter("@tableSchema", schema), new SqlParameter("@tableName", table) };
            var parameterizedQuery = string.Format(GetColumnsExistQuery(GetTableType(table)), string.Join(",", colParamterNames));
            var existingColumns    = querierConnection.SqlQuery<string>(parameterizedQuery, colParameters.Concat(tableParameters)).ToArray();

            if (cols.Any(c => !existingColumns.Contains(c)))
            {
                throw new Exception($"Columns ({cols.Where(c => !existingColumns.Contains(c)).Cat(",")}) weren't found on table {querierConnection.Database}.{schema}.{table}");
            }
        }

        /// <summary>
        /// Returns true if the table exists and all columns exist on the table. Otherwise, it returns false.
        /// All input values are parameterized so this method is safe against SQL Injection.
        /// </summary>
        /// <param name="connection">The context we are querying</param>
        /// <param name="schema">The schema the table belongs to</param>
        /// <param name="table">The table we want to see if the columns exist on</param>
        /// <param name="columnNames">A collection of columns to look for on the given table</param>
        /// <returns>True if all columns exist otherwise false</returns>
        public static void TableAndColumnsExist(this IQuerierConnection connection, string schema, string table, params string[] columnNames)
        {
            TableExists(connection, schema, table);
            ColumnsExist(connection, schema, table, columnNames);
        }
        #endregion

        #region Private Methods
        private static string CleanIdentifier(string identifier)
        {
            return identifier.Replace("[", "").Replace("]", "");
        }

        private static TableType GetTableType(string table)
        {
            if (table.StartsWith("##")) return TableType.GlobalTemp;

            if (table.StartsWith("#")) return TableType.LocalTemp;

            return TableType.Default;
        }

        private static string GetColumnsExistQuery(TableType tableType)
        {
            if (tableType == TableType.GlobalTemp) return GlobalTempColumnsExistQuery;
            if (tableType == TableType.LocalTemp) return LocalTempColumnsExistQuery;

            return DefaultColumnsExistQuery;
        }

        private static string GetTableExistsQuery(TableType tableType)
        {
            if (tableType == TableType.GlobalTemp) return GlobalTempTableExistsQuery;
            if (tableType == TableType.LocalTemp) return LocalTempTableExistsQuery;

            return DefaultTableExistsQuery;
        }

        private static bool IsExistingTable(IQuerierConnection querierConnection, string schema, string table)
        {
            return ExistingTables.Contains($"[{querierConnection.Database}].[{schema}].[{table}]");
        }

        private static void AddExistingTable(IQuerierConnection querierConnection, string schema, string table)
        {
            ExistingTables.Add($"[{querierConnection.Database}].[{schema}].[{table}]");
        }
        #endregion
    }
}