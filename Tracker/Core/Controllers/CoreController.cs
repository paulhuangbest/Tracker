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
using Library.BL;
using Library.Common;


namespace Core.Controllers
{
    public class CoreController : Controller
    {
        // GET: Core
        public ActionResult Index()
        {
            return View();
        }



        private static readonly Cluster Cluster = new Cluster("couchbaseClients/couchbase");



        public void remove(string queuename)
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
                ProfileBL bl = new ProfileBL();

                profile = bl.GetProfile(key);
            }

            return View("ProfileDetail", profile);
        }

        public ActionResult Profiles()
        {

            ProfileBL bl = new ProfileBL();

            List<CoreProfile> profiles = bl.GetProfileList();

            return View("Profiles", profiles);

        }

        [HttpPost]
        public JsonResult Upsert(CoreProfile profile)
        {
            try
            {
                profile.ModifyTime = DateTime.Now;
                string key = "";

                if (string.IsNullOrEmpty(profile.ProfileKey))
                {
                    key = profile.ProjectKey + "_profile_" + profile.ModifyTime.ToString("yyyyMMddHHmmssfff");

                    profile.ProfileKey = key;
                }

                ProfileBL bl = new ProfileBL();

                bl.UpsertProfile(profile);


                return Json(new ResultDTO()
                {
                    Status = "Success",
                    Message = "",
                    Data = ""
                });
            }
            catch (Exception e)
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


                MQInfo info = new MQInfo()
                {
                    HostName = profile.MQServer,
                    ExchangeName = "direct_" + profile.ProjectKey.ToLower(),
                    ExchangeType = "direct",
                    QueueName = queue,
                    RoutingKey = severity
                };


                RabbitMQHelper.CreateMQ(info);

            }

        }

        public JsonResult StartConsumer(FormCollection collection)
        {
            List<Contain> tasklist = HttpContext.Application["TaskList"] as List<Contain>;

            if (tasklist != null)
            {
                string key = collection["key"];
                string pkey = collection["pkey"];

                ProfileBL pbl = new ProfileBL();
                CoreProfile profile = pbl.GetProfile(pkey);

                switch (key)
                {
                    case "system":
                        for (int i = 0; i < profile.SystemConsumerNum; i++)
                        {
                            CreateConsumer("mq_system_" + profile.ProjectKey, profile.MQServer, profile.ProjectKey, i, new EventHandler<BasicDeliverEventArgs>(new TrackLogBL(SendHeartData).ResolveSystem));
                        }
                        break;

                    case "operate":
                        for (int i = 0; i < profile.OperateConsumerNum; i++)
                        {
                            CreateConsumer("mq_operate_" + profile.ProjectKey, profile.MQServer, profile.ProjectKey, i, new EventHandler<BasicDeliverEventArgs>(new TrackLogBL(SendHeartData).ResolveOperate));
                        }
                        break;

                    case "exception":
                        for (int i = 0; i < profile.ExceptionConsumerNum; i++)
                        {
                            CreateConsumer("mq_exception_" + profile.ProjectKey, profile.MQServer, profile.ProjectKey, i, new EventHandler<BasicDeliverEventArgs>(new TrackLogBL(SendHeartData).ResolveException));
                        }
                        break;

                    case "normal":
                        for (int i = 0; i < profile.NormalConsumerNum; i++)
                        {
                            CreateConsumer("mq_normal_" + profile.ProjectKey, profile.MQServer, profile.ProjectKey, i, new EventHandler<BasicDeliverEventArgs>(new TrackLogBL(SendHeartData).ResolveNormal));
                        }
                        break;
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

            ProfileBL bl = new ProfileBL();
            CoreProfile profile = bl.GetProfile(profileKey);

            StopConsumer(profile.ProjectKey, key);


            return Json(new ResultDTO()
            {
                Status = "Success",
                Message = "",
                Data = ""
            }, JsonRequestBehavior.AllowGet);
        }

        private void StopConsumer(string projectKey, string mqType)
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

        private string SendHeartData(Dictionary<string, string> args)
        {
            Dictionary<string, List<HeartData>> heartData = HttpContext.Application[args["projectKey"]] as Dictionary<string, List<HeartData>>;

            if (args["type"] == "1")
            {

                if (heartData.Keys.Contains(args["mqName"] + "_" + (int.Parse(args["index"]) + 1)))
                {
                    List<HeartData> list = heartData[args["mqName"] + "_" + (int.Parse(args["index"]) + 1)];

                    if (list.Count(p => p.Type == "1") >= 5)
                    {
                        HeartData data = list.First(p => p.Type == "1");
                        list.Remove(data);
                    }

                    list.Add(new HeartData
                    {
                        Type = "1",
                        Message = "heart",
                        Time = DateTime.Now.ToString()
                    });
                }
            }
            else if (args["type"] == "2")
            {

                if (heartData.Keys.Contains(args["ConsumerTag"]))
                {
                    List<HeartData> list = heartData[args["ConsumerTag"]];

                    if (list.Count(p => p.Type == "2") >= 5)
                    {
                        HeartData data = list.First(p => p.Type == "2");
                        list.Remove(data);
                    }

                    list.Add(new HeartData
                    {
                        Type = "2",
                        Message = args["ip"] + "|" + args["tag"] + "|" + args["url"] + "|" + args["logType"],
                        Time = args["ct"]
                    });
                }
            }



            return "";
        }
        private void CreateConsumer(string mqName, string mqServer, string projectKey, int index, EventHandler<BasicDeliverEventArgs> handler)
        {
            if (HttpContext.Application["TaskList"] == null)
                HttpContext.Application["TaskList"] = new List<Contain>();


            Dictionary<string, string> args = new Dictionary<string, string>();
            args["projectKey"] = projectKey;
            args["mqName"] = mqName;
            args["index"] = index.ToString();
            args["type"] = "1";

            ConsumerInfo info = new ConsumerInfo()
            {
                Handler = handler,
                HostName = mqServer,
                QueueName = mqName,
                ConsumerTag = mqName + "_" + (index + 1),
                Args = args,
                Notice = new Func<Dictionary<string, string>, string>(SendHeartData)
            };


            ConsumerTask cTask = RabbitMQHelper.CreateConsumer(info);

            Contain c = new Contain { task = cTask.Task, tokenSource = cTask.TokenSource, taskKey = mqName, projectKey = projectKey };

            List<Contain> tasklist = HttpContext.Application["TaskList"] as List<Contain>;
            tasklist.Add(c);

            Dictionary<string, List<HeartData>> heart = HttpContext.Application[projectKey] as Dictionary<string, List<HeartData>>;

            heart[mqName + "_" + (index + 1)] = new List<HeartData>();


        }


        public ActionResult InitProfile(string key)
        {

            try
            {
                ProfileBL bl = new ProfileBL();

                CoreProfile profile = bl.GetProfile(key);


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
                                CreateConsumer("mq_exception_" + profile.ProjectKey, profile.MQServer, profile.ProjectKey, i, new EventHandler<BasicDeliverEventArgs>(new TrackLogBL(SendHeartData).ResolveException));
                            }

                            break;

                        case "OperateLog":

                            for (int i = 0; i < profile.OperateConsumerNum; i++)
                            {
                                CreateConsumer("mq_operate_" + profile.ProjectKey, profile.MQServer, profile.ProjectKey, i, new EventHandler<BasicDeliverEventArgs>(new TrackLogBL(SendHeartData).ResolveOperate));

                            }

                            break;

                        case "SystemLog":

                            for (int i = 0; i < profile.SystemConsumerNum; i++)
                            {
                                CreateConsumer("mq_system_" + profile.ProjectKey, profile.MQServer, profile.ProjectKey, i, new EventHandler<BasicDeliverEventArgs>(new TrackLogBL(SendHeartData).ResolveSystem));
                            }

                            break;

                        case "Normal":

                            for (int i = 0; i < profile.NormalConsumerNum; i++)
                            {
                                CreateConsumer("mq_normal_" + profile.ProjectKey, profile.MQServer,profile.ProjectKey,i, new EventHandler<BasicDeliverEventArgs>(new TrackLogBL(SendHeartData).ResolveNormal));
                            }

                            break;
                    }
                }


                List<CoreProfile> profiles = bl.GetProfileList();

                return View("Profiles", profiles);

            }
            catch (Exception e)
            {
                return View("Profiles");
            }
        }

        // POST: Core/Delete/5

        public ActionResult Delete(string key)
        {

            ProfileBL bl = new ProfileBL();

            CoreProfile profile = bl.GetProfile(key);

            StopConsumer(profile.ProjectKey, null);

            bl.RemoveProfile(key);


            List<CoreProfile> profiles = bl.GetProfileList();

            return View("Profiles", profiles);

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

        public ActionResult Monitor()
        {
            ProfileBL bl = new ProfileBL();

            List<CoreProfile> profiles = bl.GetProfileList();

            return View(profiles);

        }
    }


}
