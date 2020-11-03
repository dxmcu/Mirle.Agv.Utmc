using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class SimulateData
    {
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public double DeltaTime { get; set; }
        public double StartPosition { get; set; }
        public double EndPosition { get; set; }
        public double StartAcc { get; set; }
        public double EndAcc { get; set; }
        public double StartVelocity { get; set; }
        public double EndVelocity { get; set; }
        public double DeltaVelocity { get; set; }
        public double DeltaPosition { get; set; }
        public EnumSimulateVelocityType Type { get; set; }
        public double Jerk { get; set; }

        public void SetSimulateData_AccJerkDown(double startVelocity, double startAcc, double jerk, double startTime)
        {
            StartTime = startTime;
            DeltaTime = startAcc / jerk;
            EndTime = StartTime + DeltaTime;
            StartAcc = startAcc;
            EndAcc = 0;
            StartVelocity = startVelocity;
            DeltaVelocity = startAcc * startAcc / 2 / jerk;
            EndVelocity = StartVelocity + DeltaVelocity;
            Type = EnumSimulateVelocityType.AccJerkDown;
            Jerk = jerk;
        }

        public void SetSimulateData_DecJerkDown(double startVelocity, double startDec, double jerk, double startTime)
        {
            StartTime = startTime;
            DeltaTime = startDec / jerk;
            EndTime = StartTime + DeltaTime;
            StartAcc = startDec;
            EndAcc = 0;
            StartVelocity = startVelocity;
            DeltaVelocity = startDec * startDec / 2 / jerk;
            EndVelocity = StartVelocity - DeltaVelocity;
            Type = EnumSimulateVelocityType.DecJerkDown;
            Jerk = jerk;
        }

        public void SetSimulateData_AccJerkUp(double startVelocity, double endAcc, double jerk, double startTime)
        {
            StartTime = startTime;
            DeltaTime = endAcc / jerk;
            EndTime = StartTime + DeltaTime;
            StartAcc = 0;
            EndAcc = endAcc;
            StartVelocity = startVelocity;
            DeltaVelocity = endAcc * endAcc / 2 / jerk;
            EndVelocity = StartVelocity + DeltaVelocity;
            Type = EnumSimulateVelocityType.AccJerkUp;
            Jerk = jerk;
        }

        public void SetSimulateData_DecJerkUp(double startVelocity, double endDec, double jerk, double startTime)
        {
            StartTime = startTime;
            DeltaTime = endDec / jerk;
            EndTime = StartTime + DeltaTime;
            StartAcc = 0;
            EndAcc = endDec;
            StartVelocity = startVelocity;
            DeltaVelocity = endDec * endDec / 2 / jerk;
            EndVelocity = StartVelocity - DeltaVelocity;
            Type = EnumSimulateVelocityType.DecJerkUp;
            Jerk = jerk;
        }

        public void SetSimulateData_Accing(SimulateData simulateData, AxisData axis)
        {
            StartTime = simulateData.EndTime;
            StartAcc = simulateData.EndAcc;
            EndAcc = simulateData.EndAcc;

            StartVelocity = simulateData.EndVelocity;
            EndVelocity = axis.Velocity - (axis.Acceleration * axis.Acceleration / 2 / axis.Jerk);
            DeltaVelocity = EndVelocity - StartVelocity;

            DeltaTime = DeltaVelocity / StartAcc;
            EndTime = StartTime + DeltaTime;
            Type = EnumSimulateVelocityType.Accing;
            Jerk = axis.Jerk;
        }

        public void SetSimulateData_Decing(SimulateData simulateData, AxisData axis)
        {
            StartTime = simulateData.EndTime;
            StartAcc = simulateData.EndAcc;
            EndAcc = simulateData.EndAcc;

            StartVelocity = simulateData.EndVelocity;
            EndVelocity = axis.Velocity + (axis.Deceleration * axis.Deceleration / 2 / axis.Jerk);
            DeltaVelocity = StartVelocity - EndVelocity;

            DeltaTime = DeltaVelocity / StartAcc;
            EndTime = StartTime + DeltaTime;
            Type = EnumSimulateVelocityType.Decing;
            Jerk = axis.Jerk;
        }

        public void SetSimulateData_Isokinetic(double startVelcoity, double startTime)
        {
            StartTime = startTime;
            StartVelocity = startVelcoity;
            StartAcc = 0;
            EndAcc = 0;
            Jerk = 0;
            DeltaTime = 1;
            EndTime = StartTime + DeltaTime;
            DeltaVelocity = 0;
            EndVelocity = startVelcoity;
            Type = EnumSimulateVelocityType.Isokinetic;
        }

    }
}
