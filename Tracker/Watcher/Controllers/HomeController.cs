using Entity;
using Library.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Watcher.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
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