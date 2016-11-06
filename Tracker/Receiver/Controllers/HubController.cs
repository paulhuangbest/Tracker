using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Entity;

namespace Receiver.Controllers
{
    public class HubController : Controller
    {

        public List<HubUrl> InUrl
        {
            get {
                if (ControllerContext.HttpContext.Application["InUrl"] == null)
                    ControllerContext.HttpContext.Application["InUrl"] = new List<HubUrl>();

                
                List<HubUrl> list = ControllerContext.HttpContext.Application["InUrl"] as List<HubUrl>;

                if (list.Count > 5)
                    list.RemoveAt(0);

                return list;
            }
            set {
                ControllerContext.HttpContext.Application["InUrl"] = value;
            }

        }
        
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
        public ActionResult Monitor()
        {
            return View(InUrl);
        }

        //
        // POST: /ReceiveHub/Create
        [HttpPost]
        public ActionResult PushData(FormCollection collection)
        {
            HubUrl url = new HubUrl();

            url.InDate = DateTime.Now;

            Dictionary<string, string> dic = new Dictionary<string, string>();

            

            foreach (string key in collection.AllKeys)
            {
                dic[key] = HttpUtility.HtmlDecode(collection[key]);

            }
            
            dic["status"] = LogStatus.Hub.ToString("D");

            url.ProjectKey = dic["key"];
            url.Status = dic["status"];
            url.Url = dic["url"];
            

            string message = JsonConvert.SerializeObject(dic);
            url.Body = message;

            string severity = "";
            LogType type = (LogType)Enum.Parse(typeof(LogType), dic["type"]);

            switch (type)
            {
                case LogType.ExceptionLog:
                    severity = "exception";
                    break;

                case LogType.OperateLog:
                    severity = "operate";
                    break;

                case LogType.SystemLog:
                    severity = "system";
                    break;

                case LogType.Normal:
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

            url.Type = type.ToString();
            url.MQ = severity;
            url.OutDate = DateTime.Now;

            InUrl.Add(url);

            

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
