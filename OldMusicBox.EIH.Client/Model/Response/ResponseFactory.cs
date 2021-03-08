using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Model
{
    /// <summary>
    /// Response factory
    /// </summary>
    public class ResponseFactory : BaseFactory
    {
        public ResponseFactory()
        {
            this.Response = new Response();

            this.Response.ID           = "gr_" + Guid.NewGuid();
            this.Response.Version      = "2.0";
            this.Response.IssueInstant = DateTime.UtcNow;

            this.Response.Status       = Status.Success;
        }

        public string InResponseTo
        {
            get
            {
                return this.Response.InResponseTo;
            }
            set
            {
                this.Response.InResponseTo = value;
            }
        }

        public string Issuer
        {
            get
            {
                return this.Response.Issuer;
            }
            set
            {
                this.Response.Issuer = value;
            }
        }

        public Assertion[] UnencryptedAssertions
        {
            get
            {
                return this.Response.UnencryptedAssertions;
            }
            set
            {
                this.Response.UnencryptedAssertions = value;
            }
        }

        public EncryptedAssertion[] EncryptedAssertions
        {
            get
            {
                return this.Response.EncryptedAssertion;
            }
            set
            {
                this.Response.EncryptedAssertion = value;
            }
        }

        public X509Configuration X509Configuration { get; set; }

        public Response Response { get; set; }

        public virtual string Build()
        {
            // sign the request?
            if (this.X509Configuration != null &&
                this.X509Configuration.SignatureCertificate != null
                )
            {
                var signedResponse = this.MessageSigner.Sign(this.Response, this.X509Configuration);
                return this.Encoding.GetString(signedResponse);
            }

            throw new ArgumentException("LogoutResponse must be signed. The factory needs a non-empty X509 configuration.");
        }
    }
}
