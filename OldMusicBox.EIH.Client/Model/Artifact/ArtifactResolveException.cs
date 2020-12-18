using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Model.Artifact
{
    /// <summary>
    /// Exception
    /// </summary>
    public class ArtifactResolveException : Exception
    {
        public ArtifactResolveException() : base() { }

        public ArtifactResolveException(string message) : base(message) { }

        public ArtifactResolveException(string message, Exception inner) : base(message, inner) { }
    }
}
