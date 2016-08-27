using System.Linq;
using Rucker.Dispose;
using Rucker.Extensions;

namespace Rucker.Data.Testing
{
    public class TestDbTable : Disposable
    {
        #region Fields
        private readonly IQuerierConnection _connection;
        #endregion

        #region Properties
        public TableUri TableUri { get; }
        #endregion

        #region Constructors
        public TestDbTable(string definition, IQuerierConnection connection, params object[] rows)
        {
            _connection = connection;
            
            TableUri = new TableUri($"table://{_connection.Server}/{_connection.Database}/##test_{System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this)}");

            Create(definition);
            Insert(rows);
        }
        #endregion

        #region Public Methods
        public IRows Read()
        {
            return _connection.SqlQuery($"SELECT * FROM {TableUri.TableName}");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connection.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Private Methods
        private void Create(string definition)
        {
            _connection.SqlCommand($"CREATE TABLE {TableUri.TableName} {definition}");
        }

        private void Insert(object[] objects)
        {
            foreach (var row in objects) Insert(new ObjectRow(row));
        }

        private void Insert(ObjectRow row)
        {
            var columns = $"{row.Columns.Cat(",")}";
            var values  = $"{row.RawValues.Select(Valify).Cat(",")}";

            _connection.SqlCommand($"INSERT {TableUri.TableName} ({columns}) VALUES ({values})");
        }

        private string Valify(object value)
        {
            if(value == null)
            {
                return "NULL";
            }

            if(value is int)
            {
                return $"{value}";
            }

            return $"'{value}'";
        }
        #endregion
    }
}