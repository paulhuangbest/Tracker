using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class OperateLog:TrackLog
    {
        public string User { get; set; }
        public string Action { get; set; }
        public string ActionType { get; set; }
        public string Section { get; set; }

        public List<string> Stack { get; set; }
    }
}
