using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using System.IO;
using System.Linq;

namespace OldMusicBox.EIH.Tests
{
    /// <summary>
    /// X509 certificate provider for the client.
    /// Client uses the cert to sign SAML2 requests 
    /// sent to the server
    /// </summary>
    public class ClientCertificateProvider
    {
        private static Pkcs12Store _clientSigStore;

        private static Pkcs12Store _clientEncStore;

        private static Pkcs12Store GetEncCertStore()
        {
            if (_clientEncStore == null)
            {
                var path = Directory.GetCurrentDirectory() + @"/Resources/du_enc_ec.p12";
                var pwd = "12345";

                _clientEncStore = new Pkcs12Store();
                using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    _clientEncStore.Load(fs, pwd.ToCharArray());
                }
            }

            return _clientEncStore;
        }

        public static X509Certificate GetEncCertificate()
        {
            var alias = GetEncCertStore().Aliases.Cast<string>().First();
            return GetEncCertStore().GetCertificate(alias).Certificate;
        }

        public static AsymmetricKeyParameter GetEncPrivateKey()
        {
            var alias = GetEncCertStore().Aliases.Cast<string>().First();
            return GetEncCertStore().GetKey(alias).Key;
        }
    }
}