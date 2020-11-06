using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Alarms
{
    public class AgvcAlarmArgs : EventArgs
    {
        public int ErrorCode { get; set; } = 0;
        public bool IsAlarm { get; set; } = false;
    }
}
