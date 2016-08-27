using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using NUnit.Framework;

namespace Rucker.Core.Tests
{
    [TestFixture]
    public class PrincipalExtensionsTests
    {
        [Test]
        public void NullPrincipalReturnsNoClaims()
        {
            Assert.IsEmpty((null as IPrincipal).Claims());
            Assert.IsEmpty((null as IPrincipal).ClaimTypes());
        }

        [Test]
        public void GenericPrincipalReturnsIdentityClaim()
        {
            var claim = new GenericPrincipal(new GenericIdentity("mark.rucker", "username"), null).Claims().Single();
            
            Assert.AreEqual(ClaimTypes.Name, claim.Type);
            Assert.AreEqual("mark.rucker", claim.Value);
            Assert.AreEqual("username", claim.Subject.AuthenticationType);
        }

        [Test]
        public void ClaimsPrincipalReturnsIdentityClaim()
        {
            var claims    = new[] {new Claim("system", "admin")};
            var identity  = new GenericIdentity("mark.rucker", "username");
            var principal = new ClaimsPrincipal(new ClaimsIdentity(identity, claims));

            var nameClaim  = principal.Claims().Single(c => c.Type == ClaimTypes.Name);
            var otherClaim = principal.Claims().Except(new [] { nameClaim }).Single();

            Assert.AreEqual(ClaimTypes.Name, nameClaim.Type);
            Assert.AreEqual("mark.rucker"  , nameClaim.Value);
            Assert.AreEqual("username"     , nameClaim.Subject.AuthenticationType);

            Assert.AreEqual("system", otherClaim.Type);
            Assert.AreEqual("admin" , otherClaim.Value);
        }
    }
}