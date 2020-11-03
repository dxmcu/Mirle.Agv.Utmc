using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  Mirle.Agv.INX.Model
{
    public class Command
    {
        public MapAGVPosition TriggerAGVPosition { get; set; }
        public MapAGVPosition EndAGVPosition { get; set; }

        public double TriggerEncoder { get; set; }
        public double SafetyDistance { get; set; }
        public EnumCommandType CmdType { get; set; }
        public string TurnType { get; set; }
        public double Velocity { get; set; }

        public double EndEncoder { get; set; }
        public double MovingAngle { get; set; }
        public double NewMovingAngle { get; set; }
        public int ReserveNumber { get; set; } = -1;
        public object Type { get; set; }
        public double NowVelocity { get; set; }

        public Command()
        {
            CmdType = EnumCommandType.Stop;
        }
    }
}

