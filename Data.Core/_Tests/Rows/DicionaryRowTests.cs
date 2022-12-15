using System;
using System.Linq;
using Data.Core.Testing;
using NUnit.Framework;

namespace Data.Core.Tests
{
    public class DicionaryRowTests
    {
        [Test]
        public void ColumnAccessorTest()
        {
            var row = new DictionaryRow
            {
                {"FirstName"    , "John"},
                {"LastName"     , "Doe"},
                {"City"         , "Oklahoma City"},
                {"State"        , "Oklahoma"},
                {"Subscriptions", "10000000"},
                {"AddressLine1" , "20000 North West Something Drive"},
                {"AddressLine2" , "Apartment 3000"},
                {"AddressLine3" , ""},
                {"AddressLine4" , ""},
            };

            Assert.AreEqual(row["FirstName"]    , "John");
            Assert.AreEqual(row["LastName"]     , "Doe");
            Assert.AreEqual(row["City"]         , "Oklahoma City");
            Assert.AreEqual(row["State"]        , "Oklahoma");
            Assert.AreEqual(row["Subscriptions"], "10000000");
            Assert.AreEqual(row["AddressLine1"] , "20000 North West Something Drive");
            Assert.AreEqual(row["AddressLine2"] , "Apartment 3000");
            Assert.AreEqual(row["AddressLine3"] , "");
            Assert.AreEqual(row["AddressLine4"] , "");
        }

        [Test]
        public void InitializationPerformanceTest()
        {
            const int tenThousand = 10000;
            
            var executionTime = Test.ExecutionTime(() => Enumerable.Range(0, tenThousand).Select(LargeDictionaryRow).ToArray());

            var actualMillisecondsPerRow   = executionTime.TotalMilliseconds / tenThousand;
            var expectedMillisecondsPerRow = 0.05;

            Console.WriteLine("Milliseconds Per Row: " + actualMillisecondsPerRow);

            //.0001
            Assert.Less(actualMillisecondsPerRow, expectedMillisecondsPerRow);
        }

        [Test]
        public void MemoryTest()
        {
            const int twoMillion = 2000000;

            var usedMemory = Test.ExecutionBits(Enumerable.Range(0, twoMillion).Select(SmallDictionaryRow).ToArray) / Test.MegaByte;
            
            Console.WriteLine("UsedMemory: " + usedMemory);
            
            //8-27-2014: 625
            Assert.Less(usedMemory, 650);
        }

        private static DictionaryRow SmallDictionaryRow(int i)
        {
            return new DictionaryRow { { "col1", i }, { "col2", i }, { "col3", i } };
        }

        private static DictionaryRow LargeDictionaryRow(int i)
        {
            return new DictionaryRow
            {
                {"A", "A"}, {"B", "B"}, {"C", "C"}, {"D", "D"}, {"E", "E"}, 
                {"F", "F"}, {"G", "G"}, {"H", "H"}, {"I", "I"}, {"J", "J"},
                {"K", "K"}, {"L", "L"}, {"M", "M"}, {"N", "N"}, {"O", "O"},
                {"P", "P"}, {"Q", "Q"}, {"R", "R"}, {"S", "S"}, {"T", "T"},
                {"U", "U"}, {"V", "V"}, {"W", "W"}, {"X", "X"}, {"Y", "Y"}, {"Z", "Z"}
            };
        }
    }
}