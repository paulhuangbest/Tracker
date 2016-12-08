using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Entity;

namespace Library.Interface
{
    public interface IWatcherDAL
    {
        DataTable GetTotalWithAllType(DateTime currentDate);

        DataTable GetSystemTotalByDate(DateTime currentDate);

        DataTable GetExceptionTotalByDate(DateTime currentDate);

        DataTable GetOperateTotalByDate(DateTime currentDate);

        DataTable GetNormalTotalByDate(DateTime currentDate);

        List<SystemLog> SearchSystemLog(Dictionary<string, string> condition);

        List<ExceptionLog> SearchExecptionLog(Dictionary<string, string> condition);

        List<OperateLog> SearchOperateLog(Dictionary<string, string> condition);

        List<NormalLog> SearchNormalLog(Dictionary<string, string> condition);

        SystemLog GetSystemLog(string logId);

        OperateLog GetOperateLog(string logId);

        ExceptionLog GetExceptionLog(string logId);

        NormalLog GetNormalLog(string logId);

    }
}
