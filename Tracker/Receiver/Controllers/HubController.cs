using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Receiver.Controllers
{
    public class HubController : Controller
    {
        //
        // GET: /ReceiveHub/
        public ActionResult Index()
        {
            return View();
        }
        
        
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
        public ActionResult PushData(FormCollection collection)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            foreach (string key in collection.AllKeys)
            {
                dic[key] = collection[key];

            }

            string message = JsonConvert.SerializeObject(dic);
            string severity = "";

            switch (dic["type"])
            {
                case "ExceptionLog":
                    severity = "exception";
                    break;

                case "OperateLog":
                    severity = "operate";
                    break;

                case "SystemLog":
                    severity = "system";
                    break;

                case "Normal":
                    severity = "normal";
                    break;
            }

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "direct_logs",
                                        type: "direct");

                
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "direct_logs",
                                     routingKey: severity,
                                     basicProperties: null,
                                     body: body);

            }

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
