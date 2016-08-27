using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Rucker.Data;
using Rucker.Core;

namespace Rucker.Sql
{
    public class WriteBulkAndMerge : Disposable, IWrite<BulkAndMerge>
    {
        #region Private Classes
        private class BulkerAndMerger
        {
            #region Fields
            private readonly WriteBulkAndMerge _parent;
            private readonly ConnectionToSql _connection;
            #endregion

            #region Constructors
            public BulkerAndMerger(ConnectionToSql connectionToSql, WriteBulkAndMerge parent)
            {
                _connection = connectionToSql;
                _parent     = parent;
            }
            #endregion

            #region Queries
            private static string IdentityColumnsQuery(string schema, string table)
            {
                const string notTempTableQuery = "SELECT name FROM sys.identity_columns WHERE [object_id] = OBJECT_ID('{0}.{1}')";
                const string tempTableQuery    = "SELECT name FROM tempdb.sys.identity_columns WHERE [object_id] = OBJECT_ID('tempdb.{0}.{1}')";

                var param = new object[] {schema, table};
                var query = table.StartsWith("#") ? tempTableQuery : notTempTableQuery;

                return string.Format(query, param);
            }

            private static string NonIdentityColumnsQuery(string schema, string table)
            {
                const string notTempTableQuery = "SELECT name FROM sys.columns WHERE [object_id] = OBJECT_ID('{0}.{1}') AND name NOT IN ({2})";
                const string tempTableQuery    = "SELECT name FROM tempdb.sys.columns WHERE [object_id] = OBJECT_ID('tempdb.{0}.{1}') AND name NOT IN ({2})";

                var param = new object[] {schema, table, IdentityColumnsQuery(schema, table)};
                var query   = table.StartsWith("#") ? tempTableQuery : notTempTableQuery;


                return string.Format(query, param);
            }

            private string MergeQuery(IMerge merge, IEnumerable<string> sourceColumns)
            {                
                var key = merge.DestTable + merge.SourceTable;
                
                if (_parent.MergeQueries.ContainsKey(key)) return _parent.MergeQueries[key];

                _parent.MergeQueries.TryAdd(key, MergeOnClause(merge) + " " + MergeActionClause(merge, sourceColumns) + ";");

                return _parent.MergeQueries[key];
            }

            private static string MergeOnClause(IMerge merge)
            {
                return $"MERGE {merge.DestSchema}.{merge.DestTable} as [dest] USING {merge.SourceSchema}.{merge.SourceTable} as [source] ON {MatchClause(merge.MergeColumns)}";
            }

            private static string MergeActionClause(IMerge merge, IEnumerable<string> sourceColumns)
            {
                sourceColumns = sourceColumns.ToArray();
                
                if (merge.MergeAction == MergeAction.InsertOnly)
                {
                    return WhenNotMatchedInsertClause(sourceColumns);
                }

                if (merge.MergeAction == MergeAction.UpdateOrInsert)
                {
                    return WhenMatchedUpdateClause(sourceColumns) + " " + WhenNotMatchedInsertClause(sourceColumns);
                }

                if (merge.MergeAction == MergeAction.UpdateOnly)
                {
                    return WhenMatchedUpdateClause(sourceColumns);
                }

                if (merge.MergeAction == MergeAction.DeleteOnly)
                {
                    return WhenMatchedDeleteClause();
                }

                throw new Exception("Not a valid Merge Action");
            }

            private static string WhenNotMatchedInsertClause(IEnumerable<string> sourceColumns)
            {
                sourceColumns = sourceColumns.ToArray();

                var insertColumns = string.Join(",", sourceColumns);
                var insertValues = string.Join(",", sourceColumns.Select(c => "[source]" + "." + c));

                return $"WHEN NOT MATCHED BY TARGET THEN INSERT ({insertColumns}) VALUES ({insertValues})";
            }

            private static string WhenMatchedUpdateClause(IEnumerable<string> sourceColumns)
            {
                return $"WHEN MATCHED THEN UPDATE SET {string.Join(",", sourceColumns.Select(c => c + "=" + "[source]" + "." + c))}";
            }            

            private static string WhenMatchedDeleteClause()
            {
                return "WHEN MATCHED THEN DELETE";
            }

            private static string MatchDestQuery(IMerge merge)
            {                
                return $"SELECT [dest].* FROM {merge.SourceSchema}.{merge.SourceTable} AS [source] JOIN {merge.DestSchema}.{merge.DestTable} AS [dest] ON {MatchClause(merge.MergeColumns)};";
            }

            private static string MatchClause(IEnumerable<string> matchColumns)
            {
                return (matchColumns ?? Enumerable.Empty<string>()).Select(c => $"[source].{c}=[dest].{c}").DefaultIfEmpty("1=0").Cat(" AND ");
            }
            #endregion

            #region Public Methods
            public void Write(BulkAndMerge bulkAndMerge)
            {
                if (bulkAndMerge.DataReader.Count == 0) return;

                InjectionCheck(bulkAndMerge.DestSchema, bulkAndMerge.DestTable);

                CreateStageTable(bulkAndMerge, bulkAndMerge);

                BulkLoadToStage(bulkAndMerge);

                MergeStageToDest(bulkAndMerge);

                MatchStageToDest(bulkAndMerge, bulkAndMerge);

                EmptyStageTable(bulkAndMerge);
            }

            public void WritePerformance(BulkAndMerge bulkAndMerge)
            {
                var stopwatch = new Stopwatch();

                stopwatch.Start();
                InjectionCheck(bulkAndMerge.DestSchema, bulkAndMerge.DestTable);
                stopwatch.Stop();
                Console.WriteLine("InjectionCheck      : " + stopwatch.Elapsed);

                stopwatch.Restart();
                CreateStageTable(bulkAndMerge, bulkAndMerge);
                stopwatch.Stop();
                Console.WriteLine("CreateStageTable    : " + stopwatch.Elapsed);

                stopwatch.Restart();
                BulkLoadToStage(bulkAndMerge);
                stopwatch.Stop();
                Console.WriteLine("BulkLoadToStage     : " + stopwatch.Elapsed);

                stopwatch.Restart();
                MergeStageToDest(bulkAndMerge);
                stopwatch.Stop();
                Console.WriteLine("MergeStageToDest    : " + stopwatch.Elapsed);

                stopwatch.Restart();
                MatchStageToDest(bulkAndMerge, bulkAndMerge);
                stopwatch.Stop();
                Console.WriteLine("MatchStageToDest    : " + stopwatch.Elapsed);

                stopwatch.Restart();
                EmptyStageTable(bulkAndMerge);
                stopwatch.Stop();
                Console.WriteLine("EmptyStageTable     : " + stopwatch.Elapsed);
            }
            #endregion

            #region Private Methods
            private void InjectionCheck(string finalSchema, string finalTable)
            {
                _connection.QuerierConnection.TableExists(finalSchema, finalTable);
            }

            private void CreateStageTable(IBulk bulk, IMerge merge)
            {
                if (_connection.CreatedTempTables.Contains(merge.SourceTable)) return;

                var select = bulk.ColumnMaps.Select(c => c.Target).Cat(",");
                var query = $"SELECT TOP 0 {select} INTO {merge.SourceSchema}.{merge.SourceTable} FROM {merge.DestSchema}.{merge.DestTable}";

                _connection.QuerierConnection.SqlCommand(query);
                _connection.CreatedTempTables.Add(merge.SourceTable);
            }

            private void BulkLoadToStage(IBulk bulk)
            {
                using (var writeBulk = new WriteBulk(_connection.QuerierConnection, _parent.Timeout))
                {
                    writeBulk.Write(bulk);    
                }
            }

            private void MergeStageToDest(IMerge merge)
            {
                if (merge.MergeAction != MergeAction.MatchOnly)
                {
                    _connection.QuerierConnection.SqlCommand(MergeQuery(merge, NonIdentityColumns(merge.SourceSchema, merge.SourceTable)), null, _parent.Timeout);
                }
            }

            private void MatchStageToDest(IBulk bulk, IMerge merge)
            {
                if (merge.MatchAction == null) return;

                var matches = _connection.QuerierConnection.SqlQuery(MatchDestQuery(merge), null, _parent.Timeout).ToArray();

                if (matches.Length != bulk.DataReader.Count && (merge.MergeAction == MergeAction.InsertOnly || merge.MergeAction == MergeAction.UpdateOrInsert))
                {
                    throw InsertMatchException(merge, matches.Length, bulk.DataReader.Count);
                }

                if (matches.Length != 0 && merge.MergeAction == MergeAction.DeleteOnly)
                {
                    throw DeleteMatchException(merge, matches.Length, bulk.DataReader.Count);
                }

                foreach (var match in matches)
                {
                    merge.MatchAction(match);
                }
            }

            private void EmptyStageTable(IMerge merge)
            {
                _connection.QuerierConnection.SqlCommand($"DELETE FROM {merge.SourceSchema}.{merge.SourceTable}", null, _parent.Timeout);
            }
            #endregion

            #region Utility Methods
            private IEnumerable<string> NonIdentityColumns(string schema, string tableName)
            {
                return _connection.QuerierConnection.SqlQuery<string>(NonIdentityColumnsQuery(schema, tableName)).Select(s => s.ToLower().Trim()).Distinct();
            }

            private static Exception InsertMatchException(IMerge merge, int matchCount, int stageCount)
            {
                return new Exception($"Not all staging values were merged. Dest Table:({merge.DestTable}), Match On Columns:({merge.MergeColumns.Cat(", ")}), Matches Count:({matchCount}), Staged Count: ({stageCount})");
            }

            private static Exception DeleteMatchException(IMerge merge, int matchCount, int stageCount)
            {
                return new Exception($"Not all staging values were deleted. Dest Table:({merge.DestTable}), Match On Columns:({merge.MergeColumns.Cat(", ")}), Matches Count:({matchCount}), Staged Count: ({stageCount})");
            }
            #endregion
        }

        private class ConnectionToSql
        {
            public SqlQuerierConnection QuerierConnection { get; }
            public List<string> CreatedTempTables { get; }

            public ConnectionToSql(SqlQuerierConnection querierConnection)
            {
                QuerierConnection = querierConnection;
                CreatedTempTables = new List<string>();
            }
        }
        #endregion

        #region Fields
        private int Timeout { get; }
        private ConcurrentDictionary<string, string> MergeQueries { get; }
        
        private readonly SqlQuerier _sqlQuerier;
        private readonly ConcurrentBag<ConnectionToSql> _connectionsToSql;
        #endregion

        #region Constructors
        public WriteBulkAndMerge(SqlQuerier sqlQuerier, int? timeout = null)
        {
            _sqlQuerier       = sqlQuerier;            
            _connectionsToSql = new ConcurrentBag<ConnectionToSql>();
            Timeout           = timeout ?? 30;

            MergeQueries = new ConcurrentDictionary<string, string>();
        }
        #endregion

        #region IWrite Implementation
        public void Write(BulkAndMerge bulkAndMerge)
        {
            DisposeCheck();

            var connectionToSql = GetConnectionToSql();

            try
            {
                new BulkerAndMerger(connectionToSql, this).Write(bulkAndMerge);
            }
            finally
            {
                _connectionsToSql.Add(connectionToSql);
            }
        }

        public void WritePerformance(BulkAndMerge bulkAndMerge)
        {
            DisposeCheck();

            var connectionToSql = GetConnectionToSql();

            new BulkerAndMerger(connectionToSql, this).WritePerformance(bulkAndMerge);

            _connectionsToSql.Add(connectionToSql);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var connectionToSqlServer in _connectionsToSql)
                {
                    connectionToSqlServer.QuerierConnection.Dispose();
                }
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Private Methods
        private ConnectionToSql GetConnectionToSql()
        {
            ConnectionToSql connectionToSql = _connectionsToSql.TryTake(out connectionToSql) ? connectionToSql : new ConnectionToSql(_sqlQuerier.BeginConnection());
            
            return connectionToSql;
        }
        #endregion
    }
}