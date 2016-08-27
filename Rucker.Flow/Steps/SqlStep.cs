using System;
using System.Data.SqlClient;
using Rucker.Core;

namespace Rucker.Flow
{
    public class SqlStep: Step
    {        
        #region Properties
        public string Query { get; }
        public int Timeout { get; }
        public object[] Params { get; }
        public IQuerier Querier { get; }
        #endregion

        #region Constructor
        public SqlStep(IQuerier querier, string query, int timeout, params object[] @params)
        {
            Query   = query;
            Params  = @params;
            Timeout = timeout;
            Querier = querier;
        }
        #endregion

        #region Overrides
        protected override void Initializing()
        {
            
        }

        protected sealed override void Processing()
        {
            using(Tracker.Whole(1))
            using(Tracker.Piece())
            {
                using (var connection = Querier.BeginConnection())
                {
                    try
                    {
                        connection.SqlCommand(Query, Params, Timeout);
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"The following Query Failed: {Query}", ex);
                    }
                }
            }
        }
        #endregion
    }
}