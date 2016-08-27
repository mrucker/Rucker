using NUnit.Framework;
using Rucker.Core.Testing;
using Rucker.Core.Tests;

namespace Rucker.Sql.Tests
{
    [TestFixture]
    public class SqlQuerierTests: IQuerierTests
    {
        public SqlQuerierTests() : base(new SqlQuerier(Test.ConnectionString))
        {

        }
    }
}