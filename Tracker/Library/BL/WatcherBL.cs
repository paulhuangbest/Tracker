using Entity;
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

        public List<TotalDTO> GetNormalTotalByDay(DateTime currentDate)
        {
            return IWatcher.GetNormalTotalByDate(DateTime.Now);
        }

        public List<TotalDTO> GetExceptionTotalByDay(DateTime currentDate)
        {
            return IWatcher.GetExceptionTotalByDate(DateTime.Now);
        }

        public List<TotalDTO> GetSystemTotalByDay(DateTime currentDate)
        {
            return IWatcher.GetSystemTotalByDate(DateTime.Now);
        }

        public List<TotalDTO> GetOperateTotalByDay(DateTime currentDate)
        {
            return IWatcher.GetOperateTotalByDate(DateTime.Now);
        }

        public List<TotalDTO> GetTypeTotal(DateTime currentDate)
        {
            return IWatcher.GetTotalWithAllType(DateTime.Now);
        }

        public List<TotalDTO> GetTypeTotalMonth(DateTime currentDate)
        {
            return IWatcher.GetTotalWithAllTypeMonth(DateTime.Now);
        }
    }
}
