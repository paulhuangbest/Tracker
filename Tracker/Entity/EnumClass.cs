using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class EnumClass
    {
    }

    public enum LogType
    {
        ExceptionLog = 1,
        OperateLog = 2,
        SystemLog = 3,
        Normal = 4
    }

    public enum LogStatus
    {
        Send = 1,
        Hub = 2,
        Receive = 3
    }

    public enum CostLevel
    {
        Normal = 1,
        Warn = 2,
        Block = 3
    }

    public enum ExceptionLevel
    {
        Warn = 1,
        Block = 2
    }
}
