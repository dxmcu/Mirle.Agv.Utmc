using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class AxisData
    {
        public double Acceleration { get; set; }
        public double Deceleration { get; set; }
        public double Jerk { get; set; }
        public double Velocity { get; set; }
        public double Position { get; set; }

        public AxisData()
        {
            Acceleration = 0;
            Deceleration = 0;
            Jerk = 0;
        }

        public AxisData(double acc, double dec, double jerk)
        {
            Acceleration = acc;
            Deceleration = dec;
            Jerk = jerk;
        }

        public AxisData(double acc, double dec, double jerk, double velocity)
        {
            Acceleration = acc;
            Deceleration = dec;
            Jerk = jerk;
            Velocity = velocity;
        }

        public AxisData(double acc, double dec, double jerk, double velocity, double distance)
        {
            Acceleration = acc;
            Deceleration = dec;
            Jerk = jerk;
            Velocity = velocity;
            Position = distance;
        }
    }
}
