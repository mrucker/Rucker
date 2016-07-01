using System;
using System.Linq;
using Rucker.Data;
using Rucker.Testing;
using Rucker.Extensions;
using NUnit.Framework;

namespace Rucker.Tests
{    
    [TestFixture]
    public class IQuerierTests
    {
        #region Fields
        private readonly IQuerier _querier;
        #endregion

        #region Private Class
        private class TestTable : TestDbTable
        {
            public TestTable(params object[] objects) : base("(Id INT NOT NULL IDENTITY, A VARCHAR(10) NULL, B VARCHAR(10) NULL, X INT)", objects)
            { }
        }
        #endregion

        public IQuerierTests(): this(new SqlQuerier(Test.ConnectionString))
        { }

        public IQuerierTests(IQuerier querier)
        {            
            _querier = querier;
        }

        [Test]
        public void SelectingFiveIntegers()
        {
            const string sql = "SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1";

            using (var connection = _querier.BeginConnection())
            {
                var ints = connection.SqlQuery<int>(sql).ToArray();

                Assert.AreEqual(5, ints.Length);

                foreach (var i in ints)
                {
                    Assert.AreEqual(1, i);
                }
            }
        }

        [Test]
        public void SelectingFiveStrings()
        {
            const string sql = "SELECT 'abc' UNION ALL SELECT 'ABC' UNION ALL SELECT NULL UNION ALL SELECT 'def' UNION ALL SELECT 'DEF'";

            using (var connection = _querier.BeginConnection())
            {
                var strings = connection.SqlQuery<string>(sql).ToArray();

                Assert.AreEqual(5, strings.Length);

                Assert.AreEqual("abc", strings[0]);
                Assert.AreEqual("ABC", strings[1]);
                Assert.AreEqual(null , strings[2]);
                Assert.AreEqual("def", strings[3]);
                Assert.AreEqual("DEF", strings[4]);
            }
        }

        [Test]
        public void SelectOneAlphabetObject()
        {
            const string sql = "SELECT 'A' AS A, 'B' AS B, 'C' AS C, 'D' AS D, 'E' AS E, 'F' AS F, 'G' AS G, 'H' AS H, 'I' AS I, 'J' AS J, 'K' AS K, 'L' AS L, 'M' AS M, 'N' AS N, 'O' AS O, 'P' AS P, 'Q' AS Q, 'R' AS R, 'S' AS S, 'T' AS T, 'U' AS U, 'V' AS V, 'W' AS W, 'X' AS X, 'Y' AS Y, 'Z' AS Z";

            using (var connection = _querier.BeginConnection())
            {
                var alphabets = connection.SqlQuery<Alphabet>(sql).ToArray();

                Assert.AreEqual(1, alphabets.Length);

                var alphabet = alphabets.First();

                AssertAlphabet(alphabet);
            }
        }

        [Test]
        public void SelectTwoAlphabetObjects()
        {
            const string sql = @"SELECT 'A' AS A, 'B' AS B, 'C' AS C, 'D' AS D, 'E' AS E, 'F' AS F, 'G' AS G, 'H' AS H, 'I' AS I, 'J' AS J, 'K' AS K, 'L' AS L, 'M' AS M, 'N' AS N, 'O' AS O, 'P' AS P, 'Q' AS Q, 'R' AS R, 'S' AS S, 'T' AS T, 'U' AS U, 'V' AS V, 'W' AS W, 'X' AS X, 'Y' AS Y, 'Z' AS Z
                                 UNION ALL 
                                 SELECT 'A' AS A, 'B' AS B, 'C' AS C, 'D' AS D, 'E' AS E, 'F' AS F, 'G' AS G, 'H' AS H, 'I' AS I, 'J' AS J, 'K' AS K, 'L' AS L, 'M' AS M, 'N' AS N, 'O' AS O, 'P' AS P, 'Q' AS Q, 'R' AS R, 'S' AS S, 'T' AS T, 'U' AS U, 'V' AS V, 'W' AS W, 'X' AS X, 'Y' AS Y, 'Z' AS Z";

            using (var connection = _querier.BeginConnection())
            {
                var alphabets = connection.SqlQuery<Alphabet>(sql).ToArray();

                Assert.AreEqual(2, alphabets.Length);

                foreach (var alphabet in alphabets)
                {
                    AssertAlphabet(alphabet);
                }
            }
        }

        [Test]
        public void SelectTopZeroAsRows()
        {
            using (var testTable  = new TestTable())
            using (var connection = _querier.BeginConnection())
            {
                Assert.IsTrue(connection.SqlQuery($"SELECT TOP 0 * FROM {testTable.TableUri.TableName}").None());
            }
        }

        [Test]
        public void SelectTopOneAsRows()
        {
            using (var testTable = new TestTable(new {X = 1, B = "a"}))
            using (var connection = _querier.BeginConnection())
            {
                var row = connection.SqlQuery($"SELECT TOP 1 * FROM {testTable.TableUri.TableName}").Single();

                Assert.AreEqual("1", row["X"]);
                Assert.AreEqual("a", row["B"]);
            }
        }

        [Test]
        public void SelectTopTwoAsRows()
        {
            using (var testTable = new TestTable(new { X = 1, B = "a" }, new {X = 3, B = "c"}))
            using (var connection = _querier.BeginConnection())
            {
                var rows = connection.SqlQuery($"SELECT TOP 2 * FROM {testTable.TableUri.TableName}");

                Assert.AreEqual(2, rows.Count());

                var row1 = rows.Take(1).Single();
                var row2 = rows.Skip(1).Single();

                Assert.AreEqual("1", row1["X"]);
                Assert.AreEqual("a", row1["B"]);

                Assert.AreEqual("3", row2["X"]);
                Assert.AreEqual("c", row2["B"]);
            }
        }

        [Test(Description = "This could easily break if Microsoft ever changes their underlying system tables at all.")]
        public void SelectFromTwoDifferentDatabases()
        {
            using (var connection = _querier.BeginConnection())
            {
                Assert.DoesNotThrow(() => connection.SqlQuery("SELECT TOP 0 * FROM msdb..sysalerts tt1 JOIN master..spt_fallback_db tt2 ON tt1.id = tt2.dbid"));
            }
        }

        [Test]
        public void SelectOneColumnFromTopTwoRows()
        {
            using (var table = new TestTable(new {X = 1}, new {X = 3}, new {X = 5}))
            using (var connection = _querier.BeginConnection())
            {
                var rows = connection.SqlQuery($"SELECT TOP 2 X FROM {table.TableUri.SchemaName}.{table.TableUri.TableName}");

                Assert.AreEqual(2, rows.Count());

                var row1 = rows.Skip(0).Take(1).Single();
                var row2 = rows.Skip(1).Take(1).Single();

                Assert.AreEqual(1, row1.Columns.Count());
                Assert.AreEqual(1, row2.Columns.Count());

                Assert.IsTrue(row1.Columns.Contains("X"));
                Assert.IsTrue(row2.Columns.Contains("X"));

                Assert.IsFalse(row1.Columns.Contains("Id"));
                Assert.IsFalse(row2.Columns.Contains("Id"));

                Assert.AreEqual("1", row1["X"]);
                Assert.AreEqual("3", row2["X"]);
            }
        }

        [Test]
        public void CheckThatTableExists()
        {
            using (var table = new TestTable())
            using (var connection = _querier.BeginConnection())
            {
                connection.TableExists(table.TableUri.SchemaName, table.TableUri.TableName);
            }
        }

        [Test]
        public void CheckThatFakeTableNotExists()
        {
            using (var connection = _querier.BeginConnection())
            {
                Assert.Throws(Is.TypeOf<Exception>().And.Message.EqualTo("dbo.FakeTable couldn't be found in master"), () => connection.TableExists("dbo", "FakeTable"));
            }
        }

        [Test]
        public void CheckThatColumnsExist()
        {
            using (var table = new TestTable())
            using (var connection = _querier.BeginConnection())
            {
                connection.ColumnsExist(table.TableUri.SchemaName, table.TableUri.TableName, "Id", "B", "X");
            }
        }

        [Test]        
        public void CheckThatFakeColumnsNotExist()
        {
            using (var table = new TestTable())
            using (var connection = _querier.BeginConnection())
            {
                Assert.Throws(Is.TypeOf<Exception>().And.Message.EqualTo($"Columns (FakeId) weren't found on table {table.TableUri.ToFullyQualifiedTableName()}"), () => connection.ColumnsExist("dbo", table.TableUri.TableName, "FakeId"));
            }
        }

        private static void AssertAlphabet(Alphabet alphabet)
        {
            Assert.AreEqual("A", alphabet.A);
            Assert.AreEqual("B", alphabet.B);
            Assert.AreEqual("C", alphabet.C);
            Assert.AreEqual("D", alphabet.D);
            Assert.AreEqual("E", alphabet.E);
            Assert.AreEqual("F", alphabet.F);
            Assert.AreEqual("G", alphabet.G);
            Assert.AreEqual("H", alphabet.H);
            Assert.AreEqual("I", alphabet.I);
            Assert.AreEqual("J", alphabet.J);
            Assert.AreEqual("K", alphabet.K);
            Assert.AreEqual("L", alphabet.L);
            Assert.AreEqual("M", alphabet.M);
            Assert.AreEqual("N", alphabet.N);
            Assert.AreEqual("O", alphabet.O);
            Assert.AreEqual("P", alphabet.P);
            Assert.AreEqual("Q", alphabet.Q);
            Assert.AreEqual("R", alphabet.R);
            Assert.AreEqual("S", alphabet.S);
            Assert.AreEqual("T", alphabet.T);
            Assert.AreEqual("U", alphabet.U);
            Assert.AreEqual("V", alphabet.V);
            Assert.AreEqual("W", alphabet.W);
            Assert.AreEqual("X", alphabet.X);
            Assert.AreEqual("Y", alphabet.Y);
            Assert.AreEqual("Z", alphabet.Z);
        }
    }
}