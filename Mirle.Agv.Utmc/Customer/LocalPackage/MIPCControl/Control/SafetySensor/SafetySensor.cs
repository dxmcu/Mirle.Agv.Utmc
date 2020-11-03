using Mirle.Agv.INX.Controller;
using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Control
{
    public class SafetySensor
    {
        protected ComputeFunction computeFunction = ComputeFunction.Instance;
        protected LoggerAgent loggerAgent = LoggerAgent.Instance;
        protected LocalData localData = LocalData.Instance;
        protected MIPCControlHandler mipcControl = null;

        public EnumSafetySensorType Type { get; set; } = EnumSafetySensorType.None;
        protected EnumMovingDirection movingDirection = EnumMovingDirection.None;

        public uint Status { get; set; } = 0;
        /// 0b 0 0 0 0 0 0 0 0 0(9bit)
        ///  Error / Warn / EMO / IPCEMO / EMS / SlowStop / 降速-低 / 降速-高 / Normal

        protected int safetyLevelCount = Enum.GetNames(typeof(EnumSafetyLevel)).Count();
        protected uint maxStatusValue = ((uint)Math.Pow(2, Enum.GetNames(typeof(EnumSafetyLevel)).Count()) - 1);

        public bool ByPassAlarm { get; set; } = false;
        public bool ByPassStatus { get; set; } = false;
        protected string normalLogName = "SafetySensor";
        protected string device = "";

        protected AlarmHandler alarmHandler;

        public SafetySensorData Config { get; set; } = null;

        protected void WriteLog(int logLevel, string carrierId, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            LogFormat logFormat = new LogFormat(normalLogName, logLevel.ToString(), memberName, device, carrierId, message);

            loggerAgent.Log(logFormat.Category, logFormat);

            if (logLevel <= localData.ErrorLevel)
            {
                logFormat = new LogFormat(localData.ErrorLogName, logLevel.ToString(), memberName, device, carrierId, message);
                loggerAgent.Log(logFormat.Category, logFormat);
            }
        }

        protected void SendAlarmCode(EnumMIPCControlErrorCode alarmCode)
        {
            try
            {
                WriteLog(3, "", String.Concat("SetAlarm, alarmCode : ", ((int)alarmCode).ToString(), ", Message : ", alarmCode.ToString()));
                alarmHandler.SetAlarm((int)alarmCode);
            }
            catch (Exception ex)
            {
                WriteLog(1, "", "SetAlarm失敗, Excption : " + ex.ToString());
            }
        }

        public virtual void Initial(MIPCControlHandler mipcControl, AlarmHandler alarmHandler, SafetySensorData config)
        {

        }


        public virtual void ChangeMovingDirection(EnumMovingDirection newDirection)
        {
        }

        public virtual void UpdateStatus()
        {

        }

        public void ByPassBySafetyLevel(EnumSafetyLevel byPassLevel)
        {

        }

        public virtual void ResetAlarm()
        {

        }
    }
}
