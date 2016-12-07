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
        public DataTable GetTotalWithAllType(DateTime currentDate);

        public DataTable GetSystemTotalByDate(DateTime currentDate);

        public DataTable GetExceptionTotalByDate(DateTime currentDate);

        public DataTable GetOperateTotalByDate(DateTime currentDate);

        public DataTable GetNormalTotalByDate(DateTime currentDate);

        public List<SystemLog> SearchSystemLog(Dictionary<string, string> condition);

        public List<ExceptionLog> SearchExecptionLog(Dictionary<string, string> condition);

        public List<OperateLog> SearchOperateLog(Dictionary<string, string> condition);

        public List<NormalLog> SearchNormalLog(Dictionary<string, string> condition);

    }
}
