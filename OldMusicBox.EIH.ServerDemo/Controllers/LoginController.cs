using OldMusicBox.EIH.Client;
using OldMusicBox.EIH.Client.Constants;
using OldMusicBox.EIH.Client.Model;
using OldMusicBox.EIH.Client.Model.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace OldMusicBox.EIH.ServerDemo.Controllers
{
    public class LoginController : Controller
    {
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        /// <summary>
        /// Primary SAML2 endpoint
        /// ...../login/SingleSignOnService - SSO endpoint
        /// POST Binding is supported when client redirects here
        /// GET  Binding is supported when login page redirects here
        /// </summary>
        public ActionResult SingleSignOnService()
        {
            try
            {
                // authentication request is passed or saved by an eariler login flow
                var authReq = this.Session[Elements.AUTHNREQUEST] as AuthnRequest;
                if (authReq == null)
                {
                    authReq = new AuthnRequestFactory().FromRequest(this.Request);
                }

                if (authReq != null)
                {
                    return HandleAuthenticationRequest(authReq);
                }
                else
                {
                    return Content("AuthnRequest missing from the request");
                }
            }
            catch ( Exception ex )
            {
                return Content(ex.Message);
            }
        }

        private ActionResult HandleAuthenticationRequest( AuthnRequest req )
        {
            if ( this.User.Identity != null && this.User.Identity.IsAuthenticated )
            {
                // retrieve session index from principal
                var sessionIndex = new PrincipalManager().GetSessionIndexFromPrincipal(this.User);
                if ( string.IsNullOrEmpty( sessionIndex ) )
                {
                    throw new ArgumentNullException("SessionIndex missing from ClaimsPrincipal");
                }
                // validate 
                var saml2             = new Saml2AuthenticationModule();
                var validateionResult = saml2.MessageSigner.Validate(req, out Org.BouncyCastle.X509.X509Certificate certificate);

                // is not only that document should be valid but also its issuer and the certificate should match!
                if ( !validateionResult )
                {
                    throw new ApplicationException("Invalid signature");
                }

                // authenticated user, return the SAML Artifact 
                var url = $"{req.AssertionConsumerServiceURL}?SAMLArt={sessionIndex}";
                return Redirect(url);
            }
            else
            {
                // anonymous user, handle authentication first
                // the reason one can't just put [Authorize] over the SSO endpoint is that
                // [Authorize] causes the response being redirected to the login page using 302 redirect
                // but this doesn't preserve the AuthnRequest that is possibly POSTed here
                this.Session.Add(Elements.AUTHNREQUEST, req);

                string loginUrl = $"{FormsAuthentication.LoginUrl}?ReturnUrl={Request.Path}";
                return Redirect(loginUrl);
            }
        }
    }
}