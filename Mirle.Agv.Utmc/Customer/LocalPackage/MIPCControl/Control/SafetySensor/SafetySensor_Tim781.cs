using Mirle.Agv.INX.Controller;
using Mirle.Agv.INX.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Control
{
    public class SafetySensor_Tim781 : SafetySensor
    {
        public override void Initial(MIPCControlHandler mipcControl, AlarmHandler alarmHandler, SafetySensorData config)
        {
            this.alarmHandler = alarmHandler;
            this.mipcControl = mipcControl;
            Config = config;
            device = MethodInfo.GetCurrentMethod().ReflectedType.Name;
        }

        public override void ChangeMovingDirection(EnumMovingDirection newDirection)
        {
            if (Config.Type == EnumSafetySensorType.AreaSensor && newDirection != movingDirection)
            {
                if (Config.AreaSensorChangeDircetionInputIOList.ContainsKey(newDirection))
                {
                    if (!mipcControl.SendMIPCDataByMIPCTagName(Config.MIPCTagNmaeInput, Config.AreaSensorChangeDircetionInputIOList[newDirection]))
                    {
                        WriteLog(5, "", String.Concat("Device : ", Config.Device, " 切換移動方向 : ", newDirection.ToString(), " 失敗, 進行retry"));

                        if (!mipcControl.SendMIPCDataByMIPCTagName(Config.MIPCTagNmaeInput, Config.AreaSensorChangeDircetionInputIOList[newDirection]))
                            WriteLog(5, "", String.Concat("Device : ", Config.Device, " 切換移動方向 : ", newDirection.ToString(), " retry失敗, ErrorBit on : "));
                        else
                        {
                            WriteLog(5, "", String.Concat("Device : ", Config.Device, " 切換移動方向 : ", newDirection.ToString(), " retry成功"));
                            movingDirection = newDirection;
                        }
                    }
                    else
                        movingDirection = newDirection;
                }
                else
                    WriteLog(3, "", String.Concat("Device : ", Config.Device, " Config 中並未定義移動方向 : ", newDirection.ToString(), " 的切換方式"));
            }
        }

        public override void UpdateStatus()
        {
            uint newStatus = Status;

            foreach (var temp in Config.MIPCInputTagNameToBit)
            {
                if (localData.MIPCData.GetDataByMIPCTagName(temp.Key) == -1)
                    return;
                else if ((localData.MIPCData.GetDataByMIPCTagName(temp.Key) == 0 && Config.MIPCInputTagNameToAB[temp.Key]) ||
                         (localData.MIPCData.GetDataByMIPCTagName(temp.Key) != 0 && !Config.MIPCInputTagNameToAB[temp.Key]))
                    newStatus = newStatus & (maxStatusValue - (uint)(1 << temp.Value));
                else
                    newStatus = newStatus | ((uint)(1 << temp.Value));
            }

            foreach (EnumSafetyLevel level in localData.MIPCData.AllByPassLevel.Keys)
            {
                newStatus = newStatus & (maxStatusValue - (uint)(1 << (int)level));
            }

            uint oldBit;
            uint newBit;

            for (int i = safetyLevelCount - 1; i >= 0; i--)
            {
                oldBit = (uint)(Status & (1 << i));
                newBit = (uint)(newStatus & (1 << i));

                if (oldBit != newBit)
                {
                    WriteLog(7, "", String.Concat(((EnumSafetyLevel)i).ToString(), " Change to ", (newBit != 0 ? "On" : "Off")));

                    if (newBit != 0)
                    {
                        switch ((EnumSafetyLevel)i)
                        {
                            case EnumSafetyLevel.Alarm:
                                SendAlarmCode(EnumMIPCControlErrorCode.AreaSensorAlarm);
                                break;
                            case EnumSafetyLevel.EMO:
                            case EnumSafetyLevel.IPCEMO:
                                SendAlarmCode(EnumMIPCControlErrorCode.AreaSensor觸發);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            
            Status = newStatus;
        }

        public override void ResetAlarm()
        {
            if ((Status & (1 << ((int)EnumSafetyLevel.Alarm))) != 0)
                SendAlarmCode(EnumMIPCControlErrorCode.AreaSensorAlarm);

            if ((Status & (1 << ((int)EnumSafetyLevel.EMO))) != 0 ||
                (Status & (1 << ((int)EnumSafetyLevel.IPCEMO))) != 0)
                SendAlarmCode(EnumMIPCControlErrorCode.AreaSensor觸發);
        }
    }
}
