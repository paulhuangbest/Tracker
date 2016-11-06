using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class HubUrl
    {
        public string Url { get; set; }
        public string Type { get; set; }
        public DateTime InDate { get; set; }
        public DateTime OutDate { get; set; }
        public string ProjectKey { get; set; }
        public string Status { get;set; }
        public string Body { get; set; }
        public string MQ { get; set; }
    }
}
