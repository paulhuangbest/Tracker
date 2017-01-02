using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Watcher.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.RouteData.Values.ContainsKey("projectkey"))
                ViewBag.ProjectKey = filterContext.RouteData.Values["projectkey"];
            else
                ViewBag.ProjectKey = "";

            base.OnActionExecuting(filterContext);
        }
        
    }
}