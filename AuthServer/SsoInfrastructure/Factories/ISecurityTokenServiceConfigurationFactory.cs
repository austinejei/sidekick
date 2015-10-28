using System.Collections.Generic;
using System.IdentityModel.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace AuthServer.SsoInfrastructure.Factories
{
    public interface ISecurityTokenServiceConfigurationFactory
    {
        SecurityTokenServiceConfiguration Create(string samlTwoTokenType, string stsName,
                                                                 X509Certificate2 samlTokenSigningCertificate, IEnumerable<string> rpAudiences);
    }
}