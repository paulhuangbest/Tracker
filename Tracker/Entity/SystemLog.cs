using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class SystemLog : TrackLog
    {
        public string Method { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }

        public string Interval { get; set; }
        public string QueryString { get; set; }

        public List<string> PostArgument { get; set; }
    }
}
