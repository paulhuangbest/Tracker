using Entity;
using Library.BL;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Watcher.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            IOwinContext ctx = Request.GetOwinContext();
            ClaimsPrincipal user = ctx.Authentication.User;
            IEnumerable<Claim> claims = user.Claims;
            //获取Cliams的方式
            //1、user.FindFirst("WatcherUser").Value
            //2、use Linq to claims

            ProfileBL bl = new ProfileBL();

            List<CoreProfile> profiles = bl.GetProfileList();


            return View(profiles);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Report()
        {
            WatcherBL bl = new WatcherBL();

            List<TotalDTO> exceptionTotal = bl.GetExceptionTotalByDay(DateTime.Now);

            List<TotalDTO> systemTotal = bl.GetSystemTotalByDay(DateTime.Now);

            List<TotalDTO> operateTotal = bl.GetOperateTotalByDay(DateTime.Now);

            List<TotalDTO> normalTotal = bl.GetNormalTotalByDay(DateTime.Now);

            List<TotalDTO> typeTotal = bl.GetTypeTotal(DateTime.Now);

            List<TotalDTO> typeTotalMonth = bl.GetTypeTotalMonth(DateTime.Now);

            ViewBag.ExceptionTotal = exceptionTotal;

            ViewBag.SystemTotal = systemTotal;

            ViewBag.OperateTotal = operateTotal;

            ViewBag.NormalTotal = normalTotal;

            ViewBag.TypeTotal = typeTotal;

            ViewBag.TypeTotalMonth = typeTotalMonth;

            return View(exceptionTotal);
        }

    }
}