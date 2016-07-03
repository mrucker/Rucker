using System;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Rucker.Dispose;

namespace Rucker.Data
{
    public class ReadDbContext: Disposable, IRead<IRows>
    {
        #region Fields
        private readonly DbQuerier _dbQuerier;
        private readonly int? _timeout;
        private readonly Func<DbContext, IOrderedQueryable<object>> _query;
        #endregion

        #region Constructor
        public ReadDbContext(DbQuerier dbQuerier, Func<DbContext, IOrderedQueryable<object>> query, int? timeout = null)
        {
            _dbQuerier = dbQuerier;
            _timeout   = timeout;
            _query     = query;
        }
        #endregion

        #region IRead<IEnumerable<T>> Implementation
        public IRows Read(int skip, int take)
        {
            DisposeCheck();

            using (var dbContextQuerier = _dbQuerier.BeginConnection())
            {
                var dbContext = dbContextQuerier.DbContext;

                if (_timeout.HasValue)
                {
                    ((IObjectContextAdapter) dbContext).ObjectContext.CommandTimeout = _timeout.Value;
                }

                var results = _query(dbContext).Skip(skip).Take(take).ToArray();

                if(!results.Any()) throw new Exception("No results were returned from the DbContextReader");

                return BaseRows.ObjectsToRows(results);
            }
        }

        public int Size()
        {
            DisposeCheck();

            using (var dbContextQuerier = _dbQuerier.BeginConnection())
            {
                var dbContext = dbContextQuerier.DbContext;

                if (_timeout.HasValue)
                {
                    ((IObjectContextAdapter)dbContext).ObjectContext.CommandTimeout = _timeout.Value;
                }

                return _query(dbContext).Count();
            }
        }
        #endregion        
    }
}
