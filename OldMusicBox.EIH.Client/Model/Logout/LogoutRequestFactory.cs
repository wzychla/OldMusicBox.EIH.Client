using OldMusicBox.EIH.Client.Constants;
using OldMusicBox.EIH.Client.Logging;
using OldMusicBox.EIH.Client.Resources;
using OldMusicBox.EIH.Client.Serialization;
using OldMusicBox.EIH.Client.Signature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OldMusicBox.EIH.Client.Model.Logout
{
    /// <summary>
    /// Logout request factory
    /// </summary>
    public class LogoutRequestFactory : BaseFactory
    {
        public LogoutRequestFactory()
        {
            this.LogoutRequest = new LogoutRequest();

            this.LogoutRequest.ID           = string.Format("id_{0}", Guid.NewGuid());
            this.LogoutRequest.IssueInstant = DateTime.UtcNow;
            this.LogoutRequest.Version      = ProtocolVersion._20;
        }

        /// <summary>
        /// Create LogoutRequest when sent by the IdP
        /// </summary>
        public LogoutRequest From(HttpRequestBase request)
        {
            if (request == null) throw new ArgumentNullException();

            var rawMessage = new RawMessageFactory().FromIdpRequest(request);
            if (rawMessage == null) return null;

            // log
            new LoggerFactory().For(this).Debug(Event.LogoutRequest, rawMessage.Payload);

            var logoutRequest        = this.MessageSerializer.Deserialize<LogoutRequest>(rawMessage.Payload, new MessageDeserializationParameters());
            logoutRequest.RawMessage = rawMessage;
            return logoutRequest;
        }

        public LogoutRequest LogoutRequest { get; private set; }

        /// <summary>
        /// Logout request issuer
        /// </summary>
        public string Issuer
        {
            get
            {
                return this.LogoutRequest.Issuer;
            }
            set
            {
                this.LogoutRequest.Issuer = value;
            }
        }

        /// <summary>
        /// The Identity Provider
        /// </summary>
        public string Destination
        {
            get
            {
                return this.LogoutRequest.Destination;
            }
            set
            {
                this.LogoutRequest.Destination = value;
            }
        }

        /// <summary>
        /// Session Index
        /// </summary>
        public string SessionIndex
        {
            get
            {
                return this.LogoutRequest.SessionIndex;
            }
            set
            {
                this.LogoutRequest.SessionIndex = value;
            }
        }

        /// <summary>
        /// Name ID
        /// </summary>
        public NameID NameID
        {
            get
            {
                return this.LogoutRequest.NameID;
            }
            set
            {
                this.LogoutRequest.NameID = value;
            }
        }

        /// <summary>
        /// Request binding
        /// </summary>
        public string RequestBinding { get; set; }

        /// <summary>
        /// Certificate used to create a signature
        /// </summary>
        public X509Configuration X509Configuration { get; set; }

        #region Post binding

        /// <summary>
        /// Post binding should return the AuthnRequest in a web page that posts to the identity provider
        /// </summary>
        public virtual string CreatePostBindingContent()
        {
            if (string.IsNullOrEmpty(this.RequestBinding))
            {
                throw new ArgumentNullException("RequestBinding", "Request Binding cannot be null");
            }
            if (string.IsNullOrEmpty(this.Destination))
            {
                throw new ArgumentNullException("Destination", "Destination cannot be null");
            }

            string contentPage = new ResourceFactory().Create(ResourceFactory.EmbeddedResource.RequestPostBinding);

            contentPage = contentPage.Replace("((Destination))", this.Destination);
            contentPage = contentPage.Replace("((SAMLRequest))", this.CreatePostBindingToken());
            contentPage = contentPage.Replace("((RelayState))",  string.Empty);

            // log
            new LoggerFactory().For(this).Debug(Event.LogoutRequest, contentPage);

            return contentPage;
        }

        /// <summary>
        /// Serialize, encode, sign, whatever necessary to create
        /// a POST binding token that is POSTed to the IdP
        /// </summary>
        /// <returns></returns>
        protected virtual string CreatePostBindingToken()
        {
            // sign the request?
            if (this.X509Configuration != null &&
                this.X509Configuration.SignatureCertificate != null
                )
            {
                var signedLogoutRequest = this.MessageSigner.Sign(this.LogoutRequest, this.X509Configuration);
                return Convert.ToBase64String(signedLogoutRequest);
            }

            throw new ArgumentException("LogoutRequest must be signed. The factory needs a non-empty X509 configuration.");
        }

        #endregion
    }
}
