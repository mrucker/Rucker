using NUnit.Framework;
using Data.Core.Testing;
using Data.Core.Tests;

namespace Data.Sql.Tests
{
    [TestFixture]
    public class SqlQuerierTests: IQuerierTests
    {
        public SqlQuerierTests() : base(new SqlQuerier(Test.ConnectionString))
        {

        }
    }
}