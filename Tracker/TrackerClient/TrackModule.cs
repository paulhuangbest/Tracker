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

            TrackLog["url"] = context.Request.Url.OriginalString;
            TrackLog["method"] = context.Request.HttpMethod;
            TrackLog["begin"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
            TrackLog["query"] = context.Request.Url.Query;

            string post = "";
            foreach (string key in context.Request.Form.Keys)
            {
                if (key.IndexOf("__VIEWSTATE") <0 && key.IndexOf("__EVENT") <0)
                    post += key +"="+ context.Request.Form[key] + "&";
            }

            foreach (string key in context.Request.Files.Keys)
            {
                post += key + "=" + context.Request.Files[key].FileName + "&";
            }

            post = post.Trim('&');

            TrackLog["post"] = post;

            string userHostAddress = HttpContext.Current.Request.UserHostAddress;

            if (string.IsNullOrEmpty(userHostAddress))
            {
                userHostAddress = context.Request.ServerVariables["REMOTE_ADDR"];
            }

            TrackLog["ip"] = userHostAddress;

            string strCookies = "";
            foreach (string key in context.Request.Cookies.Keys)
            {
                HttpCookie cookie = context.Request.Cookies[key];
                strCookies += cookie.Name + "=" + cookie.Value + ";path=" + cookie.Path + ";expires=" + cookie.Expires + ";domain=" + cookie.Domain + ";request@@";
            }

            strCookies = strCookies.Trim('@', '@');
            TrackLog["cookies"] = strCookies;
        }

        public void Application_EndRequest(object sender, EventArgs e)
        {

            HttpApplication application = sender as HttpApplication;

            HttpContext context = application.Context;

            HttpResponse response = context.Response;

            //response.Write("这是来自自定义HttpModule中有EndRequest");
            TrackLog["end"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");

            string strCookies = "";
            foreach (string key in context.Response.Cookies.Keys)
            {
                HttpCookie cookie = context.Response.Cookies[key];
                strCookies += cookie.Name + "=" + cookie.Value + ";path=" + cookie.Path + ";expires=" + cookie.Expires + ";domain=" + cookie.Domain + ";response@@";
            }

            strCookies = strCookies.Trim('@', '@');
            TrackLog["cookies"] += strCookies;

            TrackLog["code"] = response.StatusCode.ToString();

            Client.SystemLog(TrackLog);

        }
    }
}
