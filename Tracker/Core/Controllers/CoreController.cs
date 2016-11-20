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
using Couchbase.N1QL;

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
                            var message = System.Text.Encoding.UTF8.GetString(body);
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
                                    var message = System.Text.Encoding.UTF8.GetString(body);
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
                var message = System.Text.Encoding.UTF8.GetString(body);
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
                var message = System.Text.Encoding.UTF8.GetString(body);
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
                var message = System.Text.Encoding.UTF8.GetString(body);
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

                Dictionary<string, List<HeartData>> heartData = HttpContext.Application[dic["key"]] as Dictionary<string, List<HeartData>>;

                if (heartData.Keys.Contains(mqName + "_" + index + 1))
                {
                    List<HeartData> list = heartData[mqName + "_" + index + 1];

                    if (list.Count(p => p.Type == "1") >= 5)
                    {
                        HeartData data = list.Last(p => p.Type == "1");
                        list.Remove(data);
                    }

                    list.Add(new HeartData
                    {
                        Type = "1",
                        Message = "heart",
                        Time = DateTime.Now
                    });
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
        public ActionResult Edit(string key)
        {
            CoreProfile profile = null;

            if (!string.IsNullOrEmpty(key))
            {
                using (var bucket = Cluster.OpenBucket("TrackInfo"))
                {
                    profile = bucket.GetDocument<CoreProfile>(key).Content;
                }
            }
            return View("ProfileDetail",profile);
        }

        public  ActionResult Profiles()
        {
            
            using (var bucket = Cluster.OpenBucket("TrackInfo"))
            {
                
                var queryRequest = new QueryRequest()
                .Statement("select enable,exceptionConsumerNum,modifyTime,mqServer,normalConsumerNum,operateConsumerNum,profileKey,projectKey,systemConsumerNum from TrackInfo where profileKey is not null")
                .Metrics(false);


                
                var result = bucket.Query<CoreProfile>(queryRequest);
                
                
                //CoreProfile b = bucket.Get<CoreProfile>("wms_profile_20161112014426258").Value;
                 return View("Profiles",result.Rows); 
            }

            //var result = Cluster.Query<CoreProfile>("select * from TrackInfo where profileKey is not null");

                  
            
            
        }
        
        [HttpPost]
        public JsonResult Upsert(CoreProfile profile)
        {
            try 
            {
                using (var bucket = Cluster.OpenBucket("TrackInfo"))
                {
                    profile.ModifyTime = DateTime.Now;
                    string key = "";

                    if (string.IsNullOrEmpty(profile.ProfileKey))
                    {
                        key = profile.ProjectKey + "_profile_" + profile.ModifyTime.ToString("yyyyMMddHHmmssfff");

                        profile.ProfileKey = key;
                    }
                    else
                    {
                        key = profile.ProfileKey;
                    }

                    var document = new Document<CoreProfile>()
                    {
                        Id = key,
                        Content = profile
                    };

                    var upsert = bucket.Upsert<CoreProfile>(document);
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

        public JsonResult StartConsumer(FormCollection collection)
        {
            List<Contain> tasklist = HttpContext.Application["TaskList"] as List<Contain>;

            if (tasklist != null)
            {
                string key = collection["key"];
                string pkey = collection["pkey"];

                using (var bucket = Cluster.OpenBucket("TrackInfo"))
                {

                    CoreProfile profile = bucket.Get<CoreProfile>(pkey).Value;
                    switch (key)
                    {
                        case "system":
                            for (int i = 0; i < profile.SystemConsumerNum; i++)
                            {
                                CreateConsumer("mq_system_" + profile.ProjectKey, profile.MQServer,profile.ProjectKey,i, new EventHandler<BasicDeliverEventArgs>(ResolveSystem));
                            }
                            break;

                        case "operate":
                            for (int i = 0; i < profile.OperateConsumerNum; i++)
                            {
                                CreateConsumer("mq_operate_" + profile.ProjectKey, profile.MQServer, profile.ProjectKey,i, new EventHandler<BasicDeliverEventArgs>(ResolveOperate));
                            }
                            break;

                        case "exception":
                            for (int i = 0; i < profile.ExceptionConsumerNum; i++)
                            {
                                CreateConsumer("mq_exception_" + profile.ProjectKey, profile.MQServer, profile.ProjectKey,i, new EventHandler<BasicDeliverEventArgs>(ResolveException));
                            }
                            break;

                        case "normal":
                            for (int i = 0; i < profile.NormalConsumerNum; i++)
                            {
                                CreateConsumer("mq_normal_" + profile.ProjectKey, profile.MQServer, profile.ProjectKey,i, new EventHandler<BasicDeliverEventArgs>(ResolveNormal));
                            }
                            break;
                    }
                }
                
                
            }

            return Json(new ResultDTO()
            {
                Status = "Success",
                Message = "",
                Data = ""
            });
            //
        }

        public JsonResult RemoveConsumer(FormCollection collection)
        {
            string profileKey = collection["pkey"];
            string key = collection["key"];

            using (var bucket = Cluster.OpenBucket("TrackInfo"))
            {

                CoreProfile profile = bucket.GetDocument<CoreProfile>(profileKey).Content;

                StopConsumer(profile.ProjectKey, key);
            }
            

            return Json(new ResultDTO()
            {
                Status = "Success",
                Message = "",
                Data = ""
            },JsonRequestBehavior.AllowGet);        
        }

        private void StopConsumer(string projectKey,string mqType)
        {
            if (string.IsNullOrEmpty(projectKey) && string.IsNullOrEmpty(mqType))
            {
                List<Contain> tasklist = HttpContext.Application["TaskList"] as List<Contain>;

                if (tasklist != null)
                {
                    foreach (Contain c in tasklist)
                    {
                        c.tokenSource.Cancel(false);

                        if (c.task != null && (c.task.Status == TaskStatus.Canceled || c.task.Status == TaskStatus.RanToCompletion || c.task.Status == TaskStatus.Faulted))
                            c.task.Dispose();


                    }
                    tasklist.Clear();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(projectKey) && string.IsNullOrEmpty(mqType))
                {
                    List<Contain> tasklist = HttpContext.Application["TaskList"] as List<Contain>;

                    if (tasklist != null)
                    {
                        List<Contain> list = tasklist.Where(p => p.projectKey == projectKey).ToList();

                        foreach (Contain c in list)
                        {
                            c.tokenSource.Cancel(false);

                            if (c.task != null && (c.task.Status == TaskStatus.Canceled || c.task.Status == TaskStatus.RanToCompletion || c.task.Status == TaskStatus.Faulted))
                                c.task.Dispose();


                            tasklist.Remove(c);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(projectKey) && !string.IsNullOrEmpty(mqType))
                {
                    List<Contain> tasklist = HttpContext.Application["TaskList"] as List<Contain>;

                    if (tasklist != null)
                    {
                        List<Contain> list = tasklist.Where(p => p.projectKey == projectKey && p.taskKey.Contains(mqType)).ToList();

                        foreach (Contain c in list)
                        {
                            c.tokenSource.Cancel(false);

                            if (c.task != null && (c.task.Status == TaskStatus.Canceled || c.task.Status == TaskStatus.RanToCompletion || c.task.Status == TaskStatus.Faulted))
                                c.task.Dispose();


                            tasklist.Remove(c);
                        }
                    }
                }
                

            }

        }

        private void CreateConsumer(string mqName ,string mqServer, string projectKey,int index,EventHandler<BasicDeliverEventArgs> handler)
        {

            if (HttpContext.Application["TaskList"] == null)
                HttpContext.Application["TaskList"] = new List<Contain>();
            

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

                            Dictionary<string, List<HeartData>> heartData = HttpContext.Application[projectKey] as Dictionary<string, List<HeartData>>;

                            if (heartData.Keys.Contains(mqName + "_" + index + 1))
                            {
                                List<HeartData> list = heartData[mqName + "_" + index + 1];

                                if (list.Count(p => p.Type == "1") >= 5)
                                {
                                    HeartData data = list.Last(p => p.Type == "1");
                                    list.Remove(data);
                                }

                                list.Add(new HeartData { 
                                    Type = "1",
                                    Message = "heart",
                                    Time = DateTime.Now
                                });
                            }

                            Thread.Sleep(5000);
                        }
                    }
                }

            }, ct);


            Contain c = new Contain { task = t, tokenSource = tokenSource, taskKey = mqName, projectKey = projectKey };

            List<Contain> tasklist = HttpContext.Application["TaskList"] as List<Contain>;
            tasklist.Add(c);

            Dictionary<string, List<HeartData>> heart = HttpContext.Application[projectKey] as Dictionary<string, List<HeartData>>;
            
            heart[mqName + "_" + (index + 1)] = new List<HeartData>();
            //if (!heart.Keys.Contains(mqName))
            //    heart[mqName + "_1"] = new List<HeartData>();
            //else
            //{
            //    int total = heart.Keys.Count(p => p.Contains(mqName));
            //    heart[mqName + "_" + (total + 1)] = new List<HeartData>();
            //}
        }

        public ActionResult InitProfile(string key)
        {
            

            try
            {
                
                using (var bucket = Cluster.OpenBucket("TrackInfo"))
                {

                    CoreProfile profile = bucket.GetDocument<CoreProfile>(key).Content;

                    StopConsumer(profile.ProjectKey, null);

                    CreateMQ(profile);

                    InitHeartData(profile.ProjectKey);

                    string[] types = Enum.GetNames(typeof(LogType));

                    foreach (string type in types)
                    {
                        switch (type)
                        {
                            case "ExceptionLog":

                                for (int i = 0; i < profile.ExceptionConsumerNum; i++)
                                {
                                    CreateConsumer("mq_exception_" + profile.ProjectKey, profile.MQServer,profile.ProjectKey,i, new EventHandler<BasicDeliverEventArgs>(ResolveException));
                                }

                                break;

                            case "OperateLog":

                                for (int i = 0; i < profile.OperateConsumerNum; i++)
                                {
                                    CreateConsumer("mq_operate_" + profile.ProjectKey, profile.MQServer,profile.ProjectKey,i, new EventHandler<BasicDeliverEventArgs>(ResolveOperate));

                                }

                                break;

                            case "SystemLog":

                                for (int i = 0; i < profile.SystemConsumerNum; i++)
                                {
                                    CreateConsumer("mq_system_" + profile.ProjectKey, profile.MQServer,profile.ProjectKey,i, new EventHandler<BasicDeliverEventArgs>(ResolveSystem));
                                }

                                break;

                            case "Normal":

                                for (int i = 0; i < profile.NormalConsumerNum; i++)
                                {
                                    CreateConsumer("mq_normal_" + profile.ProjectKey, profile.MQServer,profile.ProjectKey,i, new EventHandler<BasicDeliverEventArgs>(ResolveNormal));
                                }

                                break;
                        }
                    }

                }

                using (var bucket = Cluster.OpenBucket("TrackInfo"))
                {

                    var queryRequest = new QueryRequest()
                    .Statement("select enable,exceptionConsumerNum,modifyTime,mqServer,normalConsumerNum,operateConsumerNum,profileKey,projectKey,systemConsumerNum from TrackInfo where profileKey is not null")
                    .Metrics(false);



                    var result = bucket.Query<CoreProfile>(queryRequest);


                    //CoreProfile b = bucket.Get<CoreProfile>("wms_profile_20161112014426258").Value;
                    return View("Profiles", result.Rows);
                }
                //return Json(new ResultDTO()
                //{
                //    Status = "Success",
                //    Message = "",
                //    Data = ""
                //},JsonRequestBehavior.AllowGet);


            }
            catch (Exception e)
            {
                return View("Profiles");
                //return Json(new ResultDTO()
                //{
                //    Status = "Fail",
                //    Message = "",
                //    Data = ""
                //}, JsonRequestBehavior.AllowGet);
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

        private void InitHeartData(string projectKey)
        {
            if (HttpContext.Application[projectKey] == null)
            {
                HttpContext.Application[projectKey] = new Dictionary<string, List<HeartData>>();//<consumerName,List<HeartData>>
            }
            else
            {
                Dictionary<string, List<HeartData>> dic = HttpContext.Application[projectKey] as Dictionary<string, List<HeartData>>;
                dic.Clear();
            }
        }


    }


}
