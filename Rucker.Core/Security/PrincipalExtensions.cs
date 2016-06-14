using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Collections.Generic;

namespace Rucker.Security
{
    public static class PrincipalExtensions
    {
        public static IEnumerable<Claim> Claims(this IPrincipal principal)
        {
            return (principal as ClaimsPrincipal)?.Claims ?? Enumerable.Empty<Claim>();
        }

        public static IEnumerable<string> ClaimTypes(this IPrincipal principal)
        {
            return principal.Claims().Select(c => c.Type);
        }
    }
}