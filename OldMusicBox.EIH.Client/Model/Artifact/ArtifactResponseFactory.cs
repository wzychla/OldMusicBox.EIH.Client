using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Model
{
    /// <summary>
    /// ArtifactResponse factory
    /// </summary>
    public class ArtifactResponseFactory : BaseFactory
    {
        public ArtifactResponseFactory()
        {
            this.ArtifactResponse = new ArtifactResponse();

            this.ArtifactResponse.ID           = "_" + Guid.NewGuid();
            this.ArtifactResponse.Version      = "2.0";
            this.ArtifactResponse.IssueInstant = DateTime.UtcNow;

            this.ArtifactResponse.Status       = Status.Success;
        }

        public string Issuer
        {
            get
            {
                return this.ArtifactResponse.Issuer;
            }
            set
            {
                this.ArtifactResponse.Issuer = value;
            }
        }

        public string InResponseTo
        {
            get
            {
                return this.ArtifactResponse.InResponseTo;
            }
            set
            {
                this.ArtifactResponse.InResponseTo = value;
            }
        }

        public ArtifactResponse ArtifactResponse { get; set; }

        public X509Configuration X509Configuration { get; set; }

        public virtual string Create()
        {
            var signedArtifact = this.MessageSigner.Sign(this.ArtifactResponse, this.X509Configuration);
            return signedArtifact.AsEnveloped(this.Encoding);
        }
    }
}
