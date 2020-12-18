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
        byte[] Sign(
            ISignableMessage   message,
            X509Configuration  x509Configuration
            );

        bool Validate(
            IVerifiableMessage message,
            Org.BouncyCastle.X509.X509Certificate certificate
            );
    }
}
