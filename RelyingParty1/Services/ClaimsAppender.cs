namespace RelyingParty1.Services
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;

    public class ClaimsAppender : ClaimsAuthenticationManager
    {
        public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            if (!incomingPrincipal.Identity.IsAuthenticated)
            {
                return base.Authenticate(resourceName, incomingPrincipal);
            }
           // var roles = GetExchangeRole(incomingPrincipal);
            //var newPrincipal = CreateNewClaimsPrincipal(roles, (ClaimsIdentity) incomingPrincipal.Identity);

            //assign this principal to the thread and serialize
           // return base.Authenticate(resourceName, newPrincipal);
            return base.Authenticate(resourceName, incomingPrincipal);
        }

        private static IEnumerable<string> GetExchangeRole(IPrincipal incomingPrincipal)
        {
            var claimsIdentity = ((ClaimsIdentity)incomingPrincipal.Identity);
            return GetSchemeReturnRoles(1);
        }

        private static ClaimsPrincipal CreateNewClaimsPrincipal(IEnumerable<string> roles, ClaimsIdentity incomingIdentity)
        {
            var newClaims = incomingIdentity.Claims.ToList();
            newClaims.AddRange(roles.Select(report => new Claim("SomeRP1Role", report.ToString(CultureInfo.InvariantCulture))));

            var newClaimsIdentity = new ClaimsIdentity(newClaims, "Federation");
            return new ClaimsPrincipal(newClaimsIdentity);
        }

        private static IEnumerable<string> GetSchemeReturnRoles(int accountId)
        {
            //look in rp database for domain specific claims
            return new List<string> { "RP1Manager", "RP1User" };
        }
    }
}