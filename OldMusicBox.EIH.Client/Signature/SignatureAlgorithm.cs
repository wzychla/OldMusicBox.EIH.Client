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
    public enum SignatureAlgorithm
    {
        RSA1,
        RSA256,
        DSA256,
        ECDSA256,
        HMAC1,
        HMAC256
    }
}
