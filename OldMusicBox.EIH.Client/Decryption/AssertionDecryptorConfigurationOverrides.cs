using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Decryption
{
    /// <summary>
    /// Decryptor configuration overrides.
    /// Use with caution!
    /// </summary>
    public class AssertionDecryptorConfigurationOverrides
    {
        /// <summary>
        /// Overrides AES defaults
        /// </summary>
        public static int? KeyBitSize { get; set; }
        /// <summary>
        /// Overrides AES defaults
        /// </summary>
        public static int? MacBitSize { get; set; }
        /// <summary>
        /// Overrides AES defaults
        /// </summary>
        public static int? NonceBitSize { get; set; }
    }
}
