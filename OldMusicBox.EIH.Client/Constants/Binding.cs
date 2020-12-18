using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Constants
{
    /// <summary>
    /// Binding constants
    /// </summary>
    public class Binding
    {
        /// <summary>
        /// Artifact binding
        /// </summary>
        public const string ARTIFACT = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Artifact";
        /// <summary>
        /// REDIRECT binding
        /// </summary>
        public const string REDIRECT = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect";
        /// <summary>
        /// POST binding
        /// </summary>
        public const string POST     = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST";
    }
}
