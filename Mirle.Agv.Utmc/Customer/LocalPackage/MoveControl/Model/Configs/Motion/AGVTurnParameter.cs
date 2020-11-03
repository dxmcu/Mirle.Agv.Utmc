using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class AGVTurnParameter
    {
        public string TurnName { get; set; }
        public EnumTurnType Type { get; set; } = EnumTurnType.None;
        public double Velocity { get; set; }
        public double R { get; set; }
        public double VChangeSafetyDistance { get; set; } //降速完到入彎點的距離.
        public double SafetyVelocityRange { get; set; }
        public double SectionAngleChangeMin { get; set; }
        public double SectionAngleChangeMax { get; set; }
        public double RTurnDistanceRange { get; set; }
    }
}
