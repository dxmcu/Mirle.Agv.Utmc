using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    [Serializable]
    public class BatteryLog
    {
        public long MoveDistanceTotalM { get; set; } = 0;
        public int LoadUnloadCount { get; set; } = 0;
        public int ChargeCount { get; set; } = 0;
        public string ResetTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff");
        public int InitialSoc { get; set; } = 70;
    }
}
