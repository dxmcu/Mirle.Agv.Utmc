using Mirle.Agv.Utmc.Model;
using Mirle.Agv.Utmc.Model.TransferSteps;
using Mirle.Tools;
using PSDriver.PSDriver;
using SimpleWifi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Mirle.Agv.Utmc.Controller
{
    public class LocalPackage
    {
        public MirleLogger mirleLogger = MirleLogger.Instance;
        public Vehicle Vehicle { get; set; } = Vehicle.Instance;

        public Dictionary<string, PSMessageXClass> psMessageMap = new Dictionary<string, PSMessageXClass>();
        public string LocalLogMsg { get; set; } = "";
        public RobotCommand RobotCommand { get; set; }

        private Thread thdWatchPosition;
        public bool IsWatchPositionPause { get; private set; } = false;

        private Thread thdWatchBatteryState;
        public bool IsWatchBatteryStatusPause { get; private set; } = false;

        public ConcurrentQueue<PSTransactionXClass> SecondarySendQueue { get; set; } = new ConcurrentQueue<PSTransactionXClass>();
        public ConcurrentQueue<PositionArgs> ReceivePositionArgsQueue { get; set; } = new ConcurrentQueue<PositionArgs>();

        public event EventHandler<string> ImportantPspLog;
        public event EventHandler<string> OnStatusChangeReportEvent;
        public event EventHandler<EnumAutoState> OnModeChangeEvent;
        public event EventHandler<CarrierSlotStatus> OnUpdateSlotStatusEvent;
        public event EventHandler<int> OnAlarmCodeSetEvent;
        public event EventHandler<int> OnAlarmCodeResetEvent;
        public event EventHandler OnAlarmCodeAllResetEvent;
        public event EventHandler<double> OnBatteryPercentageChangeEvent;
        public event EventHandler<EnumRobotEndType> OnRobotEndEvent;

        public LocalPackage()
        {
            InitialThreads();
        }
        private void InitialThreads()
        {
        }

        #region PrimaryReceived

        private void SetVehicleManual()
        {
            OnModeChangeEvent?.Invoke(this, EnumAutoState.Manual);
            ImportantPspLog?.Invoke(this, $"ModeChange : Manual");
        }

        private void SetVehicleAuto()
        {
            OnModeChangeEvent?.Invoke(this, EnumAutoState.Auto);
            ImportantPspLog?.Invoke(this, $"ModeChange : Auto");
        }

        public void SetVehicleAutoScenario()
        {
            //AllAgvlStatusReportRequest();
            //SpinWait.SpinUntil(() => false, 50);

            //SendPositionReportRequest();
            //SpinWait.SpinUntil(() => false, 50);

            //SendBatteryStatusRequest();
            //SpinWait.SpinUntil(() => false, 50);
        }

    

        public void RequestVehicleToManual()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }       

        #endregion
      

        #region PsWrapper

        public void ReportAgvcDisConnect()
        {
            
        }

        #endregion

        #region Battery Control

      

        #endregion

        #region Move Control
       

        #endregion

        #region Logger

        public void AppendPspLogMsg(string msg)
        {
            try
            {
                LocalLogMsg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "\t", msg, "\r\n", LocalLogMsg);

                if (LocalLogMsg.Length > 65535)
                {
                    LocalLogMsg = LocalLogMsg.Substring(65535);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void LogException(string classMethodName, string exMsg)
        {
            mirleLogger.Log(new LogFormat("Error", "5", classMethodName, "Device", "CarrierID", exMsg));
        }

        public void LogPsWrapper(string msg)
        {
            mirleLogger.Log(new LogFormat("PsWrapper", "5", "AsePackage", Vehicle.AgvcConnectorConfig.ClientName, "CarrierID", msg));
            AppendPspLogMsg(msg);
        }

        #endregion

    }
}
