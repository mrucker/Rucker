using System.Data;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using Rucker.Dispose;
using Rucker.Extensions;

namespace Rucker.Data
{
    public class DbQuerierConnection: Disposable,  IQuerierConnection
    {
        #region Fields
        private readonly DbContext _dbContext;
        #endregion

        #region Properties

        public string Server
        {
            get
            {
                DisposeCheck();
                return _dbContext.Database.Connection.DataSource;
            }
        }

        public string Database
        {
            get
            {
                DisposeCheck();
                return _dbContext.Database.Connection.Database.IsNullOrEmpty() ? "master" : _dbContext.Database.Connection.Database;
            }
        }

        public DbContext DbContext
        {
            get
            {
                DisposeCheck();
                return _dbContext;
            }
        }
        #endregion

        #region Constructors
        public DbQuerierConnection(string nameOrConnectionString)
        {
            _dbContext = new DbContext(nameOrConnectionString);
        }
        #endregion

        #region Public Methods
        public IEnumerable<T> SqlQuery<T>(string sql, IEnumerable<object> parameters = null, int timeout = 30)
        {
            DisposeCheck();

            (_dbContext as IObjectContextAdapter).ObjectContext.CommandTimeout = timeout;

            return parameters == null ? _dbContext.Database.SqlQuery<T>(sql) : _dbContext.Database.SqlQuery<T>(sql, parameters.ToArray());
        }

        public IRows SqlQuery(string sql, IEnumerable<object> parameters = null, int timeout = 30)
        {
            DisposeCheck();

            using (var sqlCommand = _dbContext.Database.Connection.CreateCommand())
            {
                sqlCommand.CommandText    = sql;
                sqlCommand.CommandTimeout = timeout;

                if (sqlCommand.Connection.State != ConnectionState.Open)
                {
                    sqlCommand.Connection.Open();
                }

                if (parameters != null)
                {
                    sqlCommand.Parameters.AddRange(parameters.ToArray());
                }

                return BaseRows.DataReaderToRows(sqlCommand.ExecuteReader()).ToRows();
            }
        }

        public void SqlCommand(string sql, IEnumerable<object> parameters = null, int timeout = 30)
        {
            DisposeCheck();

            (_dbContext as IObjectContextAdapter).ObjectContext.CommandTimeout = timeout;

            var parameterArray = parameters?.ToArray();

            if (parameterArray != null)
                _dbContext.Database.ExecuteSqlCommand(sql, parameterArray);
            else
                _dbContext.Database.ExecuteSqlCommand(sql);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}