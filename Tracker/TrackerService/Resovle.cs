using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrackerService
{
    public class Resovle
    {

        public void Start()
        {
            Task.Factory.StartNew(() => {


                try
                {
                    EventLog eventlog = new EventLog();

                    //"Application"应用程序, "Security"安全, "System"系统
                    eventlog.Log = "Application";


                    //eventlog.EntryWritten

                    //EventLogEntryCollection eventLogEntryCollection = eventlog.Entries;

                    //foreach (EventLogEntry entry in eventLogEntryCollection)
                    //{
                    //    //if (entry.EventID == 4624)
                    //    //{
                    //    //    continue;
                    //    //}
                    //    string info = string.Empty;

                    //    if (@"TaskScheduler" == entry.Source.ToString())
                    //    {
                    //        info += "类型：" + entry.EntryType.ToString() + ";";
                    //        info += "日期" + entry.TimeGenerated.ToLongDateString() + ";";
                    //        info += "时间" + entry.TimeGenerated.ToLongTimeString() + ";";
                    //        info += "来源" + entry.Source.ToString() + ";";
                    //        Console.WriteLine(info);
                    //    }
                    //}
                }
                catch (Exception ex)
                {
 
                }

                Thread.Sleep(5000);
            });
                
            
        }

        public void Stop()
        { 
        }
    }
}
