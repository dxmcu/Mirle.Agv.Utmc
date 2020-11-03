using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class AxisFeedbackData
    {
        public uint Count { get; set; } = 0;
        public double Velocity { get; set; } = 0;
        public double ErrorPosition { get; set; } = 0;
        public double Torque { get; set; } = 0;
        public string Mode { get; set; } = "";
        public double Position { get; set; } = 0;
        public double DA { get; set; } = 0;
        public double QA { get; set; } = 0;
        public double V { get; set; } = 0;
        public string ErrorCode { get; set; } = "";
        public double EC { get; set; } = 0;
        public double MF { get; set; } = 0;
        public double GetwayError { get; set; } = 0;
        public EnumAxisMoveStatus AxisMoveStaus { get; set; } = EnumAxisMoveStatus.Stop;
        public EnumAxisServoOnOff AxisServoOnOff { get; set; } = EnumAxisServoOnOff.ServoOff;
        public EnumAxisStatus AxisStatus { get; set; } = EnumAxisStatus.Normal;
        public DateTime GetDataTime { get; set; } = DateTime.Now;
    }
}
