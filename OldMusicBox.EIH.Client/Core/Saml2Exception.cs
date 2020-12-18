using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Core
{
    public class Saml2Exception : Exception
    {
        public Saml2Exception() { }

        public Saml2Exception(string message) : base(message) { }

        public Saml2Exception(string message, Exception innerException) : base(message, innerException) { }
    }
}
