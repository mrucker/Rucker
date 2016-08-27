using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Rucker.Core.Testing;
using NUnit.Framework;



namespace Rucker.Core.Tests
{
    [TestFixture(typeof(DictionaryRow))]
    [TestFixture(typeof(ObjectRow))]
    [TestFixture(typeof(WithoutColumnsRow))]
    [TestFixture(typeof(UnionRow))]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class IRowTests
    {
        private readonly Type _rowType;
        private IRow _row;

        public IRowTests(Type rowType)
        {
            _rowType = rowType;
        }

        [SetUp]
        public void Setup()
        {
            if (_rowType == typeof (DictionaryRow))
            {
                _row = new DictionaryRow();
                for (var i = 'A'; i <= 'Z'; i++)
                {
                    _row.Add(i.ToString(), i.ToString());
                }
            }

            if (_rowType == typeof (ObjectRow))
            {
                _row = new ObjectRow(new Alphabet());
            }

            if (_rowType == typeof (WithoutColumnsRow))
            {
                _row = new WithoutColumnsRow(new ObjectRow(new Alphabet()), Enumerable.Empty<string>());
            }

            if (_rowType == typeof (UnionRow))
            {
                var row1 = new DictionaryRow();
                var row2 = new DictionaryRow();

                for (var i = 'A'; i <= 'L'; i++) row1.Add(i.ToString(), i.ToString());
                for (var i = 'M'; i <= 'Z'; i++) row2.Add(i.ToString(), i.ToString());

                _row = new UnionRow(row1, row2);
            }
        }

        [Test]
        public void ColumnReadTest()
        {
            for (var i = 'A'; i <= 'Z'; i++)
            {
                Assert.AreEqual(i.ToString(), _row[i.ToString()]);
            }

            Assert.AreEqual(26, _row.Columns.Count());
        }

        [Test]
        public void ColumnWriteTest()
        {            
            for (var i = 'A'; i <= 'Z'; i++)
            {
                var newValue = i + "1";

                _row[i.ToString()] = newValue;

                Assert.AreEqual(newValue, _row[i.ToString()]);
            }

            Assert.AreEqual(26, _row.Columns.Count());
        }

        [Test]
        public void ColumnAddTest()
        {
            _row.Add("abc", 123);

            Assert.AreEqual("123", _row["abc"]);
        }

        [Test]
        public void WithoutColumnsTest()
        {
            Assert.AreEqual(26, _row.Columns.Count());
            Assert.IsFalse(_row.Columns.Missing("A"));
            Assert.IsFalse(_row.Columns.Missing("B"));
            Assert.IsFalse(_row.Columns.Missing("Z"));
            Assert.AreEqual("A", _row["A"]);
            Assert.AreEqual("B", _row["B"]);
            Assert.AreEqual("Z", _row["Z"]);

            _row = _row.WithoutColumns(new[] {"A", "Z"});

            Assert.AreEqual(24, _row.Columns.Count());
            Assert.IsTrue (_row.Columns.Missing("A"));
            Assert.IsFalse(_row.Columns.Missing("B"));
            Assert.IsTrue (_row.Columns.Missing("Z"));
            Assert.Throws<ColumnNotFoundException>(() => { var readA = _row["A"]; });
            Assert.AreEqual("B", _row["B"]);
            Assert.Throws<ColumnNotFoundException>(() => { var readZ = _row["Z"]; });
        }

        [Test]
        public void WithColumnsTest()
        {
            Assert.AreEqual(26, _row.Columns.Count());
            Assert.IsTrue(_row.Columns.Contains("A"));
            Assert.IsTrue(_row.Columns.Contains("B"));
            Assert.IsTrue(_row.Columns.Contains("Z"));
            Assert.AreEqual("A", _row["A"]);
            Assert.AreEqual("B", _row["B"]);
            Assert.AreEqual("Z", _row["Z"]);

            _row = _row.WithColumns(new[] { "A", "Z" });

            Assert.AreEqual(2, _row.Columns.Count());
            Assert.IsTrue (_row.Columns.Contains("A"));
            Assert.IsFalse(_row.Columns.Contains("B"));
            Assert.IsTrue (_row.Columns.Contains("Z"));
            Assert.AreEqual("A", _row["A"]);
            Assert.Throws<ColumnNotFoundException>(() => { var readB = _row["B"]; });
            Assert.AreEqual("Z", _row["Z"]);            
        }

        [Test]
        public void UnionRowsTest()
        {
            Assert.AreEqual(26, _row.Columns.Count());
            Assert.IsTrue(_row.Columns.Missing("A2"));            

            _row = _row.Union(new DictionaryRow {{"A2", "A2"}});

            Assert.AreEqual(27, _row.Columns.Count());
            Assert.IsFalse(_row.Columns.Missing("A2"));

            var read = _row["A2"];
        }

        [Test]
        public void CaselessMatches()
        {
            Assert.AreEqual("A", _row.Caseless("a"));
            Assert.AreEqual("A", _row.Caseless("A"));
            Assert.AreEqual("B", _row.Caseless("b"));
            Assert.AreEqual("B", _row.Caseless("B"));
        }
    }
}