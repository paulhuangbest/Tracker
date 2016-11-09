using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class ExceptionLog : TrackLog
    {
        public string ExceptionMessage { get; set; }

        public string User { get; set; }
    }
}
