using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace TrackerClient
{
    public class TrackModule : IHttpModule
    {

        public void Dispose()
        {
            //throw new NotImplementedException();
            TrackLog = null;
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(Application_BeginRequest);

            context.EndRequest += new EventHandler(Application_EndRequest);
        }

        public Dictionary<string, string> TrackLog { get; set; }

        public void Application_BeginRequest(object sender, EventArgs e)
        {

            HttpApplication application = sender as HttpApplication;

            HttpContext context = application.Context;

            TrackLog = new Dictionary<string,string>();

            //HttpResponse response = context.Response;

            TrackLog["url"] = context.Request.RawUrl;
            TrackLog["method"] = context.Request.HttpMethod;
            TrackLog["begin"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");

        }

        public void Application_EndRequest(object sender, EventArgs e)
        {

            HttpApplication application = sender as HttpApplication;

            HttpContext context = application.Context;

            HttpResponse response = context.Response;

            //response.Write("这是来自自定义HttpModule中有EndRequest");
            TrackLog["end"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");

            Client.SystemLog(TrackLog);

        }
    }
}
