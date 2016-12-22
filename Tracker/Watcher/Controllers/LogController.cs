using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Watcher.Controllers
{
    [RoutePrefix("log")]
    public class LogController : Controller
    {
        // GET: Log
        public ActionResult Index()
        {
            return View();
        }

        [Route("ex")]
        public ActionResult ExceptionLog()
        {
            return View();
        }
    }
}