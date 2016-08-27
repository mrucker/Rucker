using System.Linq;
using Rucker.Core.Testing;
using Rucker.Data.Testing;
using NUnit.Framework;


namespace Rucker.Sql.Tests
{
    [TestFixture]
    public class ReadSqlQueryPre2012Tests
    {
        #region Private Class
        private class TestTable: TestDbTable
        {
            public TestTable(params object[] objects) : base("(Id INT NOT NULL IDENTITY, A VARCHAR(10) NULL, B VARCHAR(10) NOT NULL, X INT)", new SqlQuerierConnection(Test.ConnectionString), objects)
            { }
        }
        #endregion

        #region Tests
        [Test]
        public void ReadTwoSinglePages()
        {
            using (var table  = new TestTable(new { A = "A", B = "B", X = 1 }, new { A = "A", B = "B", X = 2 }))
            using (var reader = new ReadSqlServerPre2012(Querier(), Select(table)))
            {
                var result1 = reader.Read(0, 1).Single();
                var result2 = reader.Read(1, 1).Single();

                Assert.AreEqual(2, reader.Size());
                Assert.AreEqual("1", result1["X"]);
                Assert.AreEqual("2", result2["X"]);
            }
        }

        [Test]
        public void ReadTwoDoublePages()
        {
            using (var table  = new TestTable(new { A = "A", B = "B", X = 1 }, new { A = "A", B = "B", X = 2 }, new { A = "A", B = "B", X = 3 }, new { A = "A", B = "B", X = 4 }))
            using (var reader = new ReadSqlServerPre2012(Querier(), Select(table)))
            {
                var result1 = reader.Read(0, 2);
                var result2 = reader.Read(2, 2);

                Assert.AreEqual(4, reader.Size());
                Assert.AreEqual("1", result1.First()["X"]);
                Assert.AreEqual("2", result1.Last()["X"]);
                Assert.AreEqual("3", result2.First()["X"]);
                Assert.AreEqual("4", result2.Last()["X"]);
            }
        }

        [Test]
        public void ReadRowWithNull()
        {
            using (var table  = new TestTable(new { A = (string)null, B = "B", X = 1 }))
            using (var reader = new ReadSqlServerPre2012(Querier(), Select(table)))
            {
                var result1 = reader.Read(0, 1).Single();
                
                Assert.AreEqual(1, reader.Size());
                Assert.AreEqual(null, result1["A"]);
                Assert.AreEqual("B", result1["B"]);
                Assert.AreEqual("1", result1["X"]);
            }
        }
        #endregion

        #region Private Methods
        private SqlQuerier Querier()
        {
            return new SqlQuerier(Test.ConnectionString);
        }

        private string Select(TestTable table)
        {
            return $"SELECT * FROM {table.TableUri.TableName}";
        }
        #endregion
    }
}