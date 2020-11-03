using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class SafetySensorData
    {
        /// 必備.
        public string Device { get; set; } = "";
        public EnumSafetySensorType Type { get; set; } = EnumSafetySensorType.None;
        public EnumDeviceType DeviceType { get; set; } = EnumDeviceType.None;

        /// 僅BeamSensor擁有.
        public EnumMovingDirection BeamSensorDircetion { get; set; } = EnumMovingDirection.None;

        /// 皆有.
        public List<string> MIPCTagNameOutput { get; set; } = new List<string>();
        public List<EnumSafetyLevel> InputSafetyLevelList { get; set; } = new List<EnumSafetyLevel>();
        public List<bool> ABList { get; set; } = new List<bool>();

        /// 應該只有AreaSensor有.
        public List<string> MIPCTagNmaeInput { get; set; } = new List<string>();
        public Dictionary<EnumMovingDirection, string> AreaSensorChangeDircetion { get; set; } = new Dictionary<EnumMovingDirection, string>();

        /// 後製, 全部皆有.
        public Dictionary<string, int> MIPCInputTagNameToBit = new Dictionary<string, int>();
        public Dictionary<string, bool> MIPCInputTagNameToAB = new Dictionary<string, bool>();

        ///後製, 僅AreaSensor有.
        public Dictionary<EnumMovingDirection, List<float>> AreaSensorChangeDircetionInputIOList = new Dictionary<EnumMovingDirection, List<float>>();
    }
}
