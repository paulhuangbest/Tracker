using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Entity;
using Couchbase;
using Newtonsoft.Json;

namespace Core.Controllers
{
    public class CoreController : Controller
    {
        // GET: Core
        public ActionResult Index()
        {
            return View();
        }

        
        public void Join()
        {
            Task.Factory.StartNew(() =>
            {
                
                    var factory = new ConnectionFactory() { HostName = "localhost" };
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(exchange: "direct_logs",
                                                type: "direct");
                        var queueName = channel.QueueDeclare("q_system", true, false, false, null);



                        //foreach (var severity in args)
                        {
                            channel.QueueBind(queue: queueName,
                                              exchange: "direct_logs",
                                              routingKey: "info");
                        }



                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (model, ea) =>
                        {
                            var body = ea.Body;
                            var message = Encoding.UTF8.GetString(body);
                            var routingKey = ea.RoutingKey;

                        };

                        channel.BasicConsume(queue: queueName,
                                             noAck: true,
                                             consumer: consumer);

                        while (true)
                        {
                            

                            Thread.Sleep(5000);
                        }
                        


                    }
                    
            });

            
        }

        private static readonly Cluster Cluster = new Cluster("couchbaseClients/couchbase");

        public void Split()
        {

            string[] types = Enum.GetNames(typeof(LogType));

            
            foreach (string type in types)
            {
                Task t = Task.Factory.StartNew(() =>
                {
                    string queue = "", severity = "";

                    switch (type)
                    {
                        case "ExceptionLog":
                            queue = "q_exception";
                            severity = "exception";
                            break;

                        case "OperateLog":
                            queue = "q_operate";
                            severity = "operate";
                            break;

                        case "SystemLog":
                            queue = "q_system";
                            severity = "system";
                            break;

                        case "Normal":
                            queue = "q_normal";
                            severity = "normal";
                            break;
                    }



                    var factory = new ConnectionFactory() { HostName = "localhost" };
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(exchange: "direct_logs",
                                                type: "direct");
                        var queueName = channel.QueueDeclare(queue, true, false, false, null);



                        //foreach (var severity in args)
                        {
                            channel.QueueBind(queue: queueName,
                                              exchange: "direct_logs",
                                              routingKey: severity);
                        }



                        var consumer = new EventingBasicConsumer(channel);


                        switch (type)
                        {
                            case "ExceptionLog":
                                consumer.Received += new EventHandler<BasicDeliverEventArgs>(ResolveException);
                                break;

                            case "OperateLog":
                                consumer.Received += new EventHandler<BasicDeliverEventArgs>(ResolveOperate);
                                break;

                            case "SystemLog":
                                consumer.Received += new EventHandler<BasicDeliverEventArgs>(ResolveSystem);
                                break;

                            case "Normal":
                                consumer.Received += new EventHandler<BasicDeliverEventArgs>(ResolveNormal);
                                break;

                            default:

                                consumer.Received += (model, ea) =>
                                {
                                    var body = ea.Body;
                                    var message = Encoding.UTF8.GetString(body);
                                    var routingKey = ea.RoutingKey;

                                    using (var bucket = Cluster.OpenBucket())
                                    {
                                        var document = new Document<dynamic>() {
                                            Id = "Hello",
                                            Content = new
                                            {
                                                name = message
                                            }
                                        };

                                        var upsert = bucket.Upsert(document);
                                    }

                                    
                                };
                                break;
                        }
                        

                        channel.BasicConsume(queue: queueName,
                                             noAck: true,
                                             consumer: consumer);

                        while (true)
                        {


                            Thread.Sleep(5000);
                        }



                    }

                });


            }
            

            
        }

        private void ResolveNormal(object send, BasicDeliverEventArgs ea)
        { }

        private void ResolveOperate(object send, BasicDeliverEventArgs ea)
        {
            try
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);

                List<string> stack = new List<string>();

                for (int i = 0; i < 10; i++)
                {
                    if (dic.Keys.Contains("stack" + i))
                    {
                        stack.Add(dic["stack" + i]);
                    }
                    else
                        break;
                    
                }

                using (var bucket = Cluster.OpenBucket("default"))
                {
                    string key = "wms_operate_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

                    var document = new Document<OperateLog>()
                    {
                        Id = key,
                        Content = new OperateLog
                        {
                            LogId = key,
                            ProjectKey = dic["key"],
                            Type = dic["type"],
                            Status = dic["status"],
                            CreateTime = DateTime.ParseExact(dic["ct"], "yyyy-MM-dd HH:mm:ss:fff", System.Globalization.CultureInfo.CurrentCulture),
                            Url = dic["url"],
                            User = dic["user"],
                            Action = dic["action"],
                            Flow = dic["flow"],
                            Stack = stack,
                            IP = dic["ip"]
                        }
                    };

                    var upsert = bucket.Upsert(document);

                }
            }
            catch (Exception ex)
            {

            }
        }


        private void ResolveSystem(object send, BasicDeliverEventArgs ea)
        {                       

            try
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);


                DateTime begin = DateTime.ParseExact(dic["begin"], "yyyy-MM-dd HH:mm:ss:fff", System.Globalization.CultureInfo.CurrentCulture);
                DateTime end = DateTime.ParseExact(dic["end"], "yyyy-MM-dd HH:mm:ss:fff", System.Globalization.CultureInfo.CurrentCulture);
                TimeSpan ts = (TimeSpan)(end - begin);

                List<string> argument = null;

                if (!string.IsNullOrEmpty(dic["post"]))
                {
                    string post = HttpUtility.HtmlDecode(dic["post"]);

                    argument = post.Split('&').ToList();
                    
                }

                using (var bucket = Cluster.OpenBucket("default"))
                {
                    string key = "wms_system_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

                    var document = new Document<SystemLog>()
                    {
                        Id = key,
                        Content = new SystemLog
                        {
                            LogId = key,
                            ProjectKey = dic["key"],
                            Type = dic["type"],
                            Status = dic["status"],
                            CreateTime = DateTime.ParseExact(dic["ct"], "yyyy-MM-dd HH:mm:ss:fff", System.Globalization.CultureInfo.CurrentCulture),
                            Url = dic["url"],
                            Interval = ts.TotalMilliseconds.ToString(),
                            BeginTime = begin,
                            EndTime = end,
                            QueryString = dic["query"],
                            PostArgument = argument,
                            IP = dic["ip"]
                        }
                    };

                    var upsert = bucket.Upsert(document);

                }
            }
            catch (Exception ex)
            {

            }
        }

        private void ResolveException(object send, BasicDeliverEventArgs ea)
        {
            try
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);

                using (var bucket = Cluster.OpenBucket("default"))
                {
                    string key = "wms_exception_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

                    var document = new Document<ExceptionLog>()
                    {
                        Id = key,
                        Content = new ExceptionLog
                        {
                            LogId = key,
                            ProjectKey = dic["key"],
                            Type = dic["type"],
                            Status = dic["status"],
                            CreateTime = DateTime.ParseExact(dic["ct"], "yyyy-MM-dd HH:mm:ss:fff", System.Globalization.CultureInfo.CurrentCulture),
                            Url = dic["url"],
                            ExceptionMessage = dic["msg"],
                            IP = dic["ip"]
                        }
                    };

                    var upsert = bucket.Upsert(document);

                }
            }
            catch(Exception ex)
            {

            }

            
            
        }
        public void remove (string queuename)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDelete("q_system", true, true);
                    }

        }

        // POST: Core/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Core/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Core/Edit/5
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

        // GET: Core/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Core/Delete/5
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
