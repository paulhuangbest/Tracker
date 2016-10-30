using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace TrackerClient
{
    public class Client
    {
        public static void ExceptionLog(Exception ex)
        {
            Task.Factory.StartNew(() => {
                //projectkey from web.config
                //log type 
                //create time
                //status
                //exception message


                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("");
                request.Method = "post";
                request.ContentType = "application/x-www-form-urlencoded";


                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["key"] = "WMS";
                dic["type"] = LogType.ExceptionLog.ToString("D");
                dic["ct"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dic["status"] = LogStatus.Send.ToString("D");
                dic["msg"] = GetExceptionMessage(ex);

                string body = "";
                foreach (KeyValuePair<string, string> item in dic)
                {
                    body += item.Key + "=" + item.Value + "&";
                }

                body = System.Web.HttpUtility.HtmlEncode(body);

                byte[] data = System.Text.Encoding.UTF8.GetBytes(body);
                request.GetRequestStream().Write(data, 0, data.Length);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string result = reader.ReadToEnd();


                reader.Close();
                response.Close();
                request.Abort();

            });
            

        }

        public static string TrackUser { get; set; }

        public static void OperateLog(string action)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic["key"] = "WMS";
            dic["type"] = LogType.OperateLog.ToString("D");
            dic["ct"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dic["status"] = LogStatus.Send.ToString("D");
            dic["user"] = TrackUser;
            dic["action"] = action;
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

    public enum LogType
    {
        ExceptionLog = 1,       
        OperateLog = 2,
        SystemLog = 3,
        Normal = 4
    }

    public enum LogStatus
    {
        Send = 1,
        Receive = 2
    }
}
