using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    [Serializable]
    public class CreateMoveCommandListConfig
    {
        public AxisData Move { get; set; } = new AxisData(500, 500, 1000, 600);
        public AxisData Turn_Moving { get; set; } = new AxisData(5, 5, 10, 5);
        public AxisData Turn { get; set; } = new AxisData(10, 10, 20, 10);
        public AxisData EQ { get; set; } = new AxisData(80, 80, 160, 80, 200);
        public AxisData EMS { get; set; } = new AxisData(1000, 1000, 2000);

        public double LowVelocity_High { get; set; } = 500;
        public double LowVelocity_Low { get; set; } = 250;
        public double SecondCorrectionX { get; set; } = 5;
        public double NormalStopDistance { get; set; } = 50;

        public double ReserveSafetyDistance { get; set; } = 180;

        public double RetryMoveDistance { get; set; } = 100;

        public double AllowAGVAngleRange { get; set; } = 5;

        public double VelocityRange { get; set; } = 15;

        public double VChangeBufferTime { get; set; } = 1000;

        public Dictionary<EnumCommandType, double> SafteyDistance { get; set; } = new Dictionary<EnumCommandType, double>();

        public CreateMoveCommandListConfig()
        {
            SafteyDistance.Add(EnumCommandType.Vchange, 200);
            SafteyDistance.Add(EnumCommandType.Stop, 200);
            SafteyDistance.Add(EnumCommandType.SlowStop, 100);
            SafteyDistance.Add(EnumCommandType.End, 100);
            SafteyDistance.Add(EnumCommandType.Move, 500);
            SafteyDistance.Add(EnumCommandType.STurn, 40);
            SafteyDistance.Add(EnumCommandType.RTurn, 40);
            SafteyDistance.Add(EnumCommandType.SpinTurn, 40);
            SafteyDistance.Add(EnumCommandType.ChangeSection, 200);
        }
    }
}
