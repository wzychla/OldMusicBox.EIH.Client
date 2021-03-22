using OldMusicBox.EIH.Client.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Model
{
    /// <summary>
    /// Encrypted Assertion Factory
    /// </summary>
    public class EncryptedAssertionFactory
    {
        /// <summary>
        /// This will go into the encrypted Assertion
        /// </summary>
        public ClaimsPrincipal Principal { get; set; }

        /// <summary>
        /// This will go into PartyUInfo
        /// </summary>
        public string IssuerDomain { get; set; }

        /// <summary>
        /// This will go into PartyVInfo
        /// </summary>
        public string ConsumerDomain { get; set; }

        public Org.BouncyCastle.X509.X509Certificate EncryptionKey { get; set; }

        /// <summary>
        /// Create encrypted assertion
        /// </summary>
        public EncryptedAssertion[] Build()
        {
            var encryptor = new AssertionEncryptor();
            var assertion = encryptor.Encrypt(
                this.Principal,
                this.IssuerDomain,
                this.ConsumerDomain,
                this.EncryptionKey );

            return new[] 
            { 
                assertion
            };
        }
    }
}
