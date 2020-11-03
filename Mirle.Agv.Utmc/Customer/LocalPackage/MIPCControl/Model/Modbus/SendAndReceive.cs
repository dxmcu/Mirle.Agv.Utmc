using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class SendAndReceive
    {
        public ModbusData Send { get; set; }
        public ModbusData Receive { get; set; }
        public EnumSendAndRecieve Result { get; set; } = EnumSendAndRecieve.None;
        public int PollingGroup { get; set; } = -1;
        public bool IsMotionCommand { get; set; } = false;

        public double ScanTime { get; set; } = 0;

        public DateTime Time { get; set; }
        public DateTime AddTime { get; set; }
        public bool IsHearBeat { get; set; } = false;
    }
}
