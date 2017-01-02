using Entity;
using Framework.Couchbase;
using Library.Common;
using Library.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.BL
{
    public class WatcherBL
    {
        private IWatcherDAL IWatcher;

        public WatcherBL()
        {
            IWatcher = WatcherFactory.CreateWatcher();
        }

        public List<TotalDTO> GetNormalTotalByDay(DateTime currentDate,string projectKey)
        {
            return IWatcher.GetNormalTotalByDate(DateTime.Now,projectKey);
        }

        public List<TotalDTO> GetExceptionTotalByDay(DateTime currentDate,string projectKey)
        {
            return IWatcher.GetExceptionTotalByDate(DateTime.Now,projectKey);
        }

        public List<TotalDTO> GetSystemTotalByDay(DateTime currentDate,string projectKey)
        {
            return IWatcher.GetSystemTotalByDate(DateTime.Now,projectKey);
        }

        public List<TotalDTO> GetOperateTotalByDay(DateTime currentDate,string projectKey)
        {
            return IWatcher.GetOperateTotalByDate(DateTime.Now,projectKey);
        }

        public List<TotalDTO> GetTypeTotal(DateTime currentDate,string projectKey)
        {
            return IWatcher.GetTotalWithAllType(DateTime.Now, projectKey);
        }

        public List<TotalDTO> GetTypeTotalMonth(DateTime currentDate,string projectKey)
        {
            return IWatcher.GetTotalWithAllTypeMonth(DateTime.Now, projectKey);
        }

        public List<ExceptionLog> SearchExceptionLog(Dictionary<string, string> condition)
        {           

            return IWatcher.SearchExecptionLog(condition);
        }

        public ExceptionLog GetExceptionLog(string logId)
        {
            return IWatcher.GetExceptionLog(logId);
        }

        public List<ExceptionLog> DefaultExceptionLogList(string projectKey)
        {

            return IWatcher.DefaultExecptionLogList(projectKey);
        }

        public int SearchCount(Dictionary<string, string> condition)
        {
            Info info = IWatcher.SearchInfo(condition);

            return info.total;
        }
    }
}
