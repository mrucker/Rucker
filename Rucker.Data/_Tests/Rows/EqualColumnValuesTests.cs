using System;
using Rucker.Testing;
using NUnit.Framework;

namespace Rucker.Data.Tests
{
    [TestFixture, System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "UnusedVariable")]
    public class EqualColumnValuesTests
    {
        [Test]
        public void EqualsPerformanceTest()
        {
            const int tenThousand = 10000;

            var row1 = new ObjectRow(new Alphabet());
            var row2 = new ObjectRow(new Alphabet());

            var equalColumnValues = new EqualColumnValues(row1.Columns);

            var executionTime = Test.ExecutionTime(() =>
            {
                for (var i = 0; i < tenThousand; i++)
                {
                    var isEqual = equalColumnValues.Equals(row1, row2);
                }
            });

            var actualMillisecondsPerRow = executionTime.TotalMilliseconds / tenThousand;
            var expectedMillisecondsPerRow = 0.05;

            Console.WriteLine("Milliseconds Per Row: " + actualMillisecondsPerRow);

            //11/20/2015 .0387
            //12/22/2015 .0215

            Assert.Less(actualMillisecondsPerRow, expectedMillisecondsPerRow);
        }       

        [Test]
        public void HashPerformanceTest()
        {
            const int tenThousand = 10000;

            var row1 = new ObjectRow(new Alphabet()).Union(new ObjectRow(new Alphabet()));

            row1.Add("my column", 123);

            var equalColumnValues = new EqualColumnValues(row1.Columns);

            var executionTime = Test.ExecutionTime(() =>
            {
                for (var i = 0; i < tenThousand; i++)
                {
                    var hashcode = equalColumnValues.GetHashCode(row1);
                }
            });

            var actualMillisecondsPerRow = executionTime.TotalMilliseconds / tenThousand;
            var expectedMillisecondsPerRow = 0.05;

            Console.WriteLine("Milliseconds Per Row: " + actualMillisecondsPerRow);

            //11/20/2015 .0279
            //12/22/2015 .0308

            Assert.Less(actualMillisecondsPerRow, expectedMillisecondsPerRow);
        }

        [Test]
        public void MultiColumnEqualWorks()
        {
            var row1 = new ObjectRow(new Alphabet());
            var row2 = new ObjectRow(new Alphabet());
                   
            var equalColumnValues = new EqualColumnValues(row1.Columns);

            Assert.IsTrue(equalColumnValues.Equals(row1, row2));
        }

        [Test]
        public void SingleColumnEqualWorks()
        {
            var row1 = new ObjectRow(new Alphabet());
            var row2 = new ObjectRow(new Alphabet());

            var equalColumnValues = new EqualColumnValues(new[] {"A"});

            Assert.IsTrue(equalColumnValues.Equals(row1, row2));
        }

        [Test]
        public void MultiColumnNotEqualWorks()
        {
            var row1 = new ObjectRow(new Alphabet() { A = "B" });
            var row2 = new ObjectRow(new Alphabet());

            var equalColumnValues = new EqualColumnValues(row1.Columns);

            Assert.IsFalse(equalColumnValues.Equals(row1, row2));
        }

        [Test]
        public void SingleColumnNotEqualhWorks()
        {
            var row1 = new ObjectRow(new Alphabet() {A = "B"});
            var row2 = new ObjectRow(new Alphabet());

            var equalColumnValues = new EqualColumnValues(new[] { "A" });

            Assert.IsFalse(equalColumnValues.Equals(row1, row2));
        }

        [Test]
        public void MultiColumnHashWorks()
        {
            var row1 = new ObjectRow(new Alphabet());
            var row2 = new ObjectRow(new Alphabet());

            var equalColumnValues = new EqualColumnValues(row1.Columns);

            Assert.AreEqual(equalColumnValues.GetHashCode(row1), equalColumnValues.GetHashCode(row2));
        }

        [Test]
        public void SingleColumnHashWorks()
        {
            var row1 = new ObjectRow(new Alphabet());
            var row2 = new ObjectRow(new Alphabet());

            var equalColumnValues = new EqualColumnValues(new[] { "A" });

            Assert.AreEqual(equalColumnValues.GetHashCode(row1), equalColumnValues.GetHashCode(row2));
        }

        [Test]
        public void MultiColumnNotHashWorks()
        {
            var row1 = new ObjectRow(new Alphabet() { A = "B" });
            var row2 = new ObjectRow(new Alphabet());

            var equalColumnValues = new EqualColumnValues(row1.Columns);

            Assert.AreNotEqual(equalColumnValues.GetHashCode(row1), equalColumnValues.GetHashCode(row2));
        }

        [Test]
        public void SingleColumnNotHashWorks()
        {
            var row1 = new ObjectRow(new Alphabet() { A = "B" });
            var row2 = new ObjectRow(new Alphabet());

            var equalColumnValues = new EqualColumnValues(new[] { "A" });

            Assert.AreNotEqual(equalColumnValues.GetHashCode(row1), equalColumnValues.GetHashCode(row2));
        }
    }
}