using System;
using Rucker.Convert;
using NUnit.Framework;

namespace Rucker.Tests
{
    [TestFixture]
    public class ConvertTests
    {
        [Test]
        public void ValidValueToSignedNumeric(
            [Values(typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal))]Type type,
            [Values(true, false)] bool nullable,
            [Values(10, -10, "10", "+10", "-10")] object source)
        {
            var dest    = source.To((nullable) ? typeof(Nullable<>).MakeGenericType(type) : type);
            var isEqual = (source.ToString().Contains("-")) ? Is.EqualTo(-10) : Is.EqualTo(10);

            Assert.That(dest, isEqual.And.TypeOf(type));
        }

        [Test]
        public void ValidValueToSignedDecimal(
            [Values(typeof(float), typeof(double), typeof(decimal))] Type type,
            [Values(true, false)] bool nullable,
            [Values(10.5, -10.5, "10.5", "+10.5", "-10.5")] object source)
        {
            var dest = source.To((nullable) ? typeof(Nullable<>).MakeGenericType(type) : type);

            //We use a range because depending on the decimal type not all representations are exact (e.g. A 10.5 float source equals 10.4999999...)
            var isInRange = (source.ToString().Contains("-")) ? Is.InRange(-10.5, -10.4999) : Is.InRange(10.4999, 10.5);

            Assert.That(dest, isInRange.And.TypeOf(type));
        }

        [Test]
        public void ValidValueToBoolean([Values(true, false)] bool nullable, [Values(true, false, "true", "false", "TRUE", "False", 1, 0)] object source)
        {
            var dest         = (nullable) ? source.To<bool?>() : source.To<bool>();
            var sourceAsBool = source.ToString().ToLower().Equals("true") || source.ToString() == "1";

            Assert.That(dest, Is.EqualTo(sourceAsBool));
            Assert.That(dest, Is.TypeOf<bool>());
        }

        [Test]
        public void ValidValueToString([Values(true, "true", 10, "", 'A', 10.5, -10)] object source)
        {
            var dest = source.To<string>();
                        
            Assert.That(dest, Is.EqualTo(source.ToString()));
            Assert.That(dest, Is.TypeOf<string>());
        }

        [Test]
        public void ValidValueToEnum([Values(true, false)] bool nullable, [Values(DayOfWeek.Monday, 2, "Tuesday")] object source)
        {
            var dest         = nullable ? source.To<DayOfWeek?>() : source.To<DayOfWeek>();
            var sourceAsEnum = Enum.Parse(typeof(DayOfWeek), source.ToString());

            Assert.That(dest, Is.EqualTo(sourceAsEnum));
            Assert.That(dest, Is.TypeOf<DayOfWeek>());
        }

        [Test]
        public void ValidValueToDate([Values(true, false)] bool nullable, [Values("3/3/2012", "03/03/12", "3-3-2012")] object source)
        {
            var dest = nullable ? source.To<DateTime?>() : source.To<DateTime>();

            Assert.That(dest, Is.EqualTo(new DateTime(2012,3,3)).And.TypeOf<DateTime>());
        }

        [Test]
        public void NullValueToNullable([Values(typeof(DateTime?), typeof(DayOfWeek?), typeof(int?), typeof(float?), typeof(double?), typeof(decimal?), typeof(long?), typeof(bool?), typeof(string))]Type type)
        {
            Assert.That((null as object).To(type), Is.EqualTo(null));
        }

        [Test]
        public void NullValueToNonNullableFails([Values(typeof(DateTime), typeof(DayOfWeek), typeof(int), typeof(float), typeof(double), typeof(decimal), typeof(long), typeof(bool))]Type type)
        {
            Assert.Throws(Is.TypeOf<Exception>().And.Message.Contains("null is not a valid value for"), () => Convert.Convert.To(null, type));
        }

        public void InvalidValueToNumericFails([Values(typeof(int), typeof(float), typeof(double), typeof(decimal))]Type type, [Values(true, false)] bool nullable)
        {
            Assert.Throws(Is.TypeOf<Exception>().And.Message.Contains("'A' is not a valid value for"), () => "A".To((nullable) ? typeof(Nullable<>).MakeGenericType(type) : type));
        }

        public void InvalidValueToDateFails1([Values(true, false)] bool nullable, [Values("0/0/2012")] object source)
        {
            Assert.Throws(Is.TypeOf<Exception>().And.Message.Contains("0/0/2012' is not a valid value for"), () => source.To((nullable) ? typeof(DateTime?) : typeof(DateTime)));
        }

        public void InvalidValueToDateFails2([Values(true, false)] bool nullable, [Values("", "   ")] object source)
        {
            Assert.Throws(Is.TypeOf<Exception>().And.Message.Contains("'' is not a valid value for"), () => source.To((nullable) ? typeof(DateTime?) : typeof(DateTime)));
        }

        [Test]
        public void ValueToNullableTypeWorks()
        {
            var conversion = "1".To<int?>();

            Assert.That(conversion, Is.EqualTo(1));
        }

        [Test]
        public void StructTypeWorks()
        {
            Assert.That("1".To<int>(), Is.EqualTo(1));
        }

        [Test]
        public void ClassTypeWorks()
        {
            Assert.That("1".To<string>(), Is.EqualTo("1"));
        }

        [Test]
        public void EnumTypeWorks()
        {
            Assert.That("Monday".To<DayOfWeek>(), Is.EqualTo(DayOfWeek.Monday));
        }
    }
}