using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class TimeValueConfig
    {
        public Dictionary<EnumTimeoutValueType, int> TimeoutValueList { get; set; } = new Dictionary<EnumTimeoutValueType, int>();
        public Dictionary<EnumIntervalTimeType, int> IntervalTimeList { get; set; } = new Dictionary<EnumIntervalTimeType, int>();
        public Dictionary<EnumDelayTimeType, int> DelayTimeList { get; set; } = new Dictionary<EnumDelayTimeType, int>();

        public TimeValueConfig()
        {
            foreach (EnumTimeoutValueType item in (EnumTimeoutValueType[])Enum.GetValues(typeof(EnumTimeoutValueType)))
            {
                TimeoutValueList.Add(item, 0);
            }

            foreach (EnumIntervalTimeType item in (EnumIntervalTimeType[])Enum.GetValues(typeof(EnumIntervalTimeType)))
            {
                IntervalTimeList.Add(item, 0);
            }

            foreach (EnumDelayTimeType item in (EnumDelayTimeType[])Enum.GetValues(typeof(EnumDelayTimeType)))
            {
                DelayTimeList.Add(item, 0);
            }

            IntervalTimeList[EnumIntervalTimeType.ThreadSleepTime] = 30;
            IntervalTimeList[EnumIntervalTimeType.CSVLogInterval] = 50;
            IntervalTimeList[EnumIntervalTimeType.ManualFindSectionInterval] = 1000;
            IntervalTimeList[EnumIntervalTimeType.SetPositionInterval] = 120;

            TimeoutValueList[EnumTimeoutValueType.EnableTimeoutValue] = 8000;
            TimeoutValueList[EnumTimeoutValueType.TurnWheelTimeoutValue] = 3000;
            TimeoutValueList[EnumTimeoutValueType.DisableTimeoutValue] = 4000;
            TimeoutValueList[EnumTimeoutValueType.SlowStopTimeoutValue] = 10000;
            TimeoutValueList[EnumTimeoutValueType.EndTimeoutValue] = 20000;
            TimeoutValueList[EnumTimeoutValueType.OverrideTimeoutValue] = 4000;
            TimeoutValueList[EnumTimeoutValueType.RTurnFlowTimeoutValue] = 60000;
            TimeoutValueList[EnumTimeoutValueType.SpinTurnFlowTimeoutValue] = 20000;
            TimeoutValueList[EnumTimeoutValueType.CloseProgameTimeoutValue] = 10000;
            TimeoutValueList[EnumTimeoutValueType.BeamSensorStopTimeout] = 300000;

            DelayTimeList[EnumDelayTimeType.CommandStartDelayTime] = 2000;
            DelayTimeList[EnumDelayTimeType.OntimeReviseAlarmDelayTime] = 500;
            DelayTimeList[EnumDelayTimeType.SafetySensorStartDelayTime] = 2000;
            DelayTimeList[EnumDelayTimeType.Local_PauseStartDelayTime] = 2000;
        }
    }
}
