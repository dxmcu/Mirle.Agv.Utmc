using Mirle.Agv.INX.Control;
using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Controller
{
    public class LoadUnloadControlHandler
    {
        private ComputeFunction computeFunction = ComputeFunction.Instance;
        private LoggerAgent loggerAgent = LoggerAgent.Instance;
        private LocalData localData = LocalData.Instance;
        private string device = MethodInfo.GetCurrentMethod().ReflectedType.Name;
        private string normalLogName = "LoadUnload";

        private MIPCControlHandler mipcControl;
        private AlarmHandler alarmHandler;
        private LoadUnload loadUnload = null;

        private Thread thread = null;

        public event EventHandler<EnumLoadUnloadComplete> ForkCompleteEvent;

        private EnumControlStatus Status = EnumControlStatus.Ready;

        public LoadUnloadControlHandler(MIPCControlHandler mipcControl, AlarmHandler alarmHandler)
        {
            this.mipcControl = mipcControl;
            this.alarmHandler = alarmHandler;

            //ReadXML();

            // switch(config.Type)
            // {
            // loadUnload = new LoadUnload_AGC();
            // }

            //loadUnload.Initial(mipcControl, config.PIOType);
            //

            thread = new Thread(LoadUnloadThread);
            thread.Start();
        }

        public void CloseLoadUnloadControlHanlder()
        {
            Stopwatch closeTimer = new Stopwatch();
            Status = EnumControlStatus.Closing;

            closeTimer.Restart();

            /// if forkCommand
            ///     {
            ///     Stop
            ///     And Wait
            ///     timeout >> EMO & log
            /// 
            ///     }
            /// else if (fork jog)
            ///    Stop
            ///    And Wait
            ///    timeout >> EMO & log
            /// 

            Status = EnumControlStatus.WaitThreadStop;

            closeTimer.Restart();
            while (thread != null && thread.IsAlive)
            {
                if (closeTimer.ElapsedMilliseconds > 1000/*config.TimeValueConfig.TimeoutValueList[EnumTimeoutValueType.EndTimeoutValue]*/)
                {
                    // log.
                    // abort 
                    break;
                }

                Thread.Sleep(10);
            }

            Status = EnumControlStatus.Closed;
        }

        private void SendAlarmCode(EnumMoveCommandControlErrorCode alarmCode)
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

        public void WriteLog(int logLevel, string carrierId, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            LogFormat logFormat = new LogFormat(normalLogName, logLevel.ToString(), memberName, device, carrierId, message);

            loggerAgent.Log(logFormat.Category, logFormat);

            if (logLevel <= localData.ErrorLevel)
            {
                logFormat = new LogFormat(localData.ErrorLogName, logLevel.ToString(), memberName, device, carrierId, message);
                loggerAgent.Log(logFormat.Category, logFormat);
            }
        }

        public void LoadUnloadThread()
        {
            try
            {
                while (Status != EnumControlStatus.WaitThreadStop)
                {
                    //    if (localData.MoveControlData.MoveCommand != null)
                    //    {
                    //        if (localData.MoveControlData.MoveCommand.CommandStatus == EnumMoveCommandStartStatus.Start)
                    //        {
                    //            ExecuteCommandList();
                    //            SensorSafety();
                    //        }
                    //    }

                    //    IntervalSleepAndPollingAllData(true);
                }
            }
            catch (Exception ex)
            {
                SendAlarmCode(EnumMoveCommandControlErrorCode.MoveControl主Thread跳Exception);
                WriteLog(1, "", String.Concat("Exception : ", ex.ToString()));
            }
        }
    }
}
