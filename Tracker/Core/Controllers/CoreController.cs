﻿using RabbitMQ.Client;
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

                Dictionary<string, string> extend = new Dictionary<string, string>();

                foreach (string key in dic.Keys)
                {
                    if (key.Contains("extend"))
                        extend[key.Replace("extend_", "")] = dic[key];

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
                            SubKey = dic["subkey"],
                            Type = dic["type"],
                            Status = dic["status"],
                            CreateTime = DateTime.ParseExact(dic["ct"], "yyyy-MM-dd HH:mm:ss:fff", System.Globalization.CultureInfo.CurrentCulture),
                            Url = dic["url"],
                            User = dic["user"],
                            Action = dic["action"],
                            ActionType = dic["actionType"],
                            Section = dic["section"],
                            Stack = stack,
                            Extend = extend,
                            RequestIP = dic["ip"],
                            ServerIP = dic["sip"]
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

                List<string> cookies = null;

                if (!string.IsNullOrEmpty(dic["cookies"]))
                {
                    string c = HttpUtility.HtmlDecode(dic["cookies"]);

                    cookies = c.Split(new string[] { "@@" }, StringSplitOptions.RemoveEmptyEntries).ToList();

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
                            SubKey = dic["subkey"],
                            Type = dic["type"],
                            Status = dic["status"],
                            CreateTime = DateTime.ParseExact(dic["ct"], "yyyy-MM-dd HH:mm:ss:fff", System.Globalization.CultureInfo.CurrentCulture),
                            Url = dic["url"],
                            Interval = ts.TotalMilliseconds.ToString(),
                            BeginTime = begin,
                            EndTime = end,
                            QueryString = dic["query"],
                            PostArgument = argument,
                            RequestIP = dic["ip"],
                            Cookies = cookies,
                            StatusCode = dic["code"],
                            Method = dic["method"],
                            ServerIP = dic["sip"]
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

                Dictionary<string, string> extend = new Dictionary<string, string>();

                foreach (string key in dic.Keys)
                {
                    if (key.Contains("extend"))
                        extend[key.Replace("extend_", "")] = dic[key];

                }

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
                            SubKey = dic["subkey"],
                            Type = dic["type"],
                            Status = dic["status"],
                            CreateTime = DateTime.ParseExact(dic["ct"], "yyyy-MM-dd HH:mm:ss:fff", System.Globalization.CultureInfo.CurrentCulture),
                            Url = dic["url"],
                            ExceptionMessage = dic["msg"],
                            RequestIP = dic["ip"],
                            ServerIP = dic["sip"],
                            Extend = extend,
                            User = dic["user"]
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
        public ActionResult Edit(int? id)
        {
            return View("ProfileDetail");
        }

        
        [HttpPost]
        public JsonResult Upsert(CoreProfile profile)
        {
            try 
            {
                using (var bucket = Cluster.OpenBucket("TrackInfo"))
                {
                    profile.ModifyTime = DateTime.Now;

                    string key = profile.ProjectKey + "_profile_" + profile.ModifyTime.ToString("yyyyMMddHHmmssfff");

                    profile.ProfileKey = key;

                    var document = new Document<CoreProfile>()
                    {
                        Id = key,
                        Content = profile
                    };

                    var upsert = bucket.Upsert(document);
                }

                return Json(new ResultDTO()
                {
                    Status = "Success",
                    Message = "",
                    Data = ""
                });
            }
            catch(Exception e) 
            {
                return Json(new ResultDTO()
                {
                    Status = "Fail",
                    Message = "",
                    Data = ""
                });
            }
            

            
        }

        private void CreateMQ(CoreProfile profile)
        {
            if (HttpContext.Application["TaskList"] == null)
                HttpContext.Application["TaskList"] = new List<Contain>();


            var factory = new ConnectionFactory() { HostName = profile.MQServer };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: "direct_" + profile.ProjectKey,
                                            type: "direct");


                    string[] types = Enum.GetNames(typeof(LogType));

                    foreach (string type in types)
                    {
                        string queue = "", severity = "";

                        switch (type)
                        {
                            case "ExceptionLog":
                                queue = "mq_exception_" + profile.ProjectKey;
                                severity = "exception";
                                break;

                            case "OperateLog":
                                queue = "mq_operate_" + profile.ProjectKey;
                                severity = "operate";
                                break;

                            case "SystemLog":
                                queue = "mq_system_" + profile.ProjectKey;
                                severity = "system";
                                break;

                            case "Normal":
                                queue = "mq_normal_" + profile.ProjectKey;
                                severity = "normal";
                                break;
                        }



                        var queueName = channel.QueueDeclare(queue, true, false, false, null);


                        channel.QueueBind(queue: queueName,
                                            exchange: "direct_" + profile.ProjectKey,
                                            routingKey: severity);
                        
                    }

                }
            }


            
        }

        private void CreateConsumer(string mqName ,string mqServer,EventHandler<BasicDeliverEventArgs> handler)
        {
            var tokenSource = new CancellationTokenSource();
            CancellationToken ct = tokenSource.Token;

            Task t = Task.Run(() =>
            {
                var factory = new ConnectionFactory() { HostName = mqServer };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += handler;

                        channel.BasicConsume(queue: mqName,
                             noAck: true,
                             consumer: consumer);

                        while (true)
                        {
                            if (ct.IsCancellationRequested)
                            {
                                break;
                            }
                            Thread.Sleep(5000);
                        }
                    }
                }

            }, ct);

            Contain c = new Contain { task = t, tokenSource = tokenSource };

            List<Contain> tasklist = HttpContext.Application["TaskList"] as List<Contain>;
            tasklist.Add(c);
        }

        public void InitProfile(string key)
        {
            using (var bucket = Cluster.OpenBucket("TrackInfo"))
            {
                CoreProfile profile = bucket.GetDocument<CoreProfile>(key).Content;


                CreateMQ(profile);


                string[] types = Enum.GetNames(typeof(LogType));

                foreach (string type in types)
                {
                    switch (type)
                    {
                        case "ExceptionLog":

                            for (int i = 0; i < profile.ExceptionConsumerNum; i++)
                            {
                                CreateConsumer("mq_exception_" + profile.ProjectKey, profile.MQServer, new EventHandler<BasicDeliverEventArgs>(ResolveException));
                            }

                            break;

                        case "OperateLog":

                            for (int i = 0; i < profile.OperateConsumerNum; i++)
                            {
                                CreateConsumer("mq_operate_" + profile.ProjectKey, profile.MQServer, new EventHandler<BasicDeliverEventArgs>(ResolveOperate));

                            }

                            break;

                        case "SystemLog":

                            for (int i = 0; i < profile.SystemConsumerNum; i++)
                            {
                                CreateConsumer("mq_system_" + profile.ProjectKey, profile.MQServer, new EventHandler<BasicDeliverEventArgs>(ResolveSystem));
                            }

                            break;

                        case "Normal":

                            for (int i = 0; i < profile.NormalConsumerNum; i++)
                            {
                                CreateConsumer("mq_normal_" + profile.ProjectKey, profile.MQServer, new EventHandler<BasicDeliverEventArgs>(ResolveNormal));
                            }

                            break;
                    }
                }


            }
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

        public ActionResult Stop()
        {
            List<Contain> tasklist = HttpContext.Application["TaskList"] as List<Contain>;

            tasklist[0].tokenSource.Cancel(false);
            if (tasklist[0].task != null && (tasklist[0].task.Status == TaskStatus.Canceled || tasklist[0].task.Status == TaskStatus.RanToCompletion || tasklist[0].task.Status == TaskStatus.Faulted))
                tasklist[0].task.Dispose();

            tasklist.RemoveAt(0);

            return View("Index");
        }


    }

    public class Contain
    {
        public Task task { get; set; }
        public CancellationTokenSource tokenSource { get; set; }
    }
}
