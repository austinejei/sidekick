using System.IdentityModel;
using System.IdentityModel.Configuration;
using System.IdentityModel.Protocols.WSTrust;
using System.Security.Claims;

namespace AuthServer.SsoInfrastructure.Services
{
    public class SystemIdentityTokenService : SecurityTokenService
    {
        private readonly SecurityTokenServiceConfiguration securityTokenServiceConfiguration;
        const string SamlTwoTokenType = "urn:oasis:names:tc:SAML:2.0:assertion";

        public SystemIdentityTokenService(SecurityTokenServiceConfiguration securityTokenServiceConfiguration) :
            base(securityTokenServiceConfiguration)
        {
            this.securityTokenServiceConfiguration = securityTokenServiceConfiguration;
        }

        protected override Scope GetScope(ClaimsPrincipal claimsPrincipal, RequestSecurityToken requestSecurityToken)
        {
            if (requestSecurityToken.AppliesTo == null)
            {
                throw new InvalidRequestException("Request for security token does not have a realm.");
            }
            var scope = new Scope(requestSecurityToken.AppliesTo.Uri.AbsoluteUri,
                this.securityTokenServiceConfiguration.SigningCredentials)
            {
                TokenEncryptionRequired = false,
                ReplyToAddress = requestSecurityToken.AppliesTo.Uri.AbsoluteUri
            };

            requestSecurityToken.TokenType = SamlTwoTokenType;

            return scope;
        }

        protected override ClaimsIdentity GetOutputClaimsIdentity(ClaimsPrincipal principal, RequestSecurityToken request, Scope scope)
        {
            return (ClaimsIdentity) principal.Identity;
        }
    }
}
