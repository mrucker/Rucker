using NUnit.Framework;
using Data.Core.Testing;
using Data.Core.Tests;

namespace Data.Entities.Tests
{
    [TestFixture]
    public class DbQuerierTests: IQuerierTests
    {
        public DbQuerierTests() : base(new DbQuerier(Test.ConnectionString) { Persistent = true })
        {

        }
    }
}