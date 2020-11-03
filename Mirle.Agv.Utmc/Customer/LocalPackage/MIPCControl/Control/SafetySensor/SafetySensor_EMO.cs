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
    public class SafetySensor_EMO : SafetySensor
    {
        public override void Initial(MIPCControlHandler mipcControl, AlarmHandler alarmHandler, SafetySensorData config)
        {
            this.alarmHandler = alarmHandler;
            this.mipcControl = mipcControl;
            Config = config;
            device = MethodInfo.GetCurrentMethod().ReflectedType.Name;
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

            if (Status == 0 && newStatus != 0)
                SendAlarmCode(EnumMIPCControlErrorCode.AreaSensor觸發);

            Status = newStatus;
        }

        public override void ResetAlarm()
        {
            if (Status != 0)
                SendAlarmCode(EnumMIPCControlErrorCode.Bumper觸發);
        }
    }
}
