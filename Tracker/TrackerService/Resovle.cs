using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrackerService
{
    public class Resovle
    {

        public void Start()
        {
            //Task.Factory.StartNew(() => {


            //    try
            //    {
            //        EventLog eventlog = new EventLog();

            //        //"Application"应用程序, "Security"安全, "System"系统
            //        eventlog.Log = "Application";
            //        eventlog.EnableRaisingEvents = true;
            //        eventlog.EntryWritten += eventlog_EntryWritten;

            //        //eventlog.EntryWritten

            //        EventLogEntryCollection eventLogEntryCollection = eventlog.Entries;

            //        foreach (EventLogEntry entry in eventLogEntryCollection)
            //        {
            //            //if (entry.EventID == 4624)
            //            //{
            //            //    continue;
            //            //}
            //            string info = string.Empty;

            //            if (@"TaskScheduler" == entry.Source.ToString())
            //            {
            //                info += "类型：" + entry.EntryType.ToString() + ";";
            //                info += "日期" + entry.TimeGenerated.ToLongDateString() + ";";
            //                info += "时间" + entry.TimeGenerated.ToLongTimeString() + ";";
            //                info += "来源" + entry.Source.ToString() + ";";
            //                Console.WriteLine(info);
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
 
            //    }

            //    Thread.Sleep(5000);
            //});


            string logType = ConfigurationManager.AppSettings["LogType"];

            EventLog eventlog = new EventLog();

            //"Application"应用程序, "Security"安全, "System"系统
            eventlog.Log = logType;
            eventlog.EnableRaisingEvents = true;
            eventlog.EntryWritten += eventlog_EntryWritten;

        }

        void eventlog_EntryWritten(object sender, EntryWrittenEventArgs e)
        {          
            string source = e.Entry.Source;

            string logSource = ConfigurationManager.AppSettings["LogSource"];

            if (logSource == source)
            {
                string level = e.Entry.EntryType.ToString();

                List<string> logLevels = new List<string>(ConfigurationManager.AppSettings["LogLevel"].Split(','));

                if (logLevels.Contains(level))
                {
                    string[] logKeys = ConfigurationManager.AppSettings["LogKeys"].Split(',');

                    string message = e.Entry.Message;

                    foreach (string key in logKeys)
                    {
                        if (message.Contains(key))
                        {
                            //post data
                            Dictionary<string, string> dic = new Dictionary<string, string>();

                            string name = Dns.GetHostName();
                            IPHostEntry me = Dns.GetHostEntry(name);
                            string serverIp = "";
                            foreach (IPAddress ip in me.AddressList)
                            {
                                if (!ip.IsIPv6LinkLocal)
                                    serverIp += ip.ToString() + "&";
                            }
                            serverIp = serverIp.Trim('&');

                            dic["sip"] = serverIp;

                            dic["logLevel"] = level;
                            dic["logSource"] = logSource;
                            dic["logMessage"] = message;
                            dic["logType"] = ConfigurationManager.AppSettings["LogType"];

                            break;
                        }
                    }
                }
            }
            
        }

        public void Stop()
        { 
        }

       
    }
}
