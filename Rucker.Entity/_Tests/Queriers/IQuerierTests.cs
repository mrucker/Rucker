using NUnit.Framework;
using Rucker.Testing;
using Rucker.Data.Tests;

namespace Rucker.Entities.Tests
{
    [TestFixture]
    public class DbQuerierTests: IQuerierTests
    {
        public DbQuerierTests() : base(new DbQuerier(Test.ConnectionString) { Persistent = true })
        {

        }
    }
}