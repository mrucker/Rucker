using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Rucker.Data;
using Rucker.Testing;

namespace Rucker.Tests
{
    [TestFixture]
    public class WritBulkTests
    {
        #region Private Classes
        private class AlphabetTable : TestDbTable
        {
            public AlphabetTable(params object[] objects): base("(Id INT NOT NULL IDENTITY PRIMARY KEY, A CHAR, B CHAR NOT NULL, C CHAR)", objects)
            { }
        }
        #endregion

        #region Tests
        [Test]
        public void TwoSmallWritesPerformanceTest()
        {
            using (var testTable = new AlphabetTable())
            {
                var alphabets1 = new Table(testTable.TableUri.TableName, CreateRows(1000));
                var alphabets2 = new Table(testTable.TableUri.TableName, CreateRows(2000));

                using (var writeBulk = new WriteBulk(new SqlQuerierConnection(Test.ConnectionString)))
                {
                    //2-23-2016 0.5135818
                    //2-24-2016 0.5299455
                    var executionTime1 = Testing.Test.ExecutionTime(() => writeBulk.Write(new Bulk(alphabets1)));
                    Console.WriteLine("Millisecond/Alphabet: " + executionTime1.TotalMilliseconds / alphabets1.Rows.Count());

                    //2-23-2016 0.2129637
                    //2-24-2016 0.2112909
                    var executionTime2 = Testing.Test.ExecutionTime(() => writeBulk.Write(new Bulk(alphabets2)));
                    Console.WriteLine("Millisecond/Alphabet: " + executionTime2.TotalMilliseconds / alphabets2.Rows.Count());
                }

                Assert.AreEqual(3000, testTable.Read().Count());
            }
        }

        [Test, Ignore("Often times out on test servers")]
        public void OneLargeWritePerformanceTest()
        {
            using (var testTable = new AlphabetTable())
            {
                var alphabets = new Table(testTable.TableUri.TableName, CreateRows(50000));

                using (var writeBulk = new WriteBulk(new SqlQuerierConnection(Test.ConnectionString)))
                {
                    //2-23-2016 0.062496178
                    //2-24-2016 0.046557842
                    var executionTime = Testing.Test.ExecutionTime(() => writeBulk.Write(new Bulk(alphabets)));
                    Console.WriteLine("Millisecond/Alphabet: " + executionTime.TotalMilliseconds / alphabets.Rows.Count());
                }

                Assert.AreEqual(50000, testTable.Read().Count());
            }
        }

        public void WriteNullToNotNullColumn()
        {
            using (var table = new AlphabetTable())
            using (var write = new WriteBulk(new SqlQuerierConnection(Test.ConnectionString)))
            {
                Assert.Throws(Is.TypeOf<InvalidOperationException>().And.Message.Contains("does not allow DBNull.Value"), () => write.Write(new Bulk(table.TableUri.TableName, new[]{new ObjectRow(new { A = "A", B = (string)null, C = "C"})})));               
            }
        }

        [Test]
        public void WriteEmptyToNotNullColumn()
        {
            using (var table = new AlphabetTable())
            using (var write = new WriteBulk(new SqlQuerierConnection(Test.ConnectionString)))
            {
                Assert.AreEqual(0, table.Read().Count());

                write.Write(new Bulk(table.TableUri.TableName, new[] { new ObjectRow(new { A = "A", B = "", C = "C" }) }));

                Assert.AreEqual(1, table.Read().Count());
            }
        }
        #endregion

        #region Private Methods
        private static IEnumerable<IRow> CreateRows(int count)
        {
            return Enumerable.Range(0, count).Select(i => new ObjectRow(new { A = "A", B = "B", C = "C" }));
        }
        #endregion
    }
}