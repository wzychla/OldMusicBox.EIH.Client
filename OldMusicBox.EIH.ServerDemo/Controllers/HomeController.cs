using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OldMusicBox.EIH.ServerDemo.Controllers
{
    /// <summary>
    /// Not used in usual SSO flows
    /// </summary>
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Content("Nothing to do here, only SAML2 endpoints are supported");
            // return View();
        }
    }
}