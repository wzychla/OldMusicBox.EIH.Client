using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Org.BouncyCastle.Crypto.Xml
{
    /// <summary>
    /// HMac key
    /// </summary>
    public class HMacKeyParameter : AsymmetricKeyParameter
    {
        public byte[] Key { get; set; }

        public HMacKeyParameter(byte[] key, bool privateKey) : base(privateKey)
        {
            this.Key = key;
        }
    }
}
