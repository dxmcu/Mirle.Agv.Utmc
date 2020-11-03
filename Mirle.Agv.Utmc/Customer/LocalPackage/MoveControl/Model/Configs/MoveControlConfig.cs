using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    [Serializable]
    public class MoveControlConfig
    {
        public string AGVType { get; set; }

        public Dictionary<EnumMoveControlSafetyType, SafetyData> Safety { get; set; } = new Dictionary<EnumMoveControlSafetyType, SafetyData>();
        public Dictionary<EnumSensorSafetyType, bool> SensorByPass { get; set; } = new Dictionary<EnumSensorSafetyType, bool>();

        public TimeValueConfig TimeValueConfig { get; set; } = new TimeValueConfig();

        public bool VChangeWitchIsokinetic { get; set; } = true;
        public bool UsingSimulateVelocity { get; set; } = true;
        public bool SafetyStopInTurningWithDelaying { get; set; } = true;
        public bool LosePositionSetNullAddressSection { get; set; } = false;

        public double InPositionRange { get; set; } = 10;
        public double SectionWidthRange { get; set; } = 500;
        public double SectionRange { get; set; } = 50;
        public double SectionAllowDeltaTheta { get; set; } = 5;
        public double SectionAllowDeltaThetaMax { get; set; } = 20;

        public MoveControlConfig()
        {
            SafetyData tempSensorData;

            foreach (EnumMoveControlSafetyType item in (EnumMoveControlSafetyType[])Enum.GetValues(typeof(EnumMoveControlSafetyType)))
            {
                tempSensorData = new SafetyData();
                Safety.Add(item, tempSensorData);
            }

            foreach (EnumSensorSafetyType item in (EnumSensorSafetyType[])Enum.GetValues(typeof(EnumSensorSafetyType)))
            {
                tempSensorData = new SafetyData();
                SensorByPass.Add(item, false);
            }
        }
    }
}
