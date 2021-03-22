using OldMusicBox.EIH.Client;
using OldMusicBox.EIH.Client.Constants;
using OldMusicBox.EIH.Client.Model;
using OldMusicBox.EIH.Client.Model.Artifact;
using OldMusicBox.EIH.Client.Model.Request;
using OldMusicBox.EIH.Client.Signature;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace OldMusicBox.EIH.Demo.Controllers
{
    public class AccountController : Controller
    {
        /// <summary>
        /// Logon flow 
        /// </summary>
        public ActionResult Logon()
        {
            var saml2 = new Saml2AuthenticationModule();

            // parameters
            var assertionConsumerServiceURL = ConfigurationManager.AppSettings["AssertionConsumerServiceURL"];
            var assertionIssuer             = ConfigurationManager.AppSettings["AssertionIssuer"];
            var identityProvider            = ConfigurationManager.AppSettings["IdentityProvider"];
            var artifactResolve             = ConfigurationManager.AppSettings["ArtifactResolve"];

            var requestBinding  = Binding.POST;
            var responseBinding = Binding.ARTIFACT;

            // this is optional
            var x509Configuration = new X509Configuration()
            {
                SignatureCertificate  = ClientCertificateProvider.GetSigCertificate(),
                SignaturePrivateKey   = ClientCertificateProvider.GetSigPrivateKey(),
                IncludeKeyInfo        = true,
                SignatureAlgorithm    = SignatureAlgorithm.ECDSA256,
                EncryptionCertificate = ClientCertificateProvider.GetEncCertificate(),
                EncryptionPrivateKey  = ClientCertificateProvider.GetEncPrivateKey()
            };

            // check if this is 
            if (!saml2.IsSignInResponse(this.Request))
            {
                // AuthnRequest factory
                var authnRequestFactory = new AuthnRequestFactory();

                authnRequestFactory.AssertionConsumerServiceURL = assertionConsumerServiceURL;
                authnRequestFactory.AssertionIssuer             = assertionIssuer;
                authnRequestFactory.Destination                 = identityProvider;

                authnRequestFactory.X509Configuration = x509Configuration;

                authnRequestFactory.RequestBinding  = requestBinding;
                authnRequestFactory.ResponseBinding = responseBinding;

                authnRequestFactory.AuthnRequest.ForceAuthn = true;

                // specyficzne dla WK
                authnRequestFactory.AuthnRequest.Extensions = new Extensions()
                {
                    SPType = "public",
                    RequestedAttributes = new[]
                    {
                        new RequestedAttribute()
                        {
                            FriendlyName = Eidas.FamilyName,
                            Name         = Eidas.FamilyNameClaim,
                            NameFormat   = Client.Constants.NameID.URI,
                            IsRequired   = true
                        },
                        new RequestedAttribute()
                        {
                            FriendlyName = Eidas.FirstName,
                            Name         = Eidas.FirstNameClaim,
                            NameFormat   = Client.Constants.NameID.URI,
                            IsRequired   = true
                        },
                        new RequestedAttribute()
                        {
                            FriendlyName = Eidas.DateOfBirth,
                            Name         = Eidas.DateOfBirthClaim,
                            NameFormat   = Client.Constants.NameID.URI,
                            IsRequired   = true
                        },
                        new RequestedAttribute()
                        {
                            FriendlyName = Eidas.PersonIdentifier,
                            Name         = Eidas.PersonIdentifierClaim,
                            NameFormat   = Client.Constants.NameID.URI,
                            IsRequired   = true
                        }
                    }
                };
                authnRequestFactory.AuthnRequest.NameIDPolicy = new NameIDPolicy()
                {
                    AllowCreate = true,
                    Format = Client.Constants.NameID.UNSPECIFIED
                };
                authnRequestFactory.AuthnRequest.RequestedAuthnContext = new RequestAuthContext()
                {
                    Comparison = AuthnContextComparisonType.Minimum,
                    AuthnContextClassRef = Eidas.LOA_SUBSTANTIAL
                };

                return Content(authnRequestFactory.CreatePostBindingContent());
            }
            else
            {
                // the token is created from the IdP's response
                Client.Saml2SecurityToken securityToken = null;

                switch (responseBinding)
                {
                    case Binding.ARTIFACT:

                        var artifactConfig = new ArtifactResolveConfiguration()
                        {
                            ArtifactResolveUri = artifactResolve,
                            Issuer             = assertionIssuer,
                            X509Configuration  = x509Configuration
                        };

                        securityToken = saml2.GetArtifactSecurityToken(this.Request, artifactConfig);
                        break;
                    case Binding.POST:
                        securityToken = saml2.GetPostSecurityToken(this.Request);
                        break;
                    default:
                        throw new NotSupportedException(string.Format("The {0} response binding is not yet supported", responseBinding));
                }

                // fail if there is no token
                if (securityToken == null)
                {
                    throw new ArgumentNullException("No security token found in the response accoding to the Response Binding configuration");
                }

                // token will be decrypted (if necessary)
                saml2.TryDecryptingEncryptedAssertions(securityToken, x509Configuration);

                // the token will be validated
                var configuration = new SecurityTokenHandlerConfiguration
                {
                    CertificateValidator = X509CertificateValidator.None,
                    IssuerNameRegistry   = new DemoClientIssuerNameRegistry(),
                    DetectReplayedTokens = false
                };
                configuration.AudienceRestriction.AudienceMode = AudienceUriMode.Never;

                var tokenHandler = new Client.Saml2SecurityTokenHandler()
                {
                    Configuration = configuration
                };
                var identity = tokenHandler.ValidateToken(securityToken).FirstOrDefault();
                // WK nie zwraca loginu, zwraca tylko imię/nazwisko/pesel/data urodzenia
                if (identity.FindFirst(ClaimTypes.Name) == null)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Name, identity.FindFirst(ClaimTypes.NameIdentifier).Value));
                }

                // this is the SessionIndex, store it if necessary
                string sessionIndex = securityToken.Assertion.ID;

                // the token is validated succesfully
                var principal = new ClaimsPrincipal(identity);
                if (principal.Identity.IsAuthenticated)
                {
                    var formsTicket = new FormsAuthenticationTicket(
                        1, principal.Identity.Name, DateTime.UtcNow, DateTime.UtcNow.Add(FormsAuthentication.Timeout), false, sessionIndex);

                    this.Response.AppendCookie(new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(formsTicket)));

                    var redirectUrl = FormsAuthentication.GetRedirectUrl(principal.Identity.Name, false);

                    return Redirect(redirectUrl);
                }
                else
                {
                    throw new ArgumentNullException("principal", "Unauthenticated principal returned from token validation");
                }
            }
        }
    }
}