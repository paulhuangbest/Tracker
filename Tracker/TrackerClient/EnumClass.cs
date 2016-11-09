using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrackerClient
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

    public enum ActionType
    {
        Insert = 1,
        Update = 2,
        Delete = 3,
        Search = 4,
        None = 5
    }
}
