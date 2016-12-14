using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Watcher.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Login/
        public ActionResult Index()
        {
            return View("Login");
        }

        public ActionResult Register()
        {
            var claims = new List<Claim>();
            //claims.Add(new Claim(ClaimTypes.Name,"Paul"));
            claims.Add(new Claim("WatcherUser", "Paul"));
            claims.Add(new Claim("Email", "good_hy@163.com"));
            var id = new ClaimsIdentity(claims,DefaultAuthenticationTypes.ApplicationCookie);

            var ctx = Request.GetOwinContext();
            var authenticationManager = ctx.Authentication;
            authenticationManager.SignIn(id);

            return RedirectToAction("Index", "Home");
        }
	}
}