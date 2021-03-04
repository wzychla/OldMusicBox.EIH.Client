using OldMusicBox.EIH.Client;
using OldMusicBox.EIH.Client.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace OldMusicBox.EIH.ServerDemo.Controllers
{
    /// <summary>
    /// Auxiliary SAML2 endpoint
    /// ..../login-service/idpArtifactResolutionService - Artifact Resolve endpoint
    /// ..../login-service/singleLogoutService          - SLO endpoint
    /// </summary>
    public class LoginServicesController : Controller
    {
        public ActionResult IdpArtifactResolutionService()
        {
            // pick up the ArtifactResolve from the request
            var artifactResolve = new ArtifactResolveFactory().From(this.Request);
            if ( artifactResolve == null )
            {
                throw new ArgumentNullException();
            }

            // validate
            var saml2             = new Saml2AuthenticationModule();
            var validateionResult = saml2.MessageSigner.Validate(artifactResolve, out Org.BouncyCastle.X509.X509Certificate certificate);

            // is not only that document should be valid but also its issuer and the certificate should match!
            if (!validateionResult)
            {
                throw new ApplicationException("Invalid signature");
            }

            // check if a matching session exists
            var sessionIndex     = artifactResolve.Artifact;
            var sessionPrincipal = new SessionArtifactRepository().QuerySessionPrincipal(sessionIndex);

            if ( sessionPrincipal == null )
            {
                throw new ApplicationException("No session associated for given artifact");
            }

            // create encrypt and return the ArtifactResponse
            var issuer               = ConfigurationManager.AppSettings["Issuer"];
            var artifactResponse     = new ArtifactResponseFactory().From(artifactResolve, sessionPrincipal, issuer);

            var artifactSeralized    = saml2.MessageSerializer.Serialize(artifactResponse, new Client.Serialization.MessageSerializationParameters());
            var artifactResponseSoap = artifactSeralized.AsEnveloped();

            return Content(artifactResponseSoap, "text/xml");
        }

        public ActionResult SingleLogoutService()
        {
            return new EmptyResult();
        }
    }
}