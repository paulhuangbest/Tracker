using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class NormalLog : TrackLog
    {
        public Dictionary<string,string> Content { get; set; }
    }
}
