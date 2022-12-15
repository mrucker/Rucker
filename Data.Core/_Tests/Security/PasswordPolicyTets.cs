using NUnit.Framework;

namespace Data.Core.Tests
{
    [TestFixture]
    public class PasswordPolicyTets
    {
        [TestCase("Password", true , true )]
        [TestCase("A"       , true , true )]
        [TestCase("123A"    , true , true )]
        [TestCase("!_*A"    , true , true )]
        [TestCase("Password", false, true )]
        [TestCase("A"       , false, true )]
        [TestCase("123A"    , false, true )]
        [TestCase("!_*A"    , false, true )]
        [TestCase("password", false, true )]
        [TestCase("a"       , false, true )]
        [TestCase("123a"    , false, true )]
        [TestCase("!_*a"    , false, true )]
        [TestCase("password", true , false)]
        [TestCase("a"       , true , false)]
        [TestCase("123a"    , true , false)]
        [TestCase("!_*a"    , true , false)]
        public void RequireUpperCaseTest(string password, bool require, bool meetsPolicy)
        {
            var policy = new PasswordPolicy(false, require, false, false, 0, 64);

            Assert.AreEqual(meetsPolicy, policy.PasswordMeetsPolicy(password));
        }

        [TestCase("PASSWORD", true , false)]
        [TestCase("A"       , true , false)]
        [TestCase("123A"    , true , false)]
        [TestCase("!_*A"    , true , false)]
        [TestCase("Password", false, true )]
        [TestCase("A"       , false, true )]
        [TestCase("123A"    , false, true )]
        [TestCase("!_*A"    , false, true )]
        [TestCase("password", false, true )]
        [TestCase("a"       , false, true )]
        [TestCase("123a"    , false, true )]
        [TestCase("!_*a"    , false, true )]
        [TestCase("password", true , true )]
        [TestCase("a"       , true , true )]
        [TestCase("123a"    , true , true )]
        [TestCase("!_*a"    , true , true )]
        public void RequireLowerCaseTest(string password, bool require, bool meetsPolicy)
        {
            var policy = new PasswordPolicy(require, false, false, false, 0, 64);

            Assert.AreEqual(meetsPolicy, policy.PasswordMeetsPolicy(password));
        }

        [TestCase("PASSWORD", true , false)]
        [TestCase("A"       , true , false)]
        [TestCase("123A"    , true , false)]
        [TestCase("!_*A"    , true , true )]
        [TestCase("Password", false, true )]
        [TestCase("A"       , false, true )]
        [TestCase("123A"    , false, true )]
        [TestCase("!_*A"    , false, true )]
        [TestCase("password", false, true )]
        [TestCase("a"       , false, true )]
        [TestCase("123a"    , false, true )]
        [TestCase("!_*a"    , false, true )]
        [TestCase("password", true , false)]
        [TestCase("a"       , true , false)]
        [TestCase("123a"    , true , false)]
        [TestCase("!_*a"    , true , true )]
        [TestCase("!"       , true , true )]
        [TestCase("@"       , true , true )]
        [TestCase("#"       , true , true )]
        [TestCase("$"       , true , true )]
        [TestCase(")"       , true , true )]
        [TestCase("_"       , true , true )]
        [TestCase("~"       , true , true )]
        [TestCase(","       , true , true )]
        public void RequireSymbolsTest(string password, bool require, bool meetsPolicy)
        {
            var policy = new PasswordPolicy(false, false, false, require, 0, 64);

            Assert.AreEqual(meetsPolicy, policy.PasswordMeetsPolicy(password));
        }

        [TestCase("Password", true , false)]
        [TestCase("A"       , true , false)]
        [TestCase("123A"    , true , true )]
        [TestCase("!_*A0"   , true , true )]
        [TestCase("Password", false, true )]
        [TestCase("A"       , false, true )]
        [TestCase("123A"    , false, true )]
        [TestCase("!_*A"    , false, true )]
        [TestCase("password", false, true )]
        [TestCase("a"       , false, true )]
        [TestCase("123a"    , false, true )]
        [TestCase("!_*a"    , false, true )]
        [TestCase("password", true , false)]
        [TestCase("a"       , true , false)]
        [TestCase("123a"    , true , true )]
        [TestCase("0.0"     , true , true )]
        public void RequireNumbersTest(string password, bool require, bool meetsPolicy)
        {
            var policy = new PasswordPolicy(false, false, require, false, 0, 64);

            Assert.AreEqual(meetsPolicy, policy.PasswordMeetsPolicy(password));
        }

        [TestCase("Password", 5, true )]
        [TestCase("A"       , 5, false)]
        [TestCase("123A"    , 5, false)]
        [TestCase("!_*A0"   , 5, true )]
        [TestCase("Password", 5, true )]
        [TestCase("A"       , 5, false)]
        [TestCase("123A"    , 5, false)]
        [TestCase("!_*A"    , 5, false)]
        [TestCase("password", 5, true )]
        [TestCase("a"       , 5, false)]
        [TestCase("123a"    , 5, false)]
        [TestCase("!_*a,"   , 5, true )]
        [TestCase("password", 5, true )]
        [TestCase("a"       , 5, false)]
        [TestCase("123a"    , 5, false)]
        [TestCase("0.0"     , 5, false)]
        public void RequireMinimumTest(string password, int minimum, bool meetsPolicy)
        {
            var policy = new PasswordPolicy(false, false, false, false, minimum, 64);

            Assert.AreEqual(meetsPolicy, policy.PasswordMeetsPolicy(password));
        }
    }
}