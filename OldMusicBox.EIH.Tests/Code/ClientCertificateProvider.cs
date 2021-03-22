using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using System.Collections.Generic;
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
        private static Dictionary<string, Pkcs12Store> _clientStore = new Dictionary<string, Pkcs12Store>();

        private static Pkcs12Store GetCertStore(string certName)
        {
            if (!_clientStore.ContainsKey(certName))
            {
                var path = Directory.GetCurrentDirectory() + $@"/Resources/{certName}.p12";
                var pwd = "12345";

                _clientStore.Add(certName, new Pkcs12Store());
                using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    _clientStore[certName].Load(fs, pwd.ToCharArray());
                }
            }

            return _clientStore[certName];
        }

        public static X509Certificate GetCertificate(string certName)
        {
            var alias = GetCertStore(certName).Aliases.Cast<string>().First();
            return GetCertStore(certName).GetCertificate(alias).Certificate;
        }

        public static AsymmetricKeyParameter GetPrivateKey(string certName)
        {
            var alias = GetCertStore(certName).Aliases.Cast<string>().First();
            return GetCertStore(certName).GetKey(alias).Key;
        }
    }
}