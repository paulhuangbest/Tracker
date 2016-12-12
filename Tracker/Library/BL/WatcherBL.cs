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
        public List<TotalDTO> GetNormalTotalByDay(DateTime currentDate)
        {
            IWatcherDAL iDal = WatcherFactory.CreateWatcher();

            return iDal.GetNormalTotalByDate(DateTime.Now);
        }
    }
}
