using System;
using System.Linq;
using System.Collections.Generic;
using Rucker.Core.Testing;
using Rucker.Core;
using NUnit.Framework;

// ReSharper disable AccessToDisposedClosure
namespace Rucker.Sql.Tests
{
    [TestFixture]
    public class WritBulkAndMergeTests
    {
        #region Tables
        private class TestTable: TestDbTable
        {
            public TestTable(params object[] objects): base("(Id int not null identity primary key, A char NOT NULL, B char, C char, StageId int)", new SqlQuerierConnection(Test.ConnectionString), objects)
            { }
        }
        #endregion

        #region Tests
        [Test]
        public void TwoSmallWritesPerformanceTest()
        {
            using (var testTable = new TestTable())
            {
                var alphabets1 = new Table(testTable.TableUri.TableName, CreateRows(0, 1000));
                var alphabets2 = new Table(testTable.TableUri.TableName, CreateRows(1000, 2000));

                using (var bulkAndMerge = new WriteBulkAndMerge(Querier()))
                {
                    //2-23-2016 1.5681354
                    //2-24-2016 1.4953359
                    var executionTime1 = Core.Testing.Test.ExecutionTime(() => bulkAndMerge.WritePerformance(new BulkAndMerge(alphabets1)));                    
                    Console.WriteLine("Millisecond/Alphabet: " + executionTime1.TotalMilliseconds / alphabets1.Rows.Count());

                    //2-23-2016 0.3666807
                    //2-24-2016 0.3213967
                    var executionTime2 = Core.Testing.Test.ExecutionTime(() => bulkAndMerge.WritePerformance(new BulkAndMerge(alphabets2)));
                    Console.WriteLine("Millisecond/Alphabet: " + executionTime2.TotalMilliseconds / alphabets2.Rows.Count());
                }

                Assert.AreEqual(3000, testTable.Read().Count());
            }
        }

        [Test, Ignore("Often times out on test servers")]
        public void OneLargeWritePerformanceTest()
        {
            using (var testTable = new TestTable())
            {
                var alphabets = new Table(testTable.TableUri.TableName, CreateRows(0, 50000));
                
                using (var bulkAndMerge = new WriteBulkAndMerge(Querier()))
                {
                    //2-23-2016 0.125447474
                    //2-24-2016 0.129404348
                    var executionTime = Core.Testing.Test.ExecutionTime(() => bulkAndMerge.WritePerformance(new BulkAndMerge(alphabets, new[] {"StageId"}, r => { })));
                    Console.WriteLine("Millisecond/Alphabet: " + executionTime.TotalMilliseconds/alphabets.Rows.Count());
                }

                Assert.AreEqual(50000, testTable.Read().Count());
            }
        }

        [Test]
        public void TwoSmallWritesNoMatchesAndSavingForeignKeys()
        {
            using (var testTable = new TestTable(new { A = "B", B = "B", C = "C", StageId = 2 }))
            {
                var alphabets1 = new Table(testTable.TableUri.TableName, CreateRows(0, 1000));
                var alphabets2 = new Table(testTable.TableUri.TableName, CreateRows(1000, 2000));

                var matches = new Dictionary<int, int>();

                Action<IRow> saveForeignKeys = o => matches.Add(o["StageId"].To<int>(), o["Id"].To<int>());

                using (var bulkAndMerge = new WriteBulkAndMerge(Querier()))
                {
                    bulkAndMerge.Write(new BulkAndMerge(alphabets1, new[] { "StageId" }, saveForeignKeys));
                    bulkAndMerge.Write(new BulkAndMerge(alphabets2, new[] { "StageId" }, saveForeignKeys));
                }

                var finalResults      = testTable.Read();
                var resultsRandom     = finalResults.Select(f => f["StageId"]);
                var alphabetsStageIds = alphabets1.Rows.Concat(alphabets2.Rows).Select(a => a["StageId"]).ToArray();
                
                Assert.AreEqual(3000, finalResults.Count());
                Assert.AreEqual(3000, finalResults.Select(r => r["StageId"]).Distinct().Count());
                Assert.AreEqual(3000, alphabetsStageIds.Count(resultsRandom.Contains));
                Assert.AreEqual(alphabetsStageIds.Length, matches.Count);
                Assert.IsTrue(finalResults.All(f => matches[f["StageId"].To<int>()] == f["Id"].To<int>()));
            }
        }

        [Test]
        public void OneSmallWriteWithMatchUpdates()
        {
            using (var testTable = new TestTable(new {A = "B", B = "B", C = "C", StageId = 2}))
            {
                var alphabets = new Table(testTable.TableUri.TableName, CreateRows(0, 1000));

                using (var bulkAndMerge = new WriteBulkAndMerge(Querier()))
                {
                    bulkAndMerge.Write(new BulkAndMerge(alphabets, new[] {"StageId"}, MergeAction.UpdateOrInsert));
                }

                var finalResults    = testTable.Read();
                var resultsRandom   = finalResults.Select(f => f["StageId"]).ToArray();
                var alphabetsRandom = alphabets.Rows.Select(a => a["StageId"]).ToArray();

                Assert.AreEqual(1000, finalResults.Count());
                Assert.AreEqual(1000, alphabetsRandom.Distinct().Count());
                Assert.AreEqual(0000, finalResults.Count(r => r["A"] != "A"));
                Assert.AreEqual(1000, finalResults.Count(r => r["A"] == "A"));
                Assert.AreEqual(1000, alphabetsRandom.Count(resultsRandom.Contains));
            }
        }

        [Test]
        public void OneSmallWriteWithMatchIgnores()
        {
            using (var testTable = new TestTable(new { A="B", B="B", C="C", StageId=2 }))
            {
                var alphabets = new Table(testTable.TableUri.TableName, CreateRows(0, 1000));

                using (var bulkAndMerge = new WriteBulkAndMerge(Querier()))
                {
                    bulkAndMerge.Write(new BulkAndMerge(alphabets, new[] { "StageId" }));
                }

                var finalResults    = testTable.Read();
                var resultsRandom   = finalResults.Select(f => f["StageId"]).ToArray();
                var alphabetsRandom = alphabets.Rows.Select(a => a["StageId"]).ToArray();

                Assert.AreEqual(1000, finalResults.Count());
                Assert.AreEqual(1000, alphabetsRandom.Distinct().Count());
                Assert.AreEqual(0001, finalResults.Count(r => r["A"] == "B"));
                Assert.AreEqual(0999, finalResults.Count(r => r["A"] == "A"));
                Assert.AreEqual(1000, alphabetsRandom.Count(resultsRandom.Contains));
            }
        }

        [Test]
        public void OneSmallWriteWithIds()
        {
            using (var testTable = new TestTable(new { A = "B", B = "B", C = "C", StageId = 2 }))
            {
                var alphabets = new Table(testTable.TableUri.TableName, new[] {new ObjectRow(new {Id = 1 , A = "A", B = "B", C = "C", StageId = 1}), new ObjectRow(new {Id = 3, A = "A", B = "B", C = "C", StageId = 3}) });

                using (var bulkAndMerge = new WriteBulkAndMerge(Querier()))
                {
                    bulkAndMerge.Write(new BulkAndMerge(alphabets, new[] { "Id" }, MergeAction.UpdateOrInsert));
                }

                var finalResults     = testTable.Read();
                var resulStageIds    = finalResults.Select(f => f["StageId"]).ToArray();
                var alphabetStageIds = alphabets.Rows.Select(a => a["StageId"]).ToArray();

                Assert.AreEqual(0002, finalResults.Count());
                Assert.AreEqual(0002, alphabetStageIds.Distinct().Count());
                Assert.AreEqual(0000, finalResults.Count(r => r["A"] != "A"));
                Assert.AreEqual(0002, finalResults.Count(r => r["A"] == "A"));
                Assert.AreEqual(0002, alphabetStageIds.Count(resulStageIds.Contains));
            }
        }

        [Test]
        public void OneSmallWriteWithEmpty()
        {
            using (var testTable = new TestTable(new { A = "B", B = "B", C = "C", StageId = 2 }))
            {
                var alphabets = new Table(testTable.TableUri.TableName, new[] { new ObjectRow(new { Id = 1, A = "A", B = "B", C = "C", StageId = 1 }), new ObjectRow(new { Id = 3, A = "A", B = "B", C = "C", StageId = 3 }) });

                using (var bulkAndMerge = new WriteBulkAndMerge(Querier()))
                {
                    bulkAndMerge.Write(new BulkAndMerge(alphabets, new[] { "Id" }, MergeAction.UpdateOrInsert));
                }

                var finalResults = testTable.Read();
                var resulStageIds = finalResults.Select(f => f["StageId"]).ToArray();
                var alphabetStageIds = alphabets.Rows.Select(a => a["StageId"]).ToArray();

                Assert.AreEqual(0002, finalResults.Count());
                Assert.AreEqual(0002, alphabetStageIds.Distinct().Count());
                Assert.AreEqual(0000, finalResults.Count(r => r["A"] != "A"));
                Assert.AreEqual(0002, finalResults.Count(r => r["A"] == "A"));
                Assert.AreEqual(0002, alphabetStageIds.Count(resulStageIds.Contains));
            }
        }

        [Test]
        public void OneSmallWriteWithMatchesAndSavingForeignKeys()
        {
            using (var testTable = new TestTable(new { A = "C", B = "B", C = "C", StageId = -1 }, new { A="B", B="B", C="C", StageId=0}))
            {
                var matches = new Dictionary<int,int>();
                Action<IRow> saveForeignKeys = o => matches.Add(o["StageId"].To<int>(), o["Id"].To<int>());

                var alphabets = new Table(testTable.TableUri.TableName, CreateRows(0, 1000));

                using (var bulkAndMerge = new WriteBulkAndMerge(Querier()))
                {
                    bulkAndMerge.Write(new BulkAndMerge(alphabets, new[] { "StageId" }, saveForeignKeys, MergeAction.UpdateOrInsert));
                }

                var finalResults      = testTable.Read();
                var resultStageIds    = finalResults.Select(f => f["StageId"]).ToArray();
                var alphabetsStageIds = alphabets.Rows.Select(a => a["StageId"]).ToArray();

                Assert.AreEqual(1001, finalResults.Count());
                Assert.AreEqual(1000, alphabetsStageIds.Distinct().Count());
                Assert.AreEqual(0001, finalResults.Count(r => r["A"] == "C"));
                Assert.AreEqual(1000, finalResults.Count(r => r["A"] == "A"));
                Assert.AreEqual(1000, alphabetsStageIds.Count(resultStageIds.Contains));
                Assert.AreEqual(alphabetsStageIds.Length, matches.Count);
                Assert.IsTrue(finalResults.Where(f => f["StageId"] != "-1").All(f => matches[f["StageId"].To<int>()] == f["Id"].To<int>()));
            }
        }

        [Test]
        public void OneSmallMatchOnlyAndSavingForeignKeys()
        {
            using (var testTable = new TestTable(new { A = "C", B = "B", C = "C", StageId = -1 }, new { A = "B", B = "B", C = "C", StageId = 0 }))
            {
                var matches = new Dictionary<int, int>();
                Action<IRow> saveForeignKeys = o => matches.Add(o["StageId"].To<int>(), o["Id"].To<int>());

                var alphabets = new Table(testTable.TableUri.TableName, CreateRows(0, 1000));

                using (var bulkAndMerge = new WriteBulkAndMerge(Querier()))
                {
                    bulkAndMerge.Write(new BulkAndMerge(alphabets, new[] { "StageId" }, saveForeignKeys, MergeAction.MatchOnly));
                }

                var finalResults      = testTable.Read();
                var resultStageIds    = finalResults.Select(f => f["StageId"]).ToArray();
                var alphabetsStageIds = alphabets.Rows.Select(a => a["StageId"]).ToArray();

                Assert.AreEqual(1000, alphabetsStageIds.Distinct().Count());
                Assert.AreEqual(0002, finalResults.Count());
                Assert.AreEqual(0000, finalResults.Count(r => r["A"] == "A"));
                Assert.AreEqual(0001, finalResults.Count(r => r["A"] == "B"));
                Assert.AreEqual(0001, finalResults.Count(r => r["A"] == "C"));
                Assert.AreEqual(0001, alphabetsStageIds.Count(resultStageIds.Contains));
                Assert.AreEqual(0001, matches.Count);
            }
        }
        #endregion

        #region Private Methods
        private static IEnumerable<IRow> CreateRows(int startingStageId, int count)
        {
            return Enumerable.Range(0, count).Select(i => new ObjectRow(new {A = "A", B = "B", C = "C", StageId = startingStageId + i}));
        }
        
        private static SqlQuerier Querier()
        {
            return new SqlQuerier(Test.ConnectionString);
        }
        #endregion
    }
}