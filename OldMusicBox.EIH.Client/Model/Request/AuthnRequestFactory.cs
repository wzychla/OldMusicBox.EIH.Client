using OldMusicBox.EIH.Client.Constants;
using OldMusicBox.EIH.Client.Logging;
using OldMusicBox.EIH.Client.Resources;
using OldMusicBox.EIH.Client.Serialization;
using OldMusicBox.EIH.Client.Signature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OldMusicBox.EIH.Client.Model.Request
{
    /// <summary>
    /// SAML2 authentication request factory
    /// </summary>
    public class AuthnRequestFactory : BaseFactory
    {
        public AuthnRequestFactory() 
        {
            this.AuthnRequest           = new AuthnRequest();

            this.AuthnRequest.ID           = string.Format("id_{0}", Guid.NewGuid());
            this.AuthnRequest.IssueInstant = DateTime.UtcNow;
            this.AuthnRequest.Version      = ProtocolVersion._20;
        }

        public AuthnRequest AuthnRequest { get; private set; }

        /// <summary>
        /// Assertion Consumer Service URL        
        /// </summary>
        /// <remarks>
        /// In Saml2 language this is the address the Identity Provider
        /// should redirect the response to
        /// </remarks>
        public string AssertionConsumerServiceURL
        {
            get
            {
                return this.AuthnRequest.AssertionConsumerServiceURL;
            }
            set
            {
                this.AuthnRequest.AssertionConsumerServiceURL = value;
            }
        }

        /// <summary>
        /// Request issuer's name recognized by the IdentityProvider
        /// </summary>
        /// <remarks>
        /// Usually it's something different than the AssertionConsumerServiceURL
        /// </remarks>
        public string AssertionIssuer
        {
            get
            {
                return this.AuthnRequest.Issuer;
            }
            set
            {
                this.AuthnRequest.Issuer = value;
            }
        }

        /// <summary>
        /// The Identity Provider
        /// </summary>
        public string Destination
        {
            get
            {
                return this.AuthnRequest.Destination;
            }
            set
            {
                this.AuthnRequest.Destination = value;
            }
        }

        /// <summary>
        /// Request binding
        /// </summary>
        public string RequestBinding { get; set; }

        /// <summary>
        /// Relay state
        /// </summary>
        public string RelayState { get; set; }

        /// <summary>
        /// Response binding 
        /// </summary>
        public string ResponseBinding
        {
            get
            {
                return this.AuthnRequest.ProtocolBinding;
            }
            set
            {
                this.AuthnRequest.ProtocolBinding = value;
            }
        }

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
            if (string.IsNullOrEmpty(this.ResponseBinding))
            {
                throw new ArgumentNullException("ResponseBinding", "Response Binding cannot be null");
            }
            if (string.IsNullOrEmpty(this.Destination))
            {
                throw new ArgumentNullException("Destination", "Response Binding cannot be null");
            }

            string contentPage = new ResourceFactory().Create(ResourceFactory.EmbeddedResource.RequestPostBinding);

            contentPage = contentPage.Replace("((Destination))", this.Destination);
            contentPage = contentPage.Replace("((SAMLRequest))", this.CreatePostBindingToken());
            contentPage = contentPage.Replace("((RelayState))", this.RelayState);

            // log
            new LoggerFactory().For(this).Debug(Event.PostBindingPage, contentPage);

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
            if ( this.X509Configuration != null &&
                 this.X509Configuration.SignatureCertificate != null
                )
            {
                var signedAuthnRequest = this.MessageSigner.Sign(this.AuthnRequest, this.X509Configuration);
                return Convert.ToBase64String(signedAuthnRequest);
            }
            else
            {
                return this.MessageSerializer.Serialize(
                    this.AuthnRequest,
                    new MessageSerializationParameters()
                    {
                        ShouldBase64Encode = true,
                        ShouldDeflate = false
                    });
            }
        }

        #endregion

        #region Redirect binding

        /// <summary>
        /// Redirect binding should return a redirect uri that conforms to Saml2 specs
        /// </summary>
        public virtual string CreateRedirectBindingContent()
        {
            if (string.IsNullOrEmpty(this.RequestBinding))
            {
                throw new ArgumentNullException("RequestBinding", "Request Binding cannot be null");
            }
            if (string.IsNullOrEmpty(this.ResponseBinding))
            {
                throw new ArgumentNullException("ResponseBinding", "Response Binding cannot be null");
            }
            if (string.IsNullOrEmpty(this.Destination))
            {
                throw new ArgumentNullException("Destination", "Response Binding cannot be null");
            }

            var uri         = new UriBuilder(this.Destination);
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // request        
            var samlRequest =
                this.MessageSerializer.Serialize(
                    this.AuthnRequest,
                    new MessageSerializationParameters()
                    {
                        ShouldBase64Encode = true,
                        ShouldDeflate      = true
                    });
            queryString.Add(Elements.SAMLREQUEST, samlRequest);
            // relay state?
            if ( !string.IsNullOrEmpty( this.RelayState ) )
            {
                queryString.Add("RelayState", this.RelayState);
            }
            uri.Query = queryString.ToString();

            // return the uri
            return uri.ToString();
        }

        #endregion

        #region From request

        public AuthnRequest FromRequest(HttpRequestBase request)
        {
            if (request[Elements.SAMLREQUEST] != null)
            {
                return 
                    this.MessageSerializer.Deserialize<AuthnRequest>(
                        request[Elements.SAMLREQUEST],
                        new MessageDeserializationParameters()
                        {
                            ShouldDebase64Encode = true,
                            // inflated in uri, raw in form
                            ShouldInflate = request.Form[Elements.SAMLREQUEST] == null
                        });
            }
            return null;
        }

        #endregion
    }
}
