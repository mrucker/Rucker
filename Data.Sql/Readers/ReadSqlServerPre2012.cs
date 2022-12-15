using System;
using System.Linq;
using Data.Core;

namespace Data.Sql
{
    ///<summary>
    /// This class takes a query and then, using windowing functions, will page over the results of that query.
    /// </summary>
    /// <remarks>
    /// WARNING -- This class doesn't protect against sql injection even though SqlParameters are used -- WARNING
    /// </remarks>
    public class ReadSqlServerPre2012 : Disposable, IRead<IRows>
    {
        #region Fields
        private readonly IQuerier _querier;
        private readonly string _query;
        private readonly string _orderBy;
        private readonly string _cte;
        private readonly object[] _parameters;
        private readonly int _timeout;
        private readonly Lazy<int> _lazySize; 
        #endregion

        #region Queries
        private static string CountQuery
        {
            get { return "{0} SELECT COUNT(1) FROM ( {1} ) as sub"; }
        }

        /// <remarks>
        /// The OFFSET...FETCH... commands only works on SQL Server 2012. Which is why we have this elaborate work around.
        /// This is the generally recommended approach according to SO (http://stackoverflow.com/a/2244353/1066291).
        /// </remarks>
        private static string PageQuery
        {
            get
            {
                return @"
                            {0}
                        	SELECT *
					        FROM 
					        (
						        SELECT *, ROW_NUMBER() OVER (ORDER BY {2}) AS RowNumber
						        FROM 
                                (
                                    {1} 
                                ) sub1
					        )sub2
                            WHERE RowNumber BETWEEN {3} AND {4}
                        ";

                        //This seems as performant as the above query
                        //                return @"
                        //                        	SELECT TOP({2}) *
                        //					        FROM 
                        //					        (
                        //						        SELECT *, ROW_NUMBER() OVER (ORDER BY [{3}]) AS RowNumber
                        //						        FROM 
                        //                                (
                        //                                    {0} 
                        //                                ) sub1
                        //					        )sub2
                        //                          WHERE {1} < RowNumber AND RowNumber < {2}
                        //                          ORDER BY [{3}]                                
                        //                        ";

                        //This appears to be slower than the above two options
                        //                return @"
                        //                            DECLARE @Skip int = {1} + 1
                        //                            DECLARE @Take int = {2}
                        //                            DECLARE @FirstId int
                        //
                        //
                        //                            SELECT   TOP (@Skip)
                        //                                     @FirstId = [{3}]
                        //                            FROM     ({0}) SubQuery
                        //                            ORDER BY [{3}]
                        //
                        //                            SELECT   TOP (@Take) *
                        //                            FROM     ({0}) SubQuery
                        //                            WHERE    [{3}] >= @FirstId
                        //                            ORDER BY [{3}]
                        //                        ";
            }
        }

        private static string FullQuery
        {
            get 
            { 
                return @"
                            {0} 
                            {1}
                        "; 
            }
        }
        #endregion

        #region Constructors
        public ReadSqlServerPre2012(IQuerier querier, string query, int timeout = 30, params object[] parameters): this(querier, query, "Id", timeout, parameters)
        { }

        /// <remarks>
        /// In order for this to work correctly orderBy has to be distinct for each row otherwise rows could be lost between pages.
        /// </remarks>
        public ReadSqlServerPre2012(IQuerier querier, string query, string orderBy, int timeout = 30, params object[] parameters): this(querier, null, query, orderBy, timeout, parameters)
        { }

        public ReadSqlServerPre2012(IQuerier querier, string cte, string query, string orderBy, int timeout = 30, params object[] parameters)
        {
            //At this layer The sqlQuery is assumed to be injection free... (famous last words)
            _querier    = querier;
            _cte        = cte ?? "";
            _query      = query;
            _orderBy    = orderBy;
            _timeout    = timeout;
            _parameters = parameters;
            _lazySize   = new Lazy<int>(DetermineSize, true);
        }
        #endregion

        #region Public Methods
        public int Size()
        {
            DisposeCheck();

            return _lazySize.Value;
        }

        public IRows Read(int skip, int take)
        {
            DisposeCheck();

            if (skip > Size()) throw new ArgumentException("More rows were skipped than the query returns");

            using (var querier = _querier.BeginConnection())
            {
                return querier.SqlQuery(FinalQuery(skip, take), _parameters, _timeout).WithoutColumns(new []{"RowNumber"});
            }
        }
        #endregion

        #region Private Methods
        private string FinalQuery(int skip, int take)
        {   
            return skip == 0 && take >= Size() ? string.Format(FullQuery, _cte, _query) : string.Format(PageQuery, _cte, _query, _orderBy, skip + 1, skip + take);
        }

        private int DetermineSize()
        {
            using (var querier = _querier.BeginConnection())
            {
                return querier.SqlQuery<int>(string.Format(CountQuery, _cte, _query), _parameters, _timeout).Single();
            }
        }

        #endregion
    }
}