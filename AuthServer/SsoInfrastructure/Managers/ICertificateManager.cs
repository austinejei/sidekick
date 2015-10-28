using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AuthServer.SsoInfrastructure.Managers
{
    public interface ICertificateManager
    {
        RSACryptoServiceProvider PrivateKey { get; }

        X509Certificate2 GetCertificate();
    }
}