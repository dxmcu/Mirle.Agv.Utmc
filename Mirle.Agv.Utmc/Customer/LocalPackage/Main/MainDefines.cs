using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv
{
    public enum EnumControlStatus
    {
        NotInitial = 403,
        Initial = 302,
        Ready = 100,
        NotReady = 300,
        Error = 301,
        ResetAlarm = 200,
        Closing = 400,
        WaitThreadStop = 401,
        Closed = 402
    }

    public enum EnumAGVType
    {
        Demo = 0,
        AGC = 1,
        UMTC = 2,
    }

    public enum EnumSectionAction
    {
        None,
        Idle,
        NotGetReserve,
        GetReserve
    }

    public enum EnumCommandStatus
    {
        Idle = 100,
        Initial = 300,
        Busy = 200,
        Reporting = 300
    }

    public enum EnumStageDirection
    {
        None,
        Left,
        Right
    }

    public enum EnumAutoState
    {
        Auto,
        Manual,
        PreAuto
    }

    public enum EnumLoginLevel
    {
        User = 0,
        Engineer = 1,
        Admin = 2,
        MirleAdmin = 3
    }

    public enum EnumAlarmLevel
    {
        Warn,
        Alarm
    }


    public enum EnumBeamDirection
    {
        Front,
        Back,
        Left,
        Right
    }
}
