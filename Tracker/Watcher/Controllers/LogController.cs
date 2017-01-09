using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.BL;
using Entity;

namespace Watcher.Controllers
{
    [Authorize]
    [RoutePrefix("log")]
    public class LogController : BaseController
    {
        // GET: Log
        public ActionResult Index()
        {
            return View();
        }

        [Route("ex/{projectKey}")]
        public ActionResult ExceptionLog(string projectKey)
        {
            
            WatcherBL bl = new WatcherBL();
            List<ExceptionLog> list = bl.DefaultExceptionLogList(projectKey);

            return View(list);
            
        }

        [Route("exlist")]
        public ActionResult SearchExceptionLog(FormCollection collection)
        {
            Dictionary<string, string> condition = new Dictionary<string, string>();

            condition.Add("CreateTime", collection["CreateTime"]);
            condition.Add("Keyword", collection["Keyword"]);
            condition.Add("Page", collection["Page"]);
            condition.Add("Subkey", collection["Subkey"]);
            condition.Add("Level", collection["Level"]);
            condition.Add("ProjectKey", collection["ProjectKey"]);

            WatcherBL bl = new WatcherBL();

            int count = bl.SearchCount(condition);

            List<ExceptionLog> list = new List<ExceptionLog>();

            if (count > 0)
            {
                list = bl.SearchExceptionLog(condition);
                //result.Data = new { data=list,itemsCount=5 };

            }
            

            ContentResult c = new ContentResult();
            c.ContentType = "application/json";

            c.Content = Newtonsoft.Json.JsonConvert.SerializeObject(new { data = list, itemsCount = count });

            return c;
        }

        [Route("exdetail")]
        public ActionResult GetExceptionLog(FormCollection collection)
        {
            ExceptionLog log = null;

            WatcherBL bl = new WatcherBL();

            if (collection.AllKeys.Contains("LogId"))
            {
                string logId = collection["LogId"];

                log = bl.GetExceptionLog(logId);

                
            }
            

            ContentResult c = new ContentResult();
            c.ContentType = "application/json";

            c.Content = Newtonsoft.Json.JsonConvert.SerializeObject(log);

            return c;
            
        }

        [Route("extl/{projectKey}")]
        public ActionResult ExceptionLogTimeLine(string projectKey)
        {

            Dictionary<string, string> condition = new Dictionary<string, string>();

            condition["CreateTime"] = Request["createTime"] != null ? Request["createTime"] : "";
            condition["Keyword"] = Request["keyword"] != null ? Request["keyword"] : "";
            condition["Subkey"] = Request["subkey"] != null ? Request["subkey"] : "";
            condition["Level"] = Request["level"] != null ? Request["level"] : "";
            condition["ProjectKey"] = projectKey;

            ViewBag.Condition = condition;

            WatcherBL bl = new WatcherBL();
            List<ExceptionLog> list = bl.GetExceptionLogTimeLine(condition);

            Dictionary<string, List<ExceptionLog>> data = new Dictionary<string, List<ExceptionLog>>();

            if (list.Count > 0)
            {
                DateTime min = list.First().CreateTime;
                DateTime max = list.Last().CreateTime;

                
                DateTime begin = min;
                DateTime end;

                while (true)
                {

                    end = begin.AddDays(1);

                    if (end >= max)
                    {
                        end = max;
                    }

                    List<ExceptionLog> group = list.Where(p => p.CreateTime < end && p.CreateTime >= begin).ToList();
                    data[begin.ToString("yyyy-MM-dd HH:mm:ss")] = group;


                    begin = end;

                    if (end == max)
                        break;
                }

            }
            

            return View(data);

        }
    }
}