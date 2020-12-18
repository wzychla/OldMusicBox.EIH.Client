using OldMusicBox.EIH.Client;
using OldMusicBox.EIH.Client.Model;
using OldMusicBox.EIH.Client.Model.Logout;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace OldMusicBox.EIH.Demo.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Logout that is triggered by this application.
        /// Please refer to the explanation provided in the comments
        /// to the Account/Logout method
        /// </summary>
        [Authorize]
        public ActionResult Logout()
        {
            var saml2 = new Saml2AuthenticationModule();

            var assertionConsumerServiceURL = ConfigurationManager.AppSettings["AssertionConsumerServiceURL"];
            var assertionIssuer             = ConfigurationManager.AppSettings["AssertionIssuer"];
            var logoutService               = ConfigurationManager.AppSettings["SingleLogout"];

            var x509Configuration = new X509Configuration()
            {
                SignatureCertificate = ClientCertificateProvider.GetSigCertificate(),
                SignaturePrivateKey  = ClientCertificateProvider.GetSigPrivateKey(),
                IncludeKeyInfo       = true,
                SignatureAlgorithm   = Client.Signature.SignatureAlgorithm.ECDSA256
            };

            // the identity provider possibly needs the SessionIndex, too
            // note that the SessionIndex is obtained in the Account/Logon
            // and stored for the current session
            var cookieValue = this.Request.Cookies[FormsAuthentication.FormsCookieName].Value;
            var ticket      = FormsAuthentication.Decrypt(cookieValue);

            // LogoutRequest factory
            var logoutRequestCofiguration = new LogoutRequestConfiguration()
            {
                Destination = logoutService,
                Issuer = assertionIssuer,
                NameID = new Client.Model.NameID()
                {
                    Text   = this.User.Identity.Name,
                    Format = Client.Constants.NameID.UNSPECIFIED,
                },
                SessionIndex = ticket.UserData,
                X509Configuration = x509Configuration
            };

            var logoutResponse = saml2.GetLogoutResponse(this.Request, logoutRequestCofiguration);

            FormsAuthentication.SignOut();

            return Redirect("/Home/Index");
        }
    }
}