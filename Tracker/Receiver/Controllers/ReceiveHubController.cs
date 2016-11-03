using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        
        public void Push()
        {
            Task.Factory.StartNew(() =>
            {
                //while (true)
                {
                    var factory = new ConnectionFactory() { HostName = "localhost" };
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(exchange: "direct_logs",
                                                type: "direct");

                        var severity = "info";
                        var message = "Hello World!";
                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(exchange: "direct_logs",
                                             routingKey: severity,
                                             basicProperties: null,
                                             body: body);

                    }
                }
            });
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
