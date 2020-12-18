using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Constants
{
    /// <summary>
    /// Response status code
    /// </summary>
    public class StatusCodes
    {
        /// <summary>
        /// Something is wrong with the request side
        /// </summary>
        public const string REQUESTER = "urn:oasis:names:tc:SAML:2.0:status:Requester";

        /// <summary>
        /// Something is wrong at the provider side
        /// </summary>
        public const string RESPONDER = "urn:oasis:names:tc:SAML:2.0:status:Responder";

        /// <summary>
        /// All ok
        /// </summary>
        public const string SUCCESS = "urn:oasis:names:tc:SAML:2.0:status:Success";
    }
}
