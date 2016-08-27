using NUnit.Framework;
using Rucker.Core.Testing;
using Rucker.Core.Tests;

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