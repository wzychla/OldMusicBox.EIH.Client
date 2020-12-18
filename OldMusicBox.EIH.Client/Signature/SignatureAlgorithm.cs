using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Signature
{
    /// <summary>
    /// Signature Algorithm
    /// </summary>
    /// <remarks>
    /// The ECDSA signatures are NOT YET supported
    /// because the SignedXml doesn't support it
    /// </remarks>
    public enum SignatureAlgorithm
    {
        SHA1,
        SHA256,
        ECDSA256
    }
}
