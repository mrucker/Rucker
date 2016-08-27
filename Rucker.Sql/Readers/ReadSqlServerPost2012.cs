using System;
using System.Linq;
using Rucker.Data;
using Rucker.Dispose;

namespace Rucker.Sql
{
    /// <remarks>
    /// WARNING -- This class doesn't protect against sql injection even though SqlParameters are used -- WARNING
    /// </remarks>>
    public class ReadSqlServerPost2012: Disposable, IRead<IRows>
    {
        #region Fields
        private readonly IQuerier _querier;
        private readonly string _sql;
        private readonly object[] _parameters;
        private readonly int _timeout;
        #endregion

        #region Queries
        public string CountQuery => "SELECT COUNT(*) FROM ( {0} ) as sub";

        public string PageQuery => "SELECT * FROM ( {0} ) as sub ORDER BY Id OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY ";

        #endregion

        #region Constructor
        public ReadSqlServerPost2012(IQuerier querier, string sql, int timeout = 30, params object[] parameters)
        {
            //At this layer the query is assumed to be injection free... (famous last words)
            _querier    = querier;
            _sql        = sql;
            _timeout    = timeout;
            _parameters = parameters;
        }
        #endregion

        #region Public Methods
        public int Size()
        {
            DisposeCheck();

            var finalQuery = string.Format(CountQuery, _sql);

            using (var querier = _querier.BeginConnection())
            {
                return querier.SqlQuery<int>(finalQuery, _parameters, _timeout).First();
            }
        }

        /// <remarks>
        /// The OFFSET...FETCH... command only works on SQL Server 2012
        /// </remarks>
        public IRows Read(int skip, int take)
        {
            DisposeCheck();

            int size = Size();

            if (size == 0) throw new Exception("You tried to read from a sql query that returns nothing");
            if (skip > size) throw new ArgumentException("You tried to skip more rows than the sql query returns");

            string finalQuery;

            if (skip == 0 && take == size)
            {
                finalQuery = _sql;
            }
            else
            {
                if (take == 0) take = size - skip;
                finalQuery = string.Format(PageQuery, _sql, skip, take);
            }

            using (var querier = _querier.BeginConnection())
            {
                return querier.SqlQuery(finalQuery, _parameters, _timeout);
            }
        }
        #endregion
    }
}