using NUnit.Framework;
using Rucker.Data;
using Rucker.Testing;

namespace Rucker.Entities.Tests.Data.Queriers
{
    [TestFixture]
    public class IQuerierTests: Rucker.Tests.IQuerierTests
    {
        public IQuerierTests() : base(new DbQuerier(Test.ConnectionString))
        {

        }
    }
}