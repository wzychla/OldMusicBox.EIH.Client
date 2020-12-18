using OldMusicBox.EIH.Client.Signature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Model
{
    /// <summary>
    /// Couple of X509 parameters that usually go togheter
    /// </summary>
    public class X509Configuration
    {
        /// <summary>
        /// Signature certificate
        /// </summary>
        public Org.BouncyCastle.X509.X509Certificate SignatureCertificate { get; set; }

        public Org.BouncyCastle.Crypto.AsymmetricKeyParameter SignaturePrivateKey { get; set; }

        public SignatureAlgorithm SignatureAlgorithm { get; set; }

        /// <summary>
        /// Should the request contain the full X509KeyInfo section in the signature
        /// </summary>
        public bool IncludeKeyInfo { get; set; }

        /// <summary>
        /// Enc certificate
        /// </summary>
        public Org.BouncyCastle.X509.X509Certificate EncryptionCertificate { get; set; }

        public Org.BouncyCastle.Crypto.AsymmetricKeyParameter EncryptionPrivateKey { get; set; }
    }
}
