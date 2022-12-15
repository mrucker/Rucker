using System;
using System.Linq;
using System.Data;
using System.Reflection;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Collections.Generic;
using Data.Core;

namespace Data.Sql
{
    public class SqlQuerierConnection: Disposable, IQuerierConnection
    {
        #region Fields
        private readonly SqlConnection _sqlConnection;
        #endregion

        #region Properties
        public string Server
        {
            get
            {
                DisposeCheck();
                return _sqlConnection.DataSource;
            }
        }

        public string Database
        {
            get
            {
                DisposeCheck();
                return _sqlConnection.Database;
            }
        }


        public SqlConnection SqlConnection
        {
            get
            {
                DisposeCheck();
                return _sqlConnection;
            }
        }
        #endregion

        #region Constructors
        public SqlQuerierConnection(DatabaseUri databaseUri): this(databaseUri.ServerName, databaseUri.DatabaseName)
        { }

        public SqlQuerierConnection(string server, string database): this($"Server={server}; Database={database}; Integrated Security=True;")
        { }

        public SqlQuerierConnection(string nameOrConnectionString)
        {
            var sqlConnection = new SqlConnection(QuerierUtility.ConnectionString(nameOrConnectionString));
            
            if (sqlConnection.State == ConnectionState.Closed)
            {
                sqlConnection.Open();
            }

            _sqlConnection = sqlConnection;
        }
        #endregion

        #region Public Methods
        public IEnumerable<T> SqlQuery<T>(string sql, IEnumerable<object> parameters = null, int timeout = 30)
        {
            DisposeCheck();

            var sqlParameters = parameters?.Cast<SqlParameter>();

            var dataTable = ExecuteQuery(sql, sqlParameters, timeout);

            return BaseRows.RowsToObjects<T>(BaseRows.DataTableToRows(dataTable));
        }

        public IRows SqlQuery(string sql, IEnumerable<object> parameters = null, int timeout = 30)
        {
            DisposeCheck();

            var sqlParameters = parameters?.Cast<SqlParameter>();

            var dataTable = ExecuteQuery(sql, sqlParameters, timeout);

            return BaseRows.DataTableToRows(dataTable).ToRows();
        }

        public void SqlCommand(string sql, IEnumerable<object> parameters = null, int timeout = 30)
        {
            DisposeCheck();

            var sqlParameters = parameters?.Cast<SqlParameter>();

            ExecuteNonQuery(sql, sqlParameters, timeout);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sqlConnection.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Private Methods
        private void ExecuteNonQuery(string query, IEnumerable<SqlParameter> parameters = null, int timeout = 30)
        {
            using (var sqlCommand = _sqlConnection.CreateCommand())
            {
                sqlCommand.CommandText = query;
                sqlCommand.CommandTimeout = timeout;

                if (parameters != null)
                {
                    sqlCommand.Parameters.AddRange(parameters.ToArray());
                }

                sqlCommand.ExecuteNonQuery();
            }
        }

        private DataTable ExecuteQuery(string query, IEnumerable<SqlParameter> parameters = null, int timeout = 30)
        {
            var sqlCommand = _sqlConnection.CreateCommand();

            try
            {
                sqlCommand.CommandText    = query;
                sqlCommand.CommandTimeout = timeout;

                if (parameters != null)
                {
                    sqlCommand.Parameters.AddRange(parameters.ToArray());
                }

                var dataAdapter = new SqlDataAdapter(sqlCommand);
                var dataTable   = new DataTable();
                
                dataAdapter.Fill(dataTable);

                return dataTable;
            }
            finally
            {
                sqlCommand.Parameters.Clear();
                sqlCommand.Dispose();
            }
        }

        internal static IEnumerable<T> DataTableToObjects<T>(DataTable dataTable)
        {
            var dataRows = dataTable.Rows.Cast<DataRow>();
            
            return DataRowsToObjects<T>(dataRows).ToArray();
        }

        private static IEnumerable<T> DataRowsToObjects<T>(IEnumerable<DataRow> dataRows)
        {
            var typeOfT = typeof(T);
            var setters = typeOfT.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.SetMethod != null).ToArray();
            
            return dataRows.Select(dr => DataRowToObject(typeOfT, setters, dr)).Cast<T>();
        }

        private static object DataRowToObject(Type type, IEnumerable<PropertyInfo> setters, DataRow dataRow)
        {
            setters = setters.ToArray();

            if (!setters.Any() && (type.IsValueType || type == typeof(string)))
            {
                return (dataRow[0] is DBNull) ? null : dataRow[0];
            }

            var obj = Activator.CreateInstance(type);

            foreach (var property in setters)
            {
                var value          = dataRow[property.Name];
                var convertedValue = ConvertValue(property.PropertyType, value);

                property.SetValue(obj, convertedValue);
            }

            return obj;
        }

        private static object ConvertValue(Type targetType, object sourceValue)
        {
            if (sourceValue is DBNull) return null;

            var convertToTargetType = TypeDescriptor.GetConverter(targetType);

            if (!convertToTargetType.CanConvertFrom(sourceValue.GetType()) && targetType.Name != "String" && targetType != sourceValue.GetType())
            {
                throw new Exception("Cannot implicitly convert from " + targetType.Name + " to " + sourceValue.GetType().Name);
            }

            if (!convertToTargetType.CanConvertFrom(sourceValue.GetType()) && targetType.Name == "String")
            {
                return sourceValue.ToString();
            }

            if (!convertToTargetType.CanConvertFrom(sourceValue.GetType()) && targetType == sourceValue.GetType())
            {
                return sourceValue;
            }

            return convertToTargetType.ConvertFrom(sourceValue);
        }
        #endregion
    }
}