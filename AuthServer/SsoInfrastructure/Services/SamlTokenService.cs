using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Services;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using AuthServer.SsoInfrastructure.Factories;
using AuthServer.SsoInfrastructure.Managers;

namespace AuthServer.SsoInfrastructure.Services
{
    public class SamlTokenService : ISamlTokenService
    {
        private readonly IRealmTracker realmTracker;
        private readonly ISecurityTokenServiceConfigurationFactory configurationFactory;

        public SamlTokenService(
            IRealmTracker realmTracker, 
            ISecurityTokenServiceConfigurationFactory configurationFactory)
        {
            this.realmTracker = realmTracker;
            this.configurationFactory = configurationFactory;
        }

        public SignInResponseMessage CreateResponseContainingToken(Uri currrentRequestUri)
        {
            var message = WSFederationMessage.CreateFromUri(currrentRequestUri);
            
            var signinMessage = message as SignInRequestMessage;
            if (signinMessage != null)
            {
                return CreateSigninReponseMessage(signinMessage);
            }

            throw new SecurityException("We will not process any other types of message here.");
        }

        private SignInResponseMessage CreateSigninReponseMessage(SignInRequestMessage signInRequestMessage)
        {
            //get from config...
            const string SamlTwoTokenType = "urn:oasis:names:tc:SAML:2.0:assertion";
            const bool RequireSsl = false;

            var allowedRpAudiences = GetAuthorisedAudiencesWeCanIssueTokensTo();
            var samlTokenSigningCertificate = GetSamlTokenSigningCertificate();
            var stsConfiguration = configurationFactory.Create(SamlTwoTokenType, "http://sidekick.local/sso/token", samlTokenSigningCertificate, allowedRpAudiences);
            var tokenService = stsConfiguration.CreateSecurityTokenService();
            
            var signInResponseMessage = FederatedPassiveSecurityTokenServiceOperations.ProcessSignInRequest(signInRequestMessage, 
                ClaimsPrincipal.Current, tokenService);
            this.realmTracker.AddNewRealm(signInRequestMessage.Realm);

           //sanity check 
            ValidateRequestIsSsl(RequireSsl, signInRequestMessage);

            return signInResponseMessage;
        }

        private static void ValidateRequestIsSsl(bool requireSsl, SignInRequestMessage signInRequestMessage)
        {
            if (requireSsl && (signInRequestMessage.BaseUri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidRequestException("requests needs to be ssl");
            }
        }

        private static IEnumerable<string> GetAuthorisedAudiencesWeCanIssueTokensTo()
        {
            return new List<string> { InfrastructureConstants.Rp1Url, InfrastructureConstants.Rp2Url };
        }

        private static X509Certificate2 GetSamlTokenSigningCertificate()
        {
            var certificateManager = new CertificateManager();
            var samlTokenSigningCertificate = certificateManager.GetCertificate();
            return samlTokenSigningCertificate;
        }
    }
}