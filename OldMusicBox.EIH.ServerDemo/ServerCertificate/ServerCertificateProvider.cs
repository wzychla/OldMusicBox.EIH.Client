using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace OldMusicBox.EIH.ServerDemo
{
    /// <summary>
    /// X509 certificate provider for the client.
    /// Client uses the cert to sign SAML2 requests 
    /// sent to the server
    /// </summary>
    public class ServerCertificateProvider
    {
        #region Signatures

        private static Pkcs12Store _clientSigStore;

        private static Pkcs12Store GetSigCertStore()
        {
            if (_clientSigStore == null)
            {
                var path = HostingEnvironment.MapPath("~/ServerCertificate/ExampleSigningCertificate.p12");
                _clientSigStore = new Pkcs12Store();
                using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    _clientSigStore.Load(fs, "123456".ToCharArray());
                }
            }

            return _clientSigStore;
        }

        public static X509Certificate GetSigCertificate()
        {
            var alias = GetSigCertStore().Aliases.Cast<string>().First();
            return GetSigCertStore().GetCertificate(alias).Certificate;
        }

        public static AsymmetricKeyParameter GetSigPrivateKey()
        {
            var alias = GetSigCertStore().Aliases.Cast<string>().First();
            return GetSigCertStore().GetKey(alias).Key;
        }

        #endregion

        #region Encryption

        private static Pkcs12Store _clientEncStore;

        private static Pkcs12Store GetEncCertStore()
        {
            if (_clientEncStore == null)
            {
                var path = HostingEnvironment.MapPath("~/ServerCertificate/ExampleEncryptionCertificate.p12");
                _clientEncStore = new Pkcs12Store();
                using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    _clientEncStore.Load(fs, "123456".ToCharArray());
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

        #endregion

        #region Client public

        private static AsymmetricKeyParameter _clientPubStore;

        public static AsymmetricKeyParameter GetClientCertificate()
        {
            if (_clientPubStore == null)
            {
                var path = HostingEnvironment.MapPath("~/ServerCertificate/clientencryption.pem");
                using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var tr = new StreamReader(fs))
                {
                    var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(tr);
                    _clientPubStore = (AsymmetricKeyParameter)pemReader.ReadObject();
                }
            }

            return _clientPubStore;
        }

        #endregion
    }
}