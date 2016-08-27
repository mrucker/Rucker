using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Rucker.Core.Testing;
using NUnit.Framework;

namespace Rucker.Data.Tests
{
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class ObjectRowTests
    {
        [Test]
        public void ColumnReadTest1()
        {
            var row = new ObjectRow(new Alphabet());

            for (var i = 'A'; i <= 'Z'; i++)
            {
                Assert.AreEqual(i.ToString(), row[i.ToString()]);
            }

            Assert.AreEqual(26, row.Columns.Count());
        }

        [Test]
        public void ColumnReadTest2()
        {
            var alphabet    = new Alphabet();
            var properties  = alphabet.GetType().GetProperties();
            var definitions = properties.Select(p => new ObjectRow.PropertyDefinition(p));
            var values      = properties.Select(p => new ObjectRow.PropertyValue(p, alphabet));

            var row = new ObjectRow(definitions, values);

            for (var i = 'A'; i <= 'Z'; i++)
            {
                Assert.AreEqual(i.ToString(), row[i.ToString()]);
            }

            Assert.AreEqual(26, row.Columns.Count());
        }

        [Test]
        public void ColumnWriteTest()
        {
            var alphabet = new Alphabet();

            var row = new ObjectRow(alphabet);

            for (var i = 'A'; i <= 'Z'; i++)
            {
                var newValue = i + "1";

                row[i.ToString()] = newValue;
                
                Assert.AreEqual(newValue, row[i.ToString()]);
            }

            Assert.AreEqual(26, row.Columns.Count());
        }

        [Test]
        public void ColumnAddTest()
        {
            var row = new ObjectRow(new Alphabet())
            {
                {"abc", 123}
            };

            Assert.AreEqual("123", row["abc"]);
        }

        [Test]
        public void CreatePerformanceTest()
        {
            const int tenThousand = 100000;

            var alphabets = Enumerable.Range(0, tenThousand).Select(i => new Alphabet()).ToList();

            var executionTime = Test.ExecutionTime(() =>
            {
                foreach (var alphabet in alphabets)
                {
                    var properties  = alphabet.GetType().GetProperties();
                    var definitions = properties.Select(p => new ObjectRow.PropertyDefinition(p)).ToArray();
                    var values      = properties.Select(p => new ObjectRow.PropertyValue(p, alphabet)).ToArray();

                    var o = new ObjectRow(definitions, values);
                }
            });

            var actualMillisecondsPerRow   = executionTime.TotalMilliseconds / tenThousand;
            var expectedMillisecondsPerRow = 0.2;

            Console.WriteLine("Milliseconds Per Row: " + actualMillisecondsPerRow);

            // ??/??/???? .0600
            // 12/22/2014 .1011
            // 10/08/2015 .0473
            // 11/20/2015 .0230
            Assert.Less(actualMillisecondsPerRow, expectedMillisecondsPerRow);
        }

        [Test]
        public void ReadPerformanceTest()
        {
            const int tenThousand = 10000;

            var row = new ObjectRow(new Alphabet());

            var executionTime = Test.ExecutionTime(() => { for (var i = 0; i < tenThousand; i++) { var a = row["A"]; } });

            var actualMillisecondsPerRow = executionTime.TotalMilliseconds / tenThousand;
            var expectedMillisecondsPerRow = 0.05;

            Console.WriteLine("Milliseconds Per Row: " + actualMillisecondsPerRow);

            //.0001
            //11-19-2015 .00248 
            //11-19-2015 .00157
            //11-19-2015 .00053
                                     
            Assert.Less(actualMillisecondsPerRow, expectedMillisecondsPerRow);
        }

        [Test]
        public void WritePerformanceTest()
        {
            const int tenThousand = 10000;

            var row = new ObjectRow(new Alphabet());

            var executionTime = Test.ExecutionTime(() => { for (var i = 0; i < tenThousand; i++) { row["A"] = "A"; } });

            var actualMillisecondsPerRow = executionTime.TotalMilliseconds / tenThousand;
            var expectedMillisecondsPerRow = 0.05;

            Console.WriteLine("Milliseconds Per Row: " + actualMillisecondsPerRow);

            //11/21/2015 0.0009
            Assert.Less(actualMillisecondsPerRow, expectedMillisecondsPerRow);
        }

        [Test]
        public void AddPerformanceTest()
        {
            var rows = Enumerable.Range(0, 100000).Select(i => new ObjectRow(new Alphabet())).ToArray();

            var executionTime = Test.ExecutionTime(() => { for (var i = 0; i < rows.Length; i++) { rows[i].SafeAdd("index", i); } });

            var actualMillisecondsPerRow = executionTime.TotalMilliseconds / rows.Length;
            var expectedMillisecondsPerRow = 0.05;

            Console.WriteLine("Milliseconds Per Row: " + actualMillisecondsPerRow);

            //11/20/2015 .0413
            //11/20/2015 .0230

            Assert.Less(actualMillisecondsPerRow, expectedMillisecondsPerRow);
        }

        [Test]
        public void MemoryPerformanceTest()
        {
            const int twoMillion = 2000000;

            var usedMemory = Test.ExecutionBits(() => GetRows(twoMillion).ToArray()) / Test.MegaByte;

            Assert.Less(usedMemory, 200);
        }

        #region Private Methods
        private static IEnumerable<ObjectRow> GetRows(int maxRowCount)
        {
            const string col1 = "col1";
            const string col2 = "col2";
            const string col3 = "col3";

            var definitions = new []
            {
                new ObjectRow.PropertyDefinition(col1, typeof(int)),
                new ObjectRow.PropertyDefinition(col2, typeof(int)),
                new ObjectRow.PropertyDefinition(col3, typeof(int))
            };

            var values = new[]
            {
                new ObjectRow.PropertyValue(col1, 1), 
                new ObjectRow.PropertyValue(col2, 1),
                new ObjectRow.PropertyValue(col3, 1)
            };

            for (int i = 0; i < maxRowCount; i++)
            {
                yield return new ObjectRow(definitions);
            }
        }
        #endregion
    }
}