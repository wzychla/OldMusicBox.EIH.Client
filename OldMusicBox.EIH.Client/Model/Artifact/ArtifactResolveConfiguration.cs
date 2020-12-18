using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Model.Artifact
{
    /// <summary>
    /// Artifact resolve configuration
    /// </summary>
    public class ArtifactResolveConfiguration
    {
        /// <summary>
        /// Where to go with the artifact
        /// </summary>
        public string ArtifactResolveUri { get; set; }
        /// <summary>
        /// Request issuer
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// How to sign the request
        /// </summary>
        public X509Configuration X509Configuration { get; set; }
    }
}
