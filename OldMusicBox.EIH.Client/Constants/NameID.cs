using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Constants
{
    /// <summary>
    /// NameID values
    /// </summary>
    public class NameID
    {
        public const string UNSPECIFIED = "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified";
        public const string PROVIDER    = "urn:oasis:names:tc:SAML:2.0:nameid-format:provider";
        public const string FEDERATED   = "urn:oasis:names:tc:SAML:2.0:nameid-format:federated";
        public const string TRANSIENT   = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient";
        public const string PERSISTENT  = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent";

        public const string URI = "urn:oasis:names:tc:SAML:2.0:attrname-format:uri";
    }
}
