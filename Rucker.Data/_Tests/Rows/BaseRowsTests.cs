using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Rucker.Convert;
using Rucker.Testing;
using NUnit.Framework;


namespace Rucker.Data.Tests
{
    [TestFixture]
    [SuppressMessage("ReSharper", "RedundantToStringCall")]
    [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
    public class BaseRowsTests
    {
        #region Private Classes
        private class TestObject
        {
            public int N { get; set; }
            public decimal D { get; set; }
            public char L { get; set; }
            public string W { get; set; }
            public bool B { get; set; }
        }
        #endregion

        #region Tests
        [Test]
        public void ObjectsToRowsTest()
        {
            var obj = new TestObject { N = 1, D = .25M, L = 'A', W = "Ant", B = true};
            var objs = new[] {obj};

            var rows = BaseRows.ObjectsToRows(objs);
            var row  = rows.Single();

            Assert.AreEqual(5, row.Values.Count());
            Assert.AreEqual(5, row.Columns.Count());

            Assert.AreEqual(obj.N.ToString(), row["N"]);
            Assert.AreEqual(obj.D.ToString(), row["D"]);
            Assert.AreEqual(obj.L.ToString(), row["L"]);
            Assert.AreEqual(obj.W.ToString(), row["W"]);
            Assert.AreEqual(obj.B.ToString(), row["B"]);
        }

        [Test]
        public void RowsToObjectsTest()
        {
            var row  = new DictionaryRow {{"N", "1"}, {"D", "0.25"}, {"L", "A"}, {"W", "Ant"}, {"B", "True"}};
            var rows = new [] { row };

            var objs = BaseRows.RowsToObjects<TestObject>(rows);
            var obj  = objs.Single();

            Assert.AreEqual(row["N"], obj.N.ToString());
            Assert.AreEqual(row["D"], obj.D.ToString());
            Assert.AreEqual(row["L"], obj.L.ToString());
            Assert.AreEqual(row["W"], obj.W.ToString());
            Assert.AreEqual(row["B"], obj.B.ToString());
        }

        [Test]
        public void JsonsToRowsTest()
        {
            var json = "{ \"N\":\"1\", \"D\":\"0.25\", \"L\":\"A\", \"W\":\"Ant\", \"B\":\"True\" }";
            var row  = BaseRows.JsonsToRows(new[] { json }).Single();

            Assert.AreEqual("1"   , row["N"]);
            Assert.AreEqual("0.25", row["D"]);
            Assert.AreEqual("A"   , row["L"]);
            Assert.AreEqual("Ant" , row["W"]);
            Assert.AreEqual("True", row["B"]);
        }

        [Test]
        public void RowsToJsonsTest()
        {
            var row  = new DictionaryRow { { "N", "1" }, { "D", "0.25" }, { "L", "A" }, { "W", "Ant" }, { "B", "True" } };
            var rows = new [] {row};

            var jsons = BaseRows.RowsToJsons(rows);
            var json = jsons.Single();

            Assert.AreEqual("{\"B\":\"True\",\"D\":\"0.25\",\"L\":\"A\",\"N\":\"1\",\"W\":\"Ant\"}", json);
        }

        [Test]
        public void ObjectsToRowsPerformanceTest()
        {
            const int tenThousand = 10000;

            var alphabets = Enumerable.Range(0, tenThousand).Select(i => new Alphabet()).ToArray();

            var executionTime = Test.ExecutionTime(() => BaseRows.ObjectsToRows(alphabets).ToArray());

            double actualMillisecondsPerRow = executionTime.TotalMilliseconds / tenThousand;
            const double slowestTimeAllowed = .05;

            Console.WriteLine("Milliseconds Per Row: " + actualMillisecondsPerRow);

            //11-25-2014: 0.002
            //10-08-2015: 0.0002
            Assert.Less(actualMillisecondsPerRow, slowestTimeAllowed);
        }

        [Test]
        public void RowsToObjectsPerformanceTest()
        {
            const int tenThousand = 10000;

            var alphabetRows = Enumerable.Range(0, tenThousand).Select(i => new ObjectRow(new { N = 1, D = 1.23, L = 'A', W = "Ant", B = true })).ToArray();

            var executionTime = Test.ExecutionTime(() => BaseRows.RowsToObjects<TestObject>(alphabetRows).ToArray());

            double actualMillisecondsPerRow = executionTime.TotalMilliseconds / tenThousand;
            const double slowestTimeAllowed = .05;

            Console.WriteLine("Milliseconds Per Row: " + actualMillisecondsPerRow);

            //11-25-2014: 0.029
            //10-08-2015: 0.014
            Assert.Less(actualMillisecondsPerRow, slowestTimeAllowed);
        }

        [Test]
        public void DataTableToRowsPerformanceTest()
        {
            const int tenThousand = 100000;

            var dataTable = Enumerable.Range(0, tenThousand).Select(i => new Alphabet()).ToDataTable();

            var executionTime = Test.ExecutionTime(() => BaseRows.DataTableToRows(dataTable).ToArray());

            double actualMillisecondPerRow  = executionTime.TotalMilliseconds / tenThousand;
            const double slowestTimeAllowed = .2;            
            
            //08-27-2014: 0.085
            //11-25-2014: 0.070
            //10-08-2015: 0.077
            //11-20-2015: 0.017
            Assert.Less(actualMillisecondPerRow, slowestTimeAllowed);
            
            Console.WriteLine("Actual DataRowToRow Millisecond Per Row: " + actualMillisecondPerRow);
        }

        [Test]
        public void JsonsToRowsPerformanceTest()
        {
            const int tenThousand = 10000;

            var alphabets     = Enumerable.Range(0, tenThousand).Select(i => new Alphabet()).ToArray();
            var alphabetJsons = alphabets.Select(BaseRow.ObjectToRow).Select(BaseRow.RowToJson).ToArray();

            var executionTime = Test.ExecutionTime(() => BaseRows.JsonsToRows(alphabetJsons).ToArray());

            double actualMillisecondsPerRow = executionTime.TotalMilliseconds / tenThousand;
            const double slowestTimeAllowed = .2;

            //11-25-2014: 0.117
            //10-08-2015: 0.089
            Assert.Less(actualMillisecondsPerRow, slowestTimeAllowed);

            Console.WriteLine("Milliseconds Per Row: " + actualMillisecondsPerRow);
        }

        [Test]
        public void RowsGroupByMemoryTest()
        {
            const int tenThousand = 50000;

            var keyColumns = new[] {"Number1Column", "Number2Column"};

            var tenThousandRows = Enumerable.Range(0, tenThousand).Select(i => new ObjectRow(new { Number1Column = i, Number2Column = i, Number3Column = i }));

            var notGroupedMemory = Test.ExecutionBits(() => tenThousandRows.ToArray());
            var groupedMemory    = Test.ExecutionBits(() => tenThousandRows.GroupBy(r => r.WithColumns(keyColumns)).ToArray());
            var dictionaryMemory = Test.ExecutionBits(() => tenThousandRows.GroupBy(r => r.WithColumns(keyColumns)).ToDictionary(g => g.Key, g => g.Single()["Number3Column"].To<int>()));

            var groupedMultiplier    = Math.Round(groupedMemory/notGroupedMemory, 2);
            var dictionaryMultiplier = Math.Round(dictionaryMemory/notGroupedMemory, 2);

            Console.WriteLine($"Grouped rows take {groupedMultiplier} times more space than an Array of rows");
            Console.WriteLine($"Dictionary rows take {dictionaryMultiplier} times more space than Array of rows");
        }

        [Test]
        public void RowsWithLinqTest()
        {
            var obj = new TestObject { N = 1, D = .25M, L = 'A', W = "Ant", B = true };
            var objs = new[] { obj };

            var rows = BaseRows.ObjectsToRows(objs).Select(ReplaceL).ToRows();
            var row = rows.Single();

            Assert.AreEqual(5, row.Values.Count());
            Assert.AreEqual(5, row.Columns.Count());

            Assert.AreEqual(obj.N.ToString(), row["N"]);
            Assert.AreEqual(obj.D.ToString(), row["D"]);
            Assert.AreEqual("L"             , row["L"]);
            Assert.AreEqual(obj.W.ToString(), row["W"]);
            Assert.AreEqual(obj.B.ToString(), row["B"]);
        }

        [Test]
        public void ManyRowsWithOneDistinctPerformance()
        {
            const int rowCount = 1000000;
            const int modulus = 3000;

            var rows = Enumerable.Range(0, rowCount).Select(i => new ObjectRow(new Alphabet { A = $"{ i % modulus}" })).ToArray();

            var executionTime = Test.ExecutionTime(() => { rows.WithDistinct(new[] { "A" }); });

            var actualMillisecondsPerRow = executionTime.TotalMilliseconds / rowCount;
            var expectedMillisecondsPerRow = 0.05;

            Console.WriteLine("Milliseconds Per Row: " + actualMillisecondsPerRow);

            //11/20/2015 .0019

            Assert.Less(actualMillisecondsPerRow, expectedMillisecondsPerRow);
        }

        public void ManyRowsWithTwoDistinctPerformance()
        {
            const int rowCount = 1000000;
            const int modulus = 3000;

            var rows = Enumerable.Range(0, rowCount).Select(i => new ObjectRow(new Alphabet { A = $"{ i % modulus}" })).ToArray();

            var executionTime = Test.ExecutionTime(() => { rows.WithDistinct(new[] { "A" }); });

            var actualMillisecondsPerRow = executionTime.TotalMilliseconds / rowCount;
            var expectedMillisecondsPerRow = 0.05;

            Console.WriteLine("Milliseconds Per Row: " + actualMillisecondsPerRow);

            //11/20/2015 .0019

            Assert.Less(actualMillisecondsPerRow, expectedMillisecondsPerRow);
        }

        [Test]
        public void FewRowsWithDistinctPerformance()
        {
            const int rowCount = 1000;
            const int modulus = 3000;

            var rows = Enumerable.Range(0, rowCount).Select(i => new ObjectRow(new Alphabet { A = $"{ i % modulus}" })).ToArray();

            var executionTime = Test.ExecutionTime(() => { rows.WithDistinct(new[] { "A" }); });

            var actualMillisecondsPerRow = executionTime.TotalMilliseconds / rowCount;
            var expectedMillisecondsPerRow = 0.05;

            Console.WriteLine("Milliseconds Per Row: " + actualMillisecondsPerRow);

            //11/20/2015 .0391

            Assert.Less(actualMillisecondsPerRow, expectedMillisecondsPerRow);
        }

        [Test]
        public void OneColumnRowsWithDistinctWorks()
        {
            const int rowCount = 10000;
            const int modulus = 3000;

            var rows = Enumerable.Range(0, rowCount).Select(i => new ObjectRow(new Alphabet { A = $"{ i % modulus}" })).ToArray();

            var distinctRows = rows.WithDistinct(new[] { "A", "B" });
            var distinctMods = distinctRows.Select(r => r["A"].To<int>()).ToArray();

            Assert.AreEqual(modulus, distinctRows.Count());

            for (var i = 0; i < modulus; i++)
            {
                Assert.Contains(i, distinctMods);
            }
        }

        [Test]
        public void TwoColumnRowsWithDistinctWorks()
        {
            var rows = new[]
            {
                new ObjectRow(new {A = "A", B = "B"}),
                new ObjectRow(new {A = "A", B = "B"}),
                new ObjectRow(new {A = "A", B = "B"}),
                new ObjectRow(new {A = "A", B = "B"}),
                new ObjectRow(new {A = "B", B = "A"}),
                new ObjectRow(new {A = "B", B = "A"}),
                new ObjectRow(new {A = "C", B = "D"}),
                new ObjectRow(new {A = "E", B = "F"}),
            };

            Assert.AreEqual(8, rows.Length);

            var distinctRows = rows.WithDistinct(new[] { "A", "B" });

            Assert.AreEqual(4, distinctRows.Count());

            Assert.AreEqual(1, distinctRows.Count(r => r["A"] == "A" && r["B"] == "B"));
            Assert.AreEqual(1, distinctRows.Count(r => r["A"] == "B" && r["B"] == "A"));
            Assert.AreEqual(1, distinctRows.Count(r => r["A"] == "C" && r["B"] == "D"));
            Assert.AreEqual(1, distinctRows.Count(r => r["A"] == "E" && r["B"] == "F"));
        }
        #endregion

        #region Private Methods
        private static IRow ReplaceL(IRow row)
        {
            row["L"] = "L";
            
            return row;
        }
        #endregion
    }
}