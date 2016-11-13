using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class CoreProfile
    {
        public string ProfileKey { get; set; }

        public string ProjectKey { get; set; }

        public bool Enable { get; set; }

        public int SystemConsumerNum { get; set; }

        public int OperateConsumerNum { get; set; }

        public int ExceptionConsumerNum { get; set; }

        public int NormalConsumerNum { get; set; }        

        public DateTime ModifyTime { get; set; }

        public string MQServer { get; set; }
    }

    [Serializable]
    public class Contain
    {
        public Task task { get; set; }
        public CancellationTokenSource tokenSource { get; set; }
        public string taskKey { get; set; }
        public string projectKey { get; set; }
    }
}
