using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Model;
using Mirle.Agv.INX.Model.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Controller
{
    public class MainFlowHandler
    {
        private string normalLogName = "MainFlow";
        private string device = MethodInfo.GetCurrentMethod().ReflectedType.Name;

        #region Configs
        private MainFlowConfig mainFlowConfig;
        private MapConfig mapConfig;
        #endregion

        private LoggerAgent loggerAgent = LoggerAgent.Instance;
        private ComputeFunction computeFunction = ComputeFunction.Instance;
        public AlarmHandler AlarmHandler { get; set; }
        public MapHandler MapControl { get; set; }
        public MIPCControlHandler MipcControl { get; set; }
        public MoveControlHandler MoveControl { get; set; }
        public LoadUnloadControlHandler LoadUnloadControl { get; set; }
        public UserAgent UserLoginout { get; set; }

        public event EventHandler<InitialEventArgs> OnComponentIntialDoneEvent;

        public LocalData localData = LocalData.Instance;

        private void MiidlerInfo()
        {
            /// 充電相關資訊.
            /// 
            bool 正在充電 = localData.MIPCData.Charging;
            /// SOC 
            double soc = localData.BatteryInfo.SOC;
            double 到站充電高水位 = localData.MIPCData.Config.ChargingSOC_High;
            double 在站點上充電水位 = localData.MIPCData.Config.ChargingSOC_Low;
            /// 這邊訊號在充電送出成功時就會on,會做delay,middler不用再做delay.
            /// 開始充電、結束充電.
            StartChargingByAddressID("48001??");
            StopCharging();

            /// 上報位置相關.

            /// 目前位置(座標)
            MapAGVPosition nowPosition = localData.Real;

            /// 這邊迷航是會是null 但如果config設定迷航時殘留上次數值,就會迷航前的資料.
            if (nowPosition != null)
            {
                double x = nowPosition.Position.X;
                double y = nowPosition.Position.Y;
                double angle = nowPosition.Angle;
            }

            double 移動方向 = localData.MoveDirectionAngle;
            double 移動速度 = localData.MoveControlData.MotionControlData.LineVelocity;
            bool 避車停止 = localData.MoveControlData.ReserveStop;
            bool 障礙物停止 = localData.MoveControlData.SafetySensorStop;

            bool AlarmBit = AlarmHandler.HasAlarm;

            bool Auto中 = (localData.AutoManual == EnumAutoState.Auto);

            /// Address Section Distance.

            VehicleLocation nowVehicleLocation = localData.Location;

            string NowSection = nowVehicleLocation.NowSection;
            /// 迷航時為空白.
            string LastAddress = nowVehicleLocation.LastAddress;
            /// 迷航時為空白.
            /// 
            bool 是否在LastAddress上 = nowVehicleLocation.InAddress;

            double distance = nowVehicleLocation.DistanceFormSectionHead;


            /// 走行相關.
            /// 移動命令 
            /// 
            bool 現在可以下命令 = localData.MoveControlData.Ready && !localData.MoveControlData.ErrorBit;
            /// ErrorBit理論上等同AlarmHandler.HasAlarm,因此應該不會發生兩邊狀態不一致.

            bool 現在MoveControl有命令 = (localData.MoveControlData.MoveCommand != null);

            string errorMessage = "";
            MoveControl.VehicleMove(null, ref errorMessage);
            /// 這樣好了 你呼叫的地方就先打null沒關係 只是上面要有三行
            string CommandID = "???";
            List<string> movingAddressList;
            List<string> movingSectionList;

            /// 停車.
            MoveControl.VehicleStop();

            /// Pause.Continue.

            MoveControl.VehiclePause();
            MoveControl.VehicleContinue();

            /// Cancel 下了會自己先pause在Stop.

            MoveControl.VehicleCancel();

            /// Override需要自己寫.



            /// 下面一個為完成上報,一個為過站上報.
            /// MoveControl.MoveCompleteEvent += MoveControl_MoveComplete;
            /// MoveControl.PassAddressEvent += MoveControl_PassAddress;



            /// 取放相關.
            /// 開始命令
            /// 

            bool Fork現在可以下命令 = localData.LoadUnloadData.Ready && !localData.LoadUnloadData.ErrorBit;
            /// 可以視為友達版本的ForkHome訊號.

            /// LoadUnloadCommand(addressID, load or unload) 
            /// return true = 開始執行, false = 無法執行
            /// 
            /// 應該不會需要停止, 停止這樣下.
            /// 這邊要考慮一下 取放貨到底能不能中斷.
            StopLoadUnload();

            /// 
            /// 取放 結束Event
            /// LoadUnloadControl.ForkCompleteEvent
            /// 

            /// 更新loading 和CSTID
            UpdateLoadingAndCSTID();
            bool 臺車有貨物 = localData.LoadUnloadData.Loading;
            string 卡夾ID = localData.LoadUnloadData.CstID;
        }

        public bool LoadUnloadCommand(string addressID, EnumLoadUnload loadOrUnload)
        {
            return false;

        }

        public void StopLoadUnload()
        {

        }

        public void UpdateLoadingAndCSTID()
        {

        }

        public bool StartChargingByAddressID(string addressID)
        {
            return false;

        }

        public bool StopCharging()
        {
            return false;
        }

        public void InitialMainFlowHander()
        {
            if (!InitailXML())
                return;
            else if (!ControllersInitial())
                return;

            InitialEvent();

            OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs("MainFlow初始化成功", true, true));
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

        private bool InitailXML()
        {
            string xmlTarget = "";

            try
            {
                XmlHandler xmlHandler = new XmlHandler();

                xmlTarget = "MainFlow.xml";
                mainFlowConfig = xmlHandler.ReadXml<MainFlowConfig>(Path.Combine(localData.ConfigPath, "MainFlow.xml"));

                localData.MainFlowConfig = mainFlowConfig;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(String.Concat("讀取 ", xmlTarget, " 成功")));
                localData.SimulateMode = mainFlowConfig.SimulateMode;

                xmlTarget = "MapConfig.xml";
                localData.MapConfig = xmlHandler.ReadXml<MapConfig>(Path.Combine(Environment.CurrentDirectory, "MapConfig.xml"));

                xmlTarget = mainFlowConfig.BatteryLogPath;

                try
                {
                    localData.BatteryLogData = xmlHandler.ReadXml<BatteryLog>(Path.Combine(localData.ConfigPath, mainFlowConfig.BatteryLogPath));
                    OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(String.Concat("讀取 ", xmlTarget, " 成功")));
                }
                catch
                {
                    OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(String.Concat("讀取 ", xmlTarget, " 失敗")));
                    xmlTarget = mainFlowConfig.BatteryBackupLogPath;

                    try
                    {
                        localData.BatteryLogData = xmlHandler.ReadXml<BatteryLog>(Path.Combine(localData.ConfigPath, mainFlowConfig.BatteryBackupLogPath));
                        OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(String.Concat("讀取 ", xmlTarget, " 成功")));
                    }
                    catch
                    {
                        OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(String.Concat("讀取 ", xmlTarget, " 失敗")));
                        localData.BatteryLogData = new BatteryLog();
                    }
                }

                return true;
            }
            catch (Exception)
            {
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(String.Concat("讀取 ", xmlTarget, " 失敗"), true));
                return false;
            }
        }

        private bool ControllersInitial()
        {
            string xmlTarget = "";

            try
            {
                UserLoginout = new UserAgent();

                xmlTarget = "MapHandler";
                MapControl = new MapHandler(normalLogName);
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(String.Concat(xmlTarget, " 初始化成功")));

                xmlTarget = "AlarmHandler";
                AlarmHandler = new AlarmHandler();
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(String.Concat(xmlTarget, " 初始化成功")));

                xmlTarget = "MIPCControlHandler";
                MipcControl = new MIPCControlHandler(this, AlarmHandler);
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(String.Concat(xmlTarget, " 初始化成功")));

                xmlTarget = "MoveControlHandler";
                MoveControl = new MoveControlHandler(MipcControl, AlarmHandler);
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(String.Concat(xmlTarget, " 初始化成功")));

                xmlTarget = "LoadUnloadControlHandler";
                LoadUnloadControl = new LoadUnloadControlHandler(MipcControl, AlarmHandler);
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(String.Concat(xmlTarget, " 初始化成功")));

                return true;
            }
            catch
            {
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(String.Concat(xmlTarget, " 初始化失敗"), true));
                return false;
            }
        }

        private void InitialEvent()
        {
            MoveControl.MoveCompleteEvent += MoveControl_MoveComplete;
            MoveControl.PassAddressEvent += MoveControl_PassAddress;
        }

        public void CloseMainFlowHandler()
        {
            MoveControl.CloseMoveControlHandler();
            MipcControl.CloseMipcControlHandler();

        }

        private bool MoveAndForkReady(ref string errorMessage)
        {
            if (!localData.MoveControlData.Ready)
            {
                errorMessage = "MoveControl Not Ready";
                return false;
            }
            else if (localData.MoveControlData.ErrorBit)
            {
                errorMessage = "MoveControl ErrorBit on ";
                return false;
            }


            if (!localData.LoadUnloadData.Ready)
            {
                errorMessage = "Fork Not Ready";
                return false;
            }
            else if (localData.LoadUnloadData.ErrorBit)
            {
                errorMessage = "Fork ErrorBit on ";
                return false;
            }

            return true;
        }


        public bool ChangeAutoManual(EnumAutoState state, ref string errorMessage)
        {
            EnumAutoState nowState = localData.AutoManual;

            if (state == nowState)
                return true;

            switch (localData.AutoManual)
            {
                case EnumAutoState.Auto:
                    if (localData.MoveControlData.MoveCommand != null)
                        MoveControl.VehicleStop();

                    localData.AutoManual = EnumAutoState.Manual;
                    return true;

                case EnumAutoState.Manual:
                    localData.AutoManual = EnumAutoState.PreAuto;

                    if (localData.MoveControlData.MoveControlCanAuto)
                    {
                        localData.AutoManual = EnumAutoState.Auto;
                        return true;
                    }
                    else
                    {
                        localData.AutoManual = EnumAutoState.Manual;
                        return false;
                    }

                case EnumAutoState.PreAuto:
                default:
                    errorMessage = String.Concat("Auto/Manual 狀態目前為 PreAuto 因此Return false");
                    return false;
            }
        }



        #region Middler.
        public void MoveControl_MoveComplete(object sender, EnumMoveComplete status)
        {
            try
            {
                localData.MoveControlData.MoveCommand.CommandStatus = EnumMoveCommandStartStatus.End;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        public void MoveControl_PassAddress(object sender, string addressID)
        {
            try
            {
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        #endregion

        public void ResetAlarm()
        {
            AlarmHandler.ResetAllAlarms();
            MipcControl.ResetMIPCAlarm();
            MoveControl.ResetAlarm();
        }
    }
}
