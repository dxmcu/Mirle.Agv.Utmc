using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Model;
using Mirle.Agv.INX.Model.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Controller
{
    public class Driver
    {
        protected LocalData localData = LocalData.Instance;
        protected ComputeFunction computeFunction = ComputeFunction.Instance;
        protected bool resetAlarm = false;
        protected Thread pollingThread;

        protected string normalLogName = "";
        protected string device = "";

        protected LoggerAgent loggerAgent = LoggerAgent.Instance;
        protected AlarmHandler alarmHandler;

        protected EnumControlStatus status = EnumControlStatus.NotInitial;
        public bool PoolingOnOff { get; set; } = true;

        public System.Diagnostics.Stopwatch pollingTimer = new System.Diagnostics.Stopwatch();

        public EnumControlStatus Status
        {
            get
            {
                if (resetAlarm)
                    return EnumControlStatus.ResetAlarm;

                return status;
            }
        }

        #region WriteLog & SendAlarmCode.
        protected void SendAlarmCode(EnumMoveCommandControlErrorCode alarmCode)
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
        #endregion

        //virtual public void InitailDriver()

        virtual public void ConnectDriver()
        {
        }

        virtual public void CloseDriver()
        {
        }

        virtual public void ResetAlarm()
        {
        }

        virtual public void ResendAlarm()
        {
        }
    }
}
