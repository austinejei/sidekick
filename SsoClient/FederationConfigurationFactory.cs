using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.IdentityModel.Services.Configuration;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security;

namespace SsoClient
{
    public static class FederationConfigurationFactory
    {
        public static FederationConfiguration Create(string relyingPartyUrl, string stsUrl, string domain, string certificateThumbprint, string authCookieName, bool requireSsl)
        {
            var federationConfiguration = new FederationConfiguration();
            federationConfiguration.IdentityConfiguration.AudienceRestriction.AllowedAudienceUris.Add(new Uri(relyingPartyUrl));

            var issuingAuthority = new IssuingAuthority(stsUrl);
            issuingAuthority.Thumbprints.Add(certificateThumbprint);
            issuingAuthority.Issuers.Add(stsUrl);
            var issuingAuthorities = new List<IssuingAuthority> { issuingAuthority };

            var validatingIssuerNameRegistry = new ValidatingIssuerNameRegistry { IssuingAuthorities = issuingAuthorities };
            federationConfiguration.IdentityConfiguration.IssuerNameRegistry = validatingIssuerNameRegistry;
            federationConfiguration.IdentityConfiguration.CertificateValidationMode = X509CertificateValidationMode.None;

            var chunkedCookieHandler = new ChunkedCookieHandler
                                       {
                                           RequireSsl = requireSsl,
                                           Name = authCookieName,
                                           Domain = domain,
                                           PersistentSessionLifetime = new TimeSpan(0, 0, 30, 0)
                                       };
            federationConfiguration.CookieHandler = chunkedCookieHandler;
            var issuerOfToken = stsUrl;
            federationConfiguration.WsFederationConfiguration.Issuer = issuerOfToken;
            federationConfiguration.WsFederationConfiguration.Realm = relyingPartyUrl;
            federationConfiguration.WsFederationConfiguration.RequireHttps = requireSsl;

            return federationConfiguration;
        }
    }
}