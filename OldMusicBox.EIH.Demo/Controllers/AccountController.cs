﻿using OldMusicBox.EIH.Client;
using OldMusicBox.EIH.Client.Constants;
using OldMusicBox.EIH.Client.Model;
using OldMusicBox.EIH.Client.Model.Artifact;
using OldMusicBox.EIH.Client.Model.Request;
using OldMusicBox.EIH.Client.Signature;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.IdentityModel.Services;
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
            var providerName                = ConfigurationManager.AppSettings["ProviderName"];

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
                authnRequestFactory.ProviderName                = providerName;
                authnRequestFactory.Destination                 = identityProvider;

                authnRequestFactory.X509Configuration = x509Configuration;

                authnRequestFactory.RequestBinding  = requestBinding;
                authnRequestFactory.ResponseBinding = responseBinding;

                authnRequestFactory.AuthnRequest.ForceAuthn = true;

                authnRequestFactory.RelayState = "foobar"; // RelayState is supported!

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
                    // WK uses ARTIFACT binding
                    //case Binding.POST:
                    //    securityToken = saml2.GetPostSecurityToken(this.Request);
                    //    break;
                    default:
                        throw new NotSupportedException(string.Format("The {0} response binding is not yet supported", responseBinding));
                }

                // not used in the demo but 
                // can be used for the return url
                var relayState = RetrieveRelayState(this.Request);

                // fail if there is no token
                if (securityToken == null || 
                    securityToken.Response == null ||
                    securityToken.Response.Status == null ||
                    securityToken.Response.Status.StatusCode == null
                    )
                {
                    throw new ArgumentNullException("No security token found in the response accoding to the Response Binding configuration");
                }

                if (securityToken.Response.Status.StatusCode.Value == StatusCodes.SUCCESS)
                {
                    // token will be decrypted (if necessary)
                    saml2.TryDecryptingEncryptedAssertions(securityToken, x509Configuration);

                    // the token will be validated
                    var configuration = new SecurityTokenHandlerConfiguration
                    {
                        CertificateValidator = X509CertificateValidator.None,
                        IssuerNameRegistry = new DemoClientIssuerNameRegistry(),
                        DetectReplayedTokens = false
                    };
                    configuration.AudienceRestriction.AudienceMode = AudienceUriMode.Never;

                    var tokenHandler = new Client.Saml2SecurityTokenHandler()
                    {
                        Configuration = configuration
                    };
                    try
                    {
                        var identity = tokenHandler.ValidateToken(securityToken).FirstOrDefault();

                        // WK nie zwraca loginu, zwraca tylko imię/nazwisko/pesel/data urodzenia
                        if (identity.FindFirst(ClaimTypes.Name) == null)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Name, identity.FindFirst(ClaimTypes.NameIdentifier).Value));
                        }

                        // this is the SessionIndex, store it if necessary
                        string sessionIndex = securityToken.Assertion.AuthnStatement.SessionIndex;
                        identity.AddClaim(new Claim(Saml2ClaimTypes.SessionIndex, sessionIndex));

                        // the token is validated succesfully
                        var principal = new ClaimsPrincipal(identity);
                        if (principal.Identity.IsAuthenticated)
                        {
                            SessionAuthenticationModule sam = FederatedAuthentication.SessionAuthenticationModule;
                            var token =
                                sam.CreateSessionSecurityToken(principal, string.Empty,
                                     DateTime.Now.ToUniversalTime(), DateTime.Now.AddMinutes(60).ToUniversalTime(), false);

                            sam.WriteSessionTokenToCookie(token);

                            var redirectUrl = FormsAuthentication.GetRedirectUrl(principal.Identity.Name, false);

                            return Redirect(redirectUrl);
                        }
                        else
                        {
                            throw new ArgumentNullException("principal", "Unauthenticated principal returned from token validation");
                        }
                    }
                    catch (Exception ex)
                    {
                        return Content("Problem z uwierzytelnieniem " + ex.Message);
                    }
                }
                else
                {
                    return Content("Uwierzytelnienie zablokowane przez serwer. Status: " + securityToken.Response.Status.StatusCode.Value);
                }
            }
        }

        private string RetrieveRelayState( HttpRequestBase request )
        {
            var relayState = request.QueryString[Elements.RELAYSTATE];
            if ( string.IsNullOrEmpty( relayState ) )
            {
                relayState = request.Form[Elements.RELAYSTATE];
            }

            return relayState;
        }
    }
}