using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OldMusicBox.EIH.ServerDemo
{
    /// <summary>
    /// Handle hypens in route segments
    /// </summary>
    /// <remarks>
    /// https://stackoverflow.com/questions/26176087/how-to-make-the-controllers-name-hyphen-separated
    /// </remarks>
    public class HyphenatedRouteHandler : MvcRouteHandler
    {
        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            requestContext.RouteData.Values["controller"] = requestContext.RouteData.Values["controller"].ToString().Replace("-", "");
            requestContext.RouteData.Values["action"]     = requestContext.RouteData.Values["action"].ToString().Replace("-", "");
            return base.GetHttpHandler(requestContext);
        }
    }
}