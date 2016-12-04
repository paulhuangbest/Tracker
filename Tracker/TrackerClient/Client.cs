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

        public static string SubKey { get; set; }


        public static void ExceptionLog(Exception ex)
        {
            ExceptionLog("", ex, null);
        }

        public static void ExceptionLog(string user,Exception ex)
        {
            ExceptionLog(user, ex, null);
        }

        public static void ExceptionLog(string user,Exception ex,Dictionary<string,string> extend)
        {
            string url = HttpContext.Current.Request.Url.OriginalString;

            string userHostAddress = HttpContext.Current.Request.UserHostAddress;

            if (string.IsNullOrEmpty(userHostAddress))
            {
                userHostAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            

            Task.Factory.StartNew(() => {

                string name = Dns.GetHostName();
                IPHostEntry me = Dns.GetHostEntry(name);
                string serverIp = "";
                foreach (IPAddress ip in me.AddressList)
                {
                    if (!ip.IsIPv6LinkLocal)
                        serverIp += ip.ToString() + "&";
                }
                serverIp = serverIp.Trim('&');

                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["key"] = ProjectKey;
                dic["subkey"] = SubKey;
                dic["type"] = LogType.ExceptionLog.ToString("D");
                dic["ct"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                dic["status"] = LogStatus.Send.ToString("D");
                dic["msg"] = GetExceptionMessage(ex);
                dic["url"] = url;
                dic["ip"] = userHostAddress;
                dic["sip"] = serverIp;
                dic["user"] = user;

                if (extend != null)
                {
                    foreach (KeyValuePair<string, string> item in extend)
                    {
                        dic["extend_" + item.Key] = item.Value;
                    }
                }

                Post(dic);    

            });
            

        }


        public static void OperateLog(string trackUser, string action)
        {
            OperateLog(trackUser, action, ActionType.None, "", null);
        }

        public static void OperateLog(string trackUser, string action, Dictionary<string,string> extend)
        {
            OperateLog(trackUser, action, ActionType.None, "", extend);
        }

        public static void OperateLog(string trackUser, string action, string section)
        {
            OperateLog(trackUser, action, ActionType.None, section, null);
        }

        public static void OperateLog(string trackUser, string action, string section,Dictionary<string,string> extend)
        {
            OperateLog(trackUser, action, ActionType.None, section, extend);
        }

        public static void OperateLog(string trackUser , string action , ActionType actionType ,string section,Dictionary<string,string> extend)
        {
            string url = HttpContext.Current.Request.Url.OriginalString;

            string userHostAddress = HttpContext.Current.Request.UserHostAddress;

            if (string.IsNullOrEmpty(userHostAddress))
            {
                userHostAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            System.Diagnostics.StackFrame[] sfs = st.GetFrames();

            Dictionary<string, string> dic2 = new Dictionary<string, string>();

            if (extend != null)
            {
                foreach (KeyValuePair<string, string> item in extend)
                {
                    dic2["extend_" + item.Key] = item.Value;
                }
            }
                

            for (int u = 0; u < sfs.Length; u++)
            {
                System.Reflection.MethodBase mb = sfs[u].GetMethod();

                //if (mb.Name.IndexOf("OperateLog") >= 0)
                //{
                //    continue;
                //}

                if (mb.DeclaringType.FullName.IndexOf("System") < 0)
                {
                    dic2["stack" + u] = string.Format("{0}.{1}", mb.DeclaringType.FullName, mb.Name);
                }
                else
                {
                    break;
                }

            }

            Task.Factory.StartNew(() =>
            {
                string name = Dns.GetHostName();
                IPHostEntry me = Dns.GetHostEntry(name);
                string serverIp = "";
                foreach (IPAddress ip in me.AddressList)
                {
                    if (!ip.IsIPv6LinkLocal)
                        serverIp += ip.ToString() + "&";
                }
                serverIp = serverIp.Trim('&');


                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["key"] = ProjectKey;
                dic["subkey"] = SubKey;
                dic["type"] = LogType.OperateLog.ToString("D");
                dic["ct"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                dic["status"] = LogStatus.Send.ToString("D");
                dic["user"] = trackUser;
                dic["action"] = action;
                dic["actionType"] = (actionType == ActionType.None) ? "" : actionType.ToString();
                dic["section"] = section;
                dic["url"] = url;
                dic["ip"] = userHostAddress;
                dic["sip"] = serverIp;

                foreach (KeyValuePair<string, string> item in dic2)
                {
                    dic[item.Key] = item.Value;
                }

                Post(dic);
            });
        }



        public static void SystemLog(Dictionary<string,string> dic)
        {
            Task.Factory.StartNew(() =>
            {
                dic["key"] = ProjectKey;
                dic["subkey"] = SubKey;
                dic["type"] = LogType.SystemLog.ToString("D");
                dic["ct"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                dic["status"] = LogStatus.Send.ToString("D");

                Post(dic);
            });
        }

        
        public static void NormalLog(Dictionary<string,string> dic)
        {
            Task.Factory.StartNew(() =>
            {
                dic["key"] = ProjectKey;
                dic["subkey"] = SubKey;
                dic["type"] = LogType.Normal.ToString("D");
                dic["ct"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
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
