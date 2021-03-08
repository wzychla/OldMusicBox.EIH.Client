using OldMusicBox.EIH.Client;
using OldMusicBox.EIH.Client.Model;
using OldMusicBox.EIH.Client.Signature;
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
                throw new ApplicationException("No session associated for given artifact. This is a demo app and it stores sessions in memory. Restart your server then.");
            }

            // create encrypt and return the ArtifactResponse

            // this is optional
            var x509Configuration = new X509Configuration()
            {
                SignatureCertificate  = ServerCertificateProvider.GetSigCertificate(),
                SignaturePrivateKey   = ServerCertificateProvider.GetSigPrivateKey(),
                IncludeKeyInfo        = true,
                SignatureAlgorithm    = SignatureAlgorithm.ECDSA256,
                EncryptionCertificate = ServerCertificateProvider.GetEncCertificate(),
                EncryptionPrivateKey  = ServerCertificateProvider.GetEncPrivateKey()
            };

            var issuer               = ConfigurationManager.AppSettings["Issuer"];

            var responseFactory           = new ResponseFactory();
            var artifactResponseFactory   = new ArtifactResponseFactory();
            var encryptedAssertionFactory = new EncryptedAssertionFactory();

            responseFactory.X509Configuration = x509Configuration;
            responseFactory.InResponseTo      = artifactResolve.ID;
            responseFactory.Issuer            = issuer;

            artifactResponseFactory.X509Configuration = x509Configuration;
            artifactResponseFactory.InResponseTo      = artifactResolve.ID;
            artifactResponseFactory.Issuer            = issuer;

            responseFactory.EncryptedAssertions               = encryptedAssertionFactory.Build();
            artifactResponseFactory.ArtifactResponse.Response = responseFactory.Build();

            var artifactResponse        = artifactResponseFactory.Create();

            return Content(artifactResponse, "text/xml");
        }

        public ActionResult SingleLogoutService()
        {
            return new EmptyResult();
        }
    }
}