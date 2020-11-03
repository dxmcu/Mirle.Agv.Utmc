using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model.Configs
{
    [Serializable]
    public class MainFlowConfig
    {
        public string BatteryLogPath { get; set; }
        public string BatteryBackupLogPath { get; set; }
        public EnumAGVType AGVType { get; set; }
        public bool SimulateMode { get; set; }
    }
}
