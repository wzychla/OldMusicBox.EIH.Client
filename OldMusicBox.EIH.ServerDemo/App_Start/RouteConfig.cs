using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OldMusicBox.EIH.ServerDemo
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.Add(
                    new Route("{controller}/{action}/{id}",
                        new RouteValueDictionary(
                            new { controller = "Home", action = "Index", id = UrlParameter.Optional }),
                            new HyphenatedRouteHandler())
                    );
        }
    }
}
