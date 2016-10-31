using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Receiver.Controllers
{
    public class ReceiveHubController : Controller
    {
        //
        // GET: /ReceiveHub/
        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /ReceiveHub/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /ReceiveHub/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /ReceiveHub/Create
        [HttpPost]
        public ActionResult Data(FormCollection collection)
        {
            return new ContentResult() { Content = "success" };
        }

        //
        // GET: /ReceiveHub/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /ReceiveHub/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /ReceiveHub/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /ReceiveHub/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
