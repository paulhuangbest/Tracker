using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Entity;
using Framework.Couchbase;

namespace Library.Interface
{
    public interface IWatcherDAL
    {
        List<TotalDTO> GetTotalWithAllType(DateTime currentDate,string projectKey);

        List<TotalDTO> GetTotalWithAllTypeMonth(DateTime currentDate,string projectKey);

        List<TotalDTO> GetSystemTotalByDate(DateTime currentDate,string projectKey);

        List<TotalDTO> GetExceptionTotalByDate(DateTime currentDate,string projectKey);

        List<TotalDTO> GetOperateTotalByDate(DateTime currentDate,string projectKey);

        List<TotalDTO> GetNormalTotalByDate(DateTime currentDate,string projectKey);

        List<SystemLog> SearchSystemLog(Dictionary<string, string> condition);

        List<ExceptionLog> SearchExecptionLog(Dictionary<string, string> condition);

        List<OperateLog> SearchOperateLog(Dictionary<string, string> condition);

        List<NormalLog> SearchNormalLog(Dictionary<string, string> condition);

        Info SearchInfo(Dictionary<string, string> condition);

        SystemLog GetSystemLog(string logId);

        OperateLog GetOperateLog(string logId);

        ExceptionLog GetExceptionLog(string logId);

        NormalLog GetNormalLog(string logId);

        List<ExceptionLog> DefaultExecptionLogList(string projectKey);

        List<OperateLog> DefaultOperateLogList(string projectKey);

        List<SystemLog> DefaultSystemLogList(string projectKey);

        List<NormalLog> DefaultNormalLogList(string projectKey);
    }
}
