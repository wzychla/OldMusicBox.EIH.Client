using OldMusicBox.EIH.Client.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Signature
{
    /// <summary>
    /// Message signer
    /// </summary>
    public interface IMessageSigner
    {
        /// <summary>
        /// Sign
        /// </summary>
        byte[] Sign(
            ISignableMessage   message,
            X509Configuration  x509Configuration
            );

        /// <summary>
        /// Validate against given certificate
        /// </summary>
        bool Validate(
            IVerifiableMessage message,
            Org.BouncyCastle.X509.X509Certificate certificate
            );

        /// <summary>
        /// Validate against certificate found in the XML
        /// </summary>
        bool Validate(
            IVerifiableMessage message,
            out Org.BouncyCastle.X509.X509Certificate certificate
            );
    }
}
