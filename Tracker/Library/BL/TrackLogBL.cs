using Couchbase;
using Entity;
using Framework.Couchbase;
using Library.Common;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Library.BL
{
    public class TrackLogBL
    {
        public TrackLogBL() { }

        public TrackLogBL(Func<Dictionary<string, string>, string> func)
        {
            Notice = func;

        }

        CouchbaseHelper helper = new CouchbaseHelper("default");

        public bool UpsertSystemLog(SystemLog log)
        {
            var doc = new Document<SystemLog>()
            {
                Id = log.LogId,
                Content = log
            };


            return helper.Upsert<SystemLog>(doc);
        }


        public bool UpsertOperateLog(OperateLog log)
        {
            var doc = new Document<OperateLog>()
            {
                Id = log.LogId,
                Content = log
            };


            return helper.Upsert<OperateLog>(doc);
        }

        public bool UpsertExceptionLog(ExceptionLog log)
        {
            var doc = new Document<ExceptionLog>()
            {
                Id = log.LogId,
                Content = log
            };


            return helper.Upsert<ExceptionLog>(doc);
        }

        public bool UpsertNormalLog(NormalLog log)
        {
            var doc = new Document<NormalLog>()
            {
                Id = log.LogId,
                Content = log
            };


            return helper.Upsert<NormalLog>(doc);
        }

        public Func<Dictionary<string, string>, string> Notice { get; set; }

        public void ResolveSystem(object send, BasicDeliverEventArgs ea)
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

                double ms = ts.TotalMilliseconds;

                Enum cl = null;
                if (ms <= 7000)
                    cl = CostLevel.Normal;
                else if (ms > 7000 && ms <= 12000)
                    cl = CostLevel.Warn;
                else
                    cl = CostLevel.Block;

                string pageName = "";
                string p1 = dic["url"].Split('?')[0];
                string[] p1s = p1.Split('/');
                pageName = p1s[p1s.Length - 1];


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

                string key = "wms_system_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

                SystemLog slog = new SystemLog
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
                    ServerIP = dic["sip"],
                    Tag = cl.ToString() + "," + dic["code"] + "," + pageName
                };

                //TrackLogBL logBL = new TrackLogBL();

                //logBL.UpsertSystemLog(slog);

                UpsertSystemLog(slog);

                if (Notice != null)
                {
                    Dictionary<string, string> args = new Dictionary<string, string>();
                    args["ip"] = dic["ip"];
                    args["tag"] = slog.Tag;
                    args["url"] = dic["url"];
                    args["ConsumerTag"] = ea.ConsumerTag;
                    args["type"] = "2";
                    args["projectKey"] = dic["key"];
                    args["logType"] = LogType.SystemLog.ToString();
                    args["ct"] = dic["ct"];

                    Notice(args);
                }


            }
            catch (Exception ex)
            {

            }
        }


        public void ResolveOperate(object send, BasicDeliverEventArgs ea)
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


                string logId = "wms_operate_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");


                OperateLog olog = new OperateLog()
                {
                    LogId = logId,
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
                    ServerIP = dic["sip"],
                    Tag = dic["user"] + "," + dic["action"]
                };


                var upsert = UpsertOperateLog(olog);

                if (Notice != null)
                {
                    Dictionary<string, string> args = new Dictionary<string, string>();
                    args["ip"] = dic["ip"];
                    args["tag"] = olog.Tag;
                    args["url"] = dic["url"];
                    args["ConsumerTag"] = ea.ConsumerTag;
                    args["type"] = "2";
                    args["projectKey"] = dic["key"];
                    args["logType"] = LogType.OperateLog.ToString();
                    args["ct"] = dic["ct"];

                    Notice(args);
                }
            }
            catch (Exception ex)
            {

            }
        }


        public void ResolveException(object send, BasicDeliverEventArgs ea)
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

                Enum el = null;
                string msg = dic["msg"].ToLower();

                if (msg.IndexOf("connect") >= 0 || msg.IndexOf("timeout") >= 0)
                    el = ExceptionLevel.Block;
                else
                    el = ExceptionLevel.Warn;


                string logId = "wms_exception_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");


                ExceptionLog elog = new ExceptionLog
                {
                    LogId = logId,
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
                    User = dic["user"],
                    Tag = el.ToString()
                };


                var upsert = UpsertExceptionLog(elog);

                if (Notice != null)
                {
                    Dictionary<string, string> args = new Dictionary<string, string>();
                    args["ip"] = dic["ip"];
                    args["tag"] = elog.Tag;
                    args["url"] = dic["url"];
                    args["ConsumerTag"] = ea.ConsumerTag;
                    args["type"] = "2";
                    args["projectKey"] = dic["key"];
                    args["logType"] = LogType.ExceptionLog.ToString();
                    args["ct"] = dic["ct"];

                    Notice(args);
                }


            }
            catch (Exception ex)
            {

            }



        }

        public void ResolveNormal(object send, BasicDeliverEventArgs ea)
        {
            try
            {
                var body = ea.Body;
                var message = System.Text.Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);

                Dictionary<string, string> content = new Dictionary<string, string>();

                foreach(KeyValuePair<string,string> item in dic)
                {
                    if (item.Key != "key" &&
                        item.Key != "subkey" &&
                        item.Key != "type" &&
                        item.Key != "status" &&
                        item.Key != "ct")
                    {
                        content[item.Key] = item.Value;
                    }
                }

                string logId = "wms_normal_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

                NormalLog nlog = new NormalLog
                {
                    LogId = logId,
                    ProjectKey = dic["key"],
                    SubKey = dic["subkey"],
                    Type = dic["type"],
                    Status = dic["status"],
                    CreateTime = DateTime.ParseExact(dic["ct"], "yyyy-MM-dd HH:mm:ss:fff", System.Globalization.CultureInfo.CurrentCulture),
                    Url = dic.Keys.Contains("url") ?dic["url"] :"",
                    RequestIP = dic.Keys.Contains("ip") ? dic["ip"] : "",
                    ServerIP = dic["sip"],
                    Content = content,

                    Tag = dic["subkey"]
                };

                var upsert = UpsertNormalLog(nlog);


                if (Notice != null)
                {
                    Dictionary<string, string> args = new Dictionary<string, string>();
                    args["ip"] = nlog.RequestIP;
                    args["tag"] = nlog.Tag;
                    args["url"] = nlog.Url;
                    args["ConsumerTag"] = ea.ConsumerTag;
                    args["type"] = "2";
                    args["projectKey"] = nlog.ProjectKey;
                    args["logType"] = LogType.Normal.ToString();
                    args["ct"] = dic["ct"];

                    Notice(args);
                }
            }
            catch (Exception ex)
            { 
            }
        }
    }
}
