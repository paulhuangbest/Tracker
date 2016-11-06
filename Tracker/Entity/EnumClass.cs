﻿using System;
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
}
