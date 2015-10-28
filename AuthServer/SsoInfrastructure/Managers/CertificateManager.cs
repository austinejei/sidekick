using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AuthServer.SsoInfrastructure.Managers
{
    public class CertificateManager : ICertificateManager
    {
        private const string FriendlyCertificateName = "ssocert";

        private static readonly object SyncRoot = new object();

        private static X509Certificate2 certificate;

        public RSACryptoServiceProvider PrivateKey
        {
            get
            {
                lock (SyncRoot)
                {
                    if (certificate == null)
                    {
                        certificate = GetCertificate(
                            FriendlyCertificateName,
                            StoreName.CertificateAuthority,
                            StoreLocation.CurrentUser);
                    }
                }

                var crypt = certificate.PrivateKey as RSACryptoServiceProvider;

                if (crypt == null)
                {
                    throw new ArgumentException("Certificate private key could not be cast as a RSACryptoServiceProvider");
                }

                return crypt;
            }
        }

        public X509Certificate2 GetCertificate()
        {
            if (certificate != null)
            {
                return certificate;
            }

            lock (SyncRoot)
            {
                return certificate
                       ?? (certificate =
                           GetCertificate(
                               FriendlyCertificateName,
                               StoreName.My,
                               StoreLocation.LocalMachine));
            }
        }

        private static X509Certificate2 GetCertificate(
            string certificateName,
            StoreName storeName,
            StoreLocation storeLocation)
        {
            var x509Store = new X509Store(storeName, storeLocation);
            x509Store.Open(OpenFlags.ReadOnly);

            var returnCertificate = x509Store.Certificates.Cast<X509Certificate2>()
                .FirstOrDefault(x509Certificate2 => x509Certificate2.FriendlyName.Contains(certificateName));

            if (returnCertificate == null)
            {
                throw new ApplicationException(
                    string.Format("Cannot find certificate: {0} in store: {1} ", certificateName, storeName));
            }

            x509Store.Close();
            return returnCertificate;
        }
    }
}
