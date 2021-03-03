using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            return new EmptyResult();
        }

        public ActionResult SingleLogoutService()
        {
            return new EmptyResult();
        }
    }
}