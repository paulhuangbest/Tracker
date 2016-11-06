using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace TrackerClient
{
    public class Client
    {

        public static string ProjectKey { get; set; }

        public static void ExceptionLog(Exception ex)
        {
            string url = HttpContext.Current.Request.Url.OriginalString;

            Task.Factory.StartNew(() => {
                //projectkey from web.config
                //log type 
                //create time
                //status
                //exception message


                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["key"] = ProjectKey;
                dic["type"] = LogType.ExceptionLog.ToString("D");
                dic["ct"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dic["status"] = LogStatus.Send.ToString("D");
                dic["msg"] = GetExceptionMessage(ex);
                dic["url"] = url;


                Post(dic);    

            });
            

        }

        

        public static void OperateLog(string trackUser , string action ,string flowName)
        {
            string url = HttpContext.Current.Request.Url.OriginalString;
            Task.Factory.StartNew(() =>
            {

                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["key"] = ProjectKey;
                dic["type"] = LogType.OperateLog.ToString("D");
                dic["ct"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                dic["status"] = LogStatus.Send.ToString("D");
                dic["user"] = trackUser;
                dic["action"] = action;
                dic["flow"] = flowName;
                dic["url"] = url;


                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                System.Diagnostics.StackFrame[] sfs = st.GetFrames();
                for (int u = 0; u < sfs.Length; u++)
                {
                    System.Reflection.MethodBase mb = sfs[u].GetMethod();

                    if (mb.DeclaringType.FullName.IndexOf("System") < 0)
                    {
                        dic["stack" + u] = string.Format("{0}.{1}", mb.DeclaringType.FullName, mb.Name);
                    }
                    else
                    {
                        break;
                    }

                }

                Post(dic);
            });
        }



        public static void SystemLog(Dictionary<string,string> dic)
        {
            Task.Factory.StartNew(() =>
            {
                dic["key"] = ProjectKey;
                dic["type"] = LogType.SystemLog.ToString("D");
                dic["ct"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dic["status"] = LogStatus.Send.ToString("D");

                Post(dic);
            });
        }


        private static void Post(Dictionary<string, string> postData)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://localhost:48258/Hub/PushData");
            request.Method = "post";
            request.ContentType = "application/x-www-form-urlencoded";

            string body = "";
            foreach (KeyValuePair<string, string> item in postData)
            {
                body += item.Key + "=" + System.Web.HttpUtility.UrlEncode( System.Web.HttpUtility.HtmlEncode(item.Value)) + "&";
            }

            body = body.Trim('&');
            //body = System.Web.HttpUtility.UrlEncode(body);
            
            //body = System.Web.HttpUtility.HtmlEncode(body);

            byte[] data = System.Text.Encoding.UTF8.GetBytes(body);
            request.GetRequestStream().Write(data, 0, data.Length);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string result = reader.ReadToEnd();


            reader.Close();
            response.Close();
            request.Abort();
        }

        private static string GetExceptionMessage(Exception ex)
        {
            string msg = "";
            if (ex.InnerException != null)
            {
                msg = GetExceptionMessage(ex);
                msg += "===================";
            }

            return msg += ex.Message;
        }

        
    }


}
