using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Interface;


namespace Library.Common
{
    public class WatcherFactory
    {
        public static IWatcherDAL CreateWatcher()
        {
            return new Library.DAL.WatcherDAL();
        }
    }
}
