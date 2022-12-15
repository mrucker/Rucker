using System;
using System.Linq;
using System.Data.SqlClient;
using Data.Core;

namespace Data.Sql
{
    public class WriteBulk: IWrite<IBulk>
    {
        #region Fields
        private readonly SqlQuerierConnection _querierConnection;
        private readonly SqlQuerier _querier;
        private readonly int? _timeout;
        #endregion

        #region Public Constructors
        public WriteBulk(SqlQuerierConnection querierConnection, int? timeout = null)
        {
            if(querierConnection == null) throw new ArgumentNullException(nameof(querierConnection));

            _querierConnection = querierConnection;
            _timeout = timeout;
        }

        public WriteBulk(SqlQuerier querier, int? timeout = null)
        {
            if (querier == null) throw new ArgumentNullException(nameof(querier));

            _querier = querier;
            _timeout = timeout;
        }
        #endregion

        #region IWrite<Bulk> Implementation
        public void Write(IBulk bulk)
        {
            if (_querier != null)
            {
                WithFactory(bulk);
            }

            if (_querierConnection != null)
            {
                WithQuerier(bulk);
            }
        }

        public void Dispose()
        {
            
        }
        #endregion

        #region Private Methods
        private void WithFactory(IBulk bulk)
        {
            using (var querierConnection = _querier.BeginConnection())
            {
                Write(querierConnection, bulk);
            }
        }

        private void WithQuerier(IBulk bulk)
        {
            Write(_querierConnection, bulk);
        }

        private void Write(SqlQuerierConnection querierConnection, IBulk bulk)
        {
            using (var sqlBulkCopy = new SqlBulkCopy(querierConnection.SqlConnection, SqlBulkCopyOptions.KeepIdentity, null))
            {
                sqlBulkCopy.BulkCopyTimeout      = _timeout ?? sqlBulkCopy.BulkCopyTimeout;
                sqlBulkCopy.DestinationTableName = bulk.Schema + "." + bulk.Table;

                foreach (var columnMap in bulk.ColumnMaps)
                {
                    sqlBulkCopy.ColumnMappings.Add(columnMap.Source, columnMap.Target);
                }

                try
                {
                    sqlBulkCopy.WriteToServer(bulk.DataReader);
                }
                catch (SqlException ex)
                {
                    if (ex.Message.Contains("Received an invalid column length from the bcp client for colid"))
                    {
                        throw DetailedColumnLengthException(ex, sqlBulkCopy);
                    }
                    
                    throw;
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message == "The given ColumnMapping does not match up with any column in the source or destination.")
                    {
                        throw MoreDetailedColumnMappingException(bulk, ex);
                    }

                    if (ex.Message.Contains("The given value of type") && ex.Message.Contains("from the data source cannot be converted to type"))
                    {
                        throw new UserMessageException(ex.Message);
                    }

                    throw;
                }
            }
        }

        private InvalidOperationException MoreDetailedColumnMappingException(IBulk bulk, Exception ex)
        {
            return new InvalidOperationException($"Invalid column mappings for a bulk write to [{_querier.Database}].[{bulk.Schema}].[{bulk.Table}] ({bulk.ColumnMaps.Select(cm => $"'{cm.Source}':'{cm.Target}'").Cat(", ")})", ex);
        }

        private static Exception DetailedColumnLengthException(SqlException ex, SqlBulkCopy sqlBulkCopy)
        {            
            var match = System.Text.RegularExpressions.Regex.Match(ex.Message, @"\d+");
            var index = match.Value.To<int>() - 1;

            var sorted   = typeof(SqlBulkCopy).GetField("_sortedColumnMappings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(sqlBulkCopy);
            var items    = (object[])sorted.GetType().GetField("_items", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(sorted);
            var metadata = items[index].GetType().GetField("_metadata", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(items[index]);

            var column = metadata.GetType().GetField("column", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(metadata);
            var length = metadata.GetType().GetField("length", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(metadata);

            
            return new Exception($"{column} contains data with a length greater than {length}", ex);
        }

        #endregion
    }
}