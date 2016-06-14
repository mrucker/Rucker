using NUnit.Framework;
using Rucker.Data;

namespace Rucker.Tests
{
    [TestFixture]
    public class TableUriTests
    {
        [Test]
        public void TableUriWorksWithSchema()
        {
            var tableName = "Table";
            var schemaName = "abc";
            var databaseName = "Database";
            var serverName = "Server";

            var uri = new TableUri($"table://{serverName}/{databaseName}/{schemaName}/{tableName}");

            Assert.AreEqual(tableName, uri.TableName);
            Assert.AreEqual(schemaName, uri.SchemaName);
            Assert.AreEqual(databaseName, uri.DatabaseUri.DatabaseName);
            Assert.AreEqual(serverName, uri.DatabaseUri.ServerName);
        }

        [Test]
        public void TableUriWorksWithoutSchema()
        {
            var tableName = "Table";
            var schemaName = "dbo";
            var databaseName = "Database";
            var serverName = "Server";

            var uri = new TableUri($"table://{serverName}/{databaseName}/{tableName}");

            Assert.AreEqual(tableName, uri.TableName);
            Assert.AreEqual(schemaName, uri.SchemaName);
            Assert.AreEqual(databaseName, uri.DatabaseUri.DatabaseName);
            Assert.AreEqual(serverName, uri.DatabaseUri.ServerName);
        }
    }
}