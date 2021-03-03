using OldMusicBox.EIH.ServerDemo.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace OldMusicBox.EIH.ServerDemo.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public ActionResult Logon(string ReturnUrl)
        {
            var model       = new AccountLogonModel();
            
            model.Username  = "username";
            model.GivenName = "Kowalski";
            model.Surname   = "Jan";
            model.PESEL     = "11111111111";

            return View(model);
        }

        [HttpPost]
        public ActionResult Logon(AccountLogonModel model, string ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                // create a fresh guid for a session index
                string sessionIndex     = "s" + Guid.NewGuid().ToString();
                // save principal in a cookie
                var principal = new PrincipalManager().CreateSessionPrincipal(model.Username, model.PESEL, model.GivenName, model.Surname, sessionIndex);
                // also save it so that when the artifact resolution call arrives, the session is there
                new SessionArtifactRepository().StoreSessionPrincipal(sessionIndex, principal);

                // return
                //var returnUrl = Request.QueryString["ReturnUrl"];
                if (!string.IsNullOrEmpty( ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }
                else
                {
                    return Redirect("/");
                }
            }
            else
            {
                return View(model);
            }
        }
    }
}