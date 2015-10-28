using System;
using System.Collections.Generic;
using System.IdentityModel.Configuration;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using AuthServer.SsoInfrastructure.Services;

namespace AuthServer.SsoInfrastructure.Factories
{
    public class SecurityTokenServiceConfigurationFactory : ISecurityTokenServiceConfigurationFactory
    {
        public SecurityTokenServiceConfiguration Create(string samlTwoTokenType, string stsName,
           X509Certificate2 samlTokenSigningCertificate, IEnumerable<string> rpAudiences)
        {
            var stsConfiguration = new SecurityTokenServiceConfiguration
            {
                DefaultTokenLifetime = new TimeSpan(0, 0, 30, 0),
                MaximumTokenLifetime = new TimeSpan(0, 0, 30, 0),
                DefaultTokenType = samlTwoTokenType,
                TokenIssuerName = stsName,
                SigningCredentials = new X509SigningCredentials(samlTokenSigningCertificate)
            };

            foreach (var rpAudience in rpAudiences)
            {
                stsConfiguration.AudienceRestriction.AllowedAudienceUris.Add(new Uri(rpAudience));
            }
            stsConfiguration.AudienceRestriction.AudienceMode = AudienceUriMode.Always;
            stsConfiguration.SecurityTokenService = typeof(SystemIdentityTokenService);

            return stsConfiguration;
        }   
    }
}