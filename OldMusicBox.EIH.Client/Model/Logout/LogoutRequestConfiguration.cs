using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Model.Logout
{
    /// <summary>
    /// Logout configuration for active profile
    /// </summary>
    public class LogoutRequestConfiguration
    {
        /// <summary>
        /// Where to go with the logout request
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// Request issuer
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// UserName
        /// </summary>
        public NameID NameID { get; set; }

        /// <summary>
        /// Session ID
        /// </summary>
        public string SessionIndex { get; set; }
        /// <summary>
        /// How to sign the request
        /// </summary>
        public X509Configuration X509Configuration { get; set; }
    }
}
