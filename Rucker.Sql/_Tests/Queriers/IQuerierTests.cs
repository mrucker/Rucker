using NUnit.Framework;
using Rucker.Testing;
using Rucker.Data.Tests;

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