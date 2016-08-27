using System;
using System.Linq;
using NUnit.Framework;

namespace Rucker.Data.Tests
{
    [TestFixture]
    public class UriTests
    {
        [TestCase("1://1", "1", new[] { "1" })]
        [TestCase("abc://1/2/3", "abc", new[] {"1","2","3"})]
        [TestCase("abc://1/a_1/b", "abc", new[] { "1", "a_1", "b" })]
        public void UriWithoutQuery(string uriString, string scheme, string[] prts)
        {
            var uri = new BaseUri(uriString);

            Assert.AreEqual(scheme, uri.Scheme);
            Assert.IsTrue(prts.SequenceEqual(uri.Parts));
            Assert.IsEmpty(uri.Query);
        }

        [TestCase("a://1?a=1", "a", new[] {"1"}, new[] {"a"}, new[] {"1"})]
        [TestCase("ab://1/2?a=1&b=2", "ab", new[] { "1", "2" }, new[] { "a", "b" }, new[] { "1", "2" })]
        public void UriWithQuery(string uriString, string scheme, string[] prts, string[] keys, string[] vals)
        {
            var uri = new BaseUri(uriString);

            Assert.AreEqual(scheme, uri.Scheme);
            Assert.IsTrue(prts.SequenceEqual(uri.Parts));
            Assert.IsTrue(keys.SequenceEqual(uri.Query.Keys));
            Assert.IsTrue(vals.SequenceEqual(uri.Query.Values));
        }

        [TestCase("://")]
        [TestCase("://?")]
        [TestCase("a://")]
        [TestCase("://1")]
        [TestCase("a://1?")]
        [TestCase("a://?")]
        public void PoorlyFormedUris(string uriString)
        {
            Assert.Throws(Is.InstanceOf<Exception>(), () => new BaseUri(uriString) );
        }
    }
}