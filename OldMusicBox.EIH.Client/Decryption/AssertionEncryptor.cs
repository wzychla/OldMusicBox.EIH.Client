using OldMusicBox.EIH.Client.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Decryption
{
    /// <summary>
    /// Electronic Identification Hub (Węzeł Krajowy) assertion encryptor
    /// </summary>
    public class AssertionEncryptor
    {
        /// <summary>
        /// Public encrypt method
        /// </summary>
        public EncryptedAssertion Encrypt(
            ClaimsPrincipal principal,
            Org.BouncyCastle.X509.X509Certificate publicKey
            )
        {
            return new EncryptedAssertion();
        }
    }
}
