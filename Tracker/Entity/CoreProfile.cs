using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class CoreProfile
    {
        public string ProjectKey { get; set; }

        public bool Enable { get; set; }

        public int SystemConsumerNum { get; set; }

        public int OperateConsumerNum { get; set; }

        public int ExceptionConsumerNum { get; set; }

        public int NormalConsumerNum { get; set; }
    }
}
