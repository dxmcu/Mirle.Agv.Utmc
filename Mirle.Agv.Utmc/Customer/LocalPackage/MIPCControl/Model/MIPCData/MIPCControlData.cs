using Mirle.Agv.INX.Configs;
using Mirle.Agv.INX.Model.Configs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class MIPCControlData
    {
        public MIPCConfig Config { get; set; } 
        public float[] MIPCTestArray { get; set; } = new float[10];

        public Dictionary<string, MIPCData> AllDataByMIPCTagName { get; set; }
        public Dictionary<string, MIPCData> AllDataByIPCTagName { get; set; }
        public bool Start
        {
            get
            {
                return StartButton_Front || StartButton_Back;
            }
        }

        public bool StartButton_Front { get; set; } = false;
        public bool StartButton_Back { get; set; } = false;

        public float GetDataByMIPCTagName(string tagName)
        {
            if (AllDataByMIPCTagName.ContainsKey(tagName) && AllDataByMIPCTagName[tagName].Object != null)
                return (float)AllDataByMIPCTagName[tagName].Object;
            else
                return -1;
        }

        public float GetDataByIPCTagName(string tagName)
        {
            if (AllDataByIPCTagName.ContainsKey(tagName) && AllDataByIPCTagName[tagName].Object != null)
                return (float)AllDataByIPCTagName[tagName].Object;
            else
                return -1;
        }

        public bool charging { get; set; } = false;

        private bool chargingOnDelay = false;
        private Stopwatch chargingDelayTimer = new Stopwatch();
        private double chargingDelayTime = 1000;

        public bool Charging
        {
            get
            {
                return charging;
            }

            set
            {
                charging = value;
            }
        }

        public int MotionAlarmCount { get; set; } = 0;

        public Dictionary<EnumSafetyLevel, int> AllByPassLevel { get; set; } = new Dictionary<EnumSafetyLevel, int>();

        public EnumSafetyLevel safetySensorStatus { get; set; } = EnumSafetyLevel.Normal;      // 原始資料,不包含Delay內容.
        private Stopwatch areaSensorTimer = new Stopwatch();                                                // Delay用Timer.
        private bool safetySensorDelaying = false;

        private EnumSafetyLevel safetySensorStopType = EnumSafetyLevel.Normal;

        public EnumSafetyLevel SafetySensorStatus                                                      // 移動控制取得資料.
        {
            get
            {
                if (safetySensorDelaying)
                {
                    if (areaSensorTimer.ElapsedMilliseconds > LocalData.Instance.MoveControlData.MoveControlConfig.TimeValueConfig.DelayTimeList[EnumDelayTimeType.SafetySensorStartDelayTime])
                    {
                        safetySensorDelaying = false;
                        areaSensorTimer.Stop();
                        return safetySensorStatus;
                    }
                    else
                        return safetySensorStopType;
                }
                else
                    return safetySensorStatus;
            }

            set
            {
                if (value != safetySensorStatus)
                {
                    if ((int)value >= (int)EnumSafetyLevel.SlowStop)
                    {   // 變成Stop.
                        safetySensorDelaying = false;
                        areaSensorTimer.Stop();
                    }
                    else if ((int)safetySensorStatus >= (int)EnumSafetyLevel.SlowStop && (int)value < (int)EnumSafetyLevel.SlowStop)
                    {
                        safetySensorStopType = safetySensorStatus;
                        areaSensorTimer.Restart();
                        safetySensorDelaying = true;
                    }

                    safetySensorStatus = value;
                }
            }
        }

        public EnumMovingDirection MoveControlDirection { get; set; } = EnumMovingDirection.None;
        public bool BypassFront { get; set; } = false;
        public bool BypassBack { get; set; } = false;
        public bool BypassLeft { get; set; } = false;
        public bool BypassRight { get; set; } = false;

        public EnumBuzzerType MoveControlBuzzerType { get; set; } = EnumBuzzerType.None;
        public EnumDirectionLight MoveControlDirectionLight { get; set; } = EnumDirectionLight.None;
    }
}
