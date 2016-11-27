using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class TrackLog
    {
        public string LogId { get; set; }

        public string ProjectKey { get; set; }

        public string SubKey { get; set; }

        public string Type { get; set; }

        public string Status { get; set; }

        public string Url { get; set; }

        public string RequestIP { get; set; }

        public string ServerIP { get; set; }

        public DateTime CreateTime { get; set; }

        public string Level { get; set; }

        public string Tag { get; set; }

        public Dictionary<string, string> Extend { get; set; }
    }
}
