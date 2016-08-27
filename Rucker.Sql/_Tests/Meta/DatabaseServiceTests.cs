using System.Linq;
using Rucker.Testing;
using Rucker.Data.Testing;
using NUnit.Framework;


namespace Rucker.Sql.Tests
{
    [TestFixture]
    public class DatabaseServiceTests
    {
        #region Private Class
        private class TestTable : TestDbTable
        {
            public TestTable() : base("(Id int identity, Bills int NOT NULL, Cents decimal(2,2) null, Notes varchar(1000) null, Bools bit null, InsertDate datetime null, UpdateDate datetime2 NOT NULL)", new SqlQuerierConnection(Test.ConnectionString))
            { }
        }
        #endregion

        #region Fields
        private TestTable _testTable;
        #endregion

        [OneTimeSetUp]
        public void Setup()
        {
            _testTable = new TestTable();            
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _testTable.Dispose();
        }

        [Test]
        public void SimpleColumnMetaTest()
        {
            using(var service = new DatabaseService())
            {
                var columns = service.GetColumns(Test.ConnectionString, _testTable.TableUri.TableName).ToArray();

                Assert.AreEqual(7, columns.Length);
                Assert.IsTrue(columns.Any(c => c.Name == "Id"         && c.TSqlType == TSqlTypes.@int      && !c.Nullable));
                Assert.IsTrue(columns.Any(c => c.Name == "Bills"      && c.TSqlType == TSqlTypes.@int      && !c.Nullable));
                Assert.IsTrue(columns.Any(c => c.Name == "Cents"      && c.TSqlType == TSqlTypes.@decimal  &&  c.Nullable));
                Assert.IsTrue(columns.Any(c => c.Name == "Notes"      && c.TSqlType == TSqlTypes.varchar   &&  c.Nullable));
                Assert.IsTrue(columns.Any(c => c.Name == "Bools"      && c.TSqlType == TSqlTypes.bit       &&  c.Nullable));
                Assert.IsTrue(columns.Any(c => c.Name == "InsertDate" && c.TSqlType == TSqlTypes.datetime  &&  c.Nullable));
                Assert.IsTrue(columns.Any(c => c.Name == "UpdateDate" && c.TSqlType == TSqlTypes.datetime2 && !c.Nullable));
            }
        }

        [Test]
        public void SimpleDateTimeMetaTest()
        {
            using (var service = new DatabaseService())
            {
                var columns = service.GetColumns(Test.ConnectionString, _testTable.TableUri.TableName).Where(c => c.IsType(DotNetTypes.DateTime)).ToArray();

                Assert.AreEqual(2, columns.Length);
                Assert.IsTrue(columns.Any(c => c.Name == "InsertDate" && c.TSqlType == TSqlTypes.datetime  && c.Nullable));
                Assert.IsTrue(columns.Any(c => c.Name == "UpdateDate" && c.TSqlType == TSqlTypes.datetime2 && !c.Nullable));
            }
        }

        [Test]
        public void SimpleNumbersTest()
        {
            using (var service = new DatabaseService())
            {
                var columns = service.GetColumns(Test.ConnectionString, _testTable.TableUri.TableName).Where(c => c.IsType(TSqlTypes.Numbers)).ToArray();

                Assert.AreEqual(3, columns.Length);
                Assert.IsTrue(columns.Any(c => c.Name == "Id"    && c.TSqlType == TSqlTypes.@int     && !c.Nullable));
                Assert.IsTrue(columns.Any(c => c.Name == "Bills" && c.TSqlType == TSqlTypes.@int     && !c.Nullable));
                Assert.IsTrue(columns.Any(c => c.Name == "Cents" && c.TSqlType == TSqlTypes.@decimal && c.Nullable));
            }
        }
    }
}