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

        public string Type { get; set; }

        public string Status { get; set; }

        public string Url { get; set; }

        public string IP { get; set; }

        public DateTime CreateTime { get; set; }

        public Dictionary<string, string> Extend { get; set; }
    }
}
