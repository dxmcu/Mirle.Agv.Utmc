using TcpIpClientSample;
using Mirle.Agv.Utmc.Customer;
using Mirle.Agv.Utmc.Model;
using Mirle.Agv.Utmc.Model.Configs;
using Mirle.Agv.Utmc.Model.TransferSteps;
using Mirle.Agv.Utmc.Tools;
using Mirle.Tools;
using Newtonsoft.Json;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Mirle.Agv.Utmc.Alarms;

namespace Mirle.Agv.Utmc.Controller
{
    public class MainFlowHandler
    {
        public string LogConfigPath { get; set; } = "MainLog.ini";

        #region TransCmds
        public bool IsOverrideMove { get; set; }
        public bool IsAvoidMove { get; set; }
        public bool IsArrivalCharge { get; set; } = false;

        #endregion

        #region Controller

        public AgvcConnector agvcConnector;
        public MirleLogger mirleLogger = null;
        public MapHandler mapHandler;
        public UserAgent UserAgent { get; set; }
        internal Robot.IRobotHandler RobotHandler { get; set; }
        internal Battery.IBatteryHandler BatteryHandler { get; set; }
        internal Move.IMoveHandler MoveHandler { get; set; }
        internal ConnectionMode.IConnectionModeHandler ConnectionModeHandler { get; set; }

        internal Alarms.IAlarmHandler AlarmHandler { get; set; }

        public LocalPackage LocalPackage { get; set; }

        #endregion

        #region Threads

        private Thread thdVisitTransferSteps;
        public bool IsVisitTransferStepPause { get; set; } = false;

        private Thread thdWatchChargeStage;
        public bool IsWatchChargeStagePause { get; set; } = false;

        #endregion

        #region Events
        public event EventHandler<InitialEventArgs> OnComponentIntialDoneEvent;

        #endregion

        #region Models

        public Vehicle Vehicle;

        private bool isIniOk;
        public int InitialSoc { get; set; } = 70;
        public bool IsFirstAhGet { get; set; }
        public string CanAutoMsg { get; set; } = "";
        public bool WaitingTransferCompleteEnd { get; set; } = false;
        public string DebugLogMsg { get; set; } = "";
        public LastIdlePosition LastIdlePosition { get; set; } = new LastIdlePosition();
        public bool IsLowPowerStartChargeTimeout { get; set; } = false;
        public bool IsStopChargTimeoutInRobotStep { get; set; } = false;
        public DateTime LowPowerStartChargeTimeStamp { get; set; } = DateTime.Now;
        public int LowPowerRepeatedlyChargeCounter { get; set; } = 0;
        public bool IsStopCharging { get; set; } = false;

        #endregion

        public MainFlowHandler()
        {
            isIniOk = true;
            MirleLogger.LogConfigPath = LogConfigPath;

        }

        #region InitialComponents

        public void InitialMainFlowHandler()
        {
            Vehicle = Vehicle.Instance;
            LoggersInitial();
            ConfigInitial();
            VehicleInitial();
            ControllersInitial();
            EventInitial();

            VehicleLocationInitialAndThreadsInitial();

            if (isIniOk)
            {
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "全部"));
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Start Process Ok.");
            }
        }
        private void LoggersInitial()
        {
            try
            {
                if (File.Exists(LogConfigPath))
                {
                    mirleLogger = MirleLogger.Instance;
                }
                else
                {
                    throw new Exception();
                }

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "紀錄器"));
            }
            catch (Exception)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "紀錄器缺少 Log.ini"));
            }
        }

        private void ConfigInitial()
        {
            try
            {
                //Main Configs 
                string filename = "MainFlowConfig.json";
                Vehicle.MainFlowConfig = ReadFromJsonFilename<MainFlowConfig>(filename);
                if (Vehicle.MainFlowConfig.IsSimulation)
                {
                    Vehicle.LoginLevel = EnumLoginLevel.Admin;
                }
                int minThreadSleep = 100;
                Vehicle.MainFlowConfig.VisitTransferStepsSleepTimeMs = Math.Max(Vehicle.MainFlowConfig.VisitTransferStepsSleepTimeMs, minThreadSleep);
                Vehicle.MainFlowConfig.TrackPositionSleepTimeMs = Math.Max(Vehicle.MainFlowConfig.TrackPositionSleepTimeMs, minThreadSleep);
                Vehicle.MainFlowConfig.WatchLowPowerSleepTimeMs = Math.Max(Vehicle.MainFlowConfig.WatchLowPowerSleepTimeMs, minThreadSleep);
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, $"讀寫 {filename}"));

                filename = "MapConfig.json";
                Vehicle.MapConfig = ReadFromJsonFilename<MapConfig>(filename);
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, $"讀寫 {filename}"));

                filename = "AgvcConnectorConfig.json";
                Vehicle.AgvcConnectorConfig = ReadFromJsonFilename<AgvcConnectorConfig>(filename);
                Vehicle.AgvcConnectorConfig.ScheduleIntervalMs = Math.Max(Vehicle.AgvcConnectorConfig.ScheduleIntervalMs, minThreadSleep);
                Vehicle.AgvcConnectorConfig.AskReserveIntervalMs = Math.Max(Vehicle.AgvcConnectorConfig.AskReserveIntervalMs, minThreadSleep);
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, $"讀寫 {filename}"));

                filename = "AlarmConfig.json";
                Vehicle.AlarmConfig = ReadFromJsonFilename<AlarmConfig>(filename);
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, $"讀寫 {filename}"));

                filename = "BatteryLog.json";
                Vehicle.BatteryLog = ReadFromJsonFilename<BatteryLog>(filename);
                InitialSoc = Vehicle.BatteryLog.InitialSoc;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, $"讀寫 {filename}"));

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "讀寫設定檔"));
            }
            catch (Exception)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "讀寫設定檔"));
            }
        }

        private T ReadFromJsonFilename<T>(string filename)
        {
            var allText = System.IO.File.ReadAllText(filename);
            return JsonConvert.DeserializeObject<T>(allText);
        }

        private void ControllersInitial()
        {
            try
            {
                mapHandler = new MapHandler();
                agvcConnector = new AgvcConnector(this);
                UserAgent = new UserAgent();

                if (Vehicle.MainFlowConfig.IsSimulation)
                {
                    RobotHandler = new Robot.NullObjRobotHandler(Vehicle.RobotStatus, Vehicle.CarrierSlotStatus);
                    BatteryHandler = new Battery.NullObjBatteryHandler(Vehicle.BatteryStatus);
                    MoveHandler = new Move.NullObjMoveHandler(Vehicle.MoveStatus, Vehicle.MapInfo);
                    ConnectionModeHandler = new ConnectionMode.NullObjConnectionModeHandler(Vehicle.AutoState);
                    AlarmHandler = new Alarms.NullObjAlarmHandler();

                    Vehicle.MainFlowConfig.WatchLowPowerSleepTimeMs = 60 * 1000;
                }
                else
                {
                    LocalPackage = new LocalPackage();
                    LocalPackage.OnLocalPackageComponentIntialDoneEvent += LocalPackage_OnLocalPackageComponentIntialDoneEvent;
                    LocalPackage.InitialLocalMain();

                    RobotHandler = new Robot.UtmcRobotAdapter(LocalPackage);
                    BatteryHandler = new Battery.UtmcBatteryAdapter(LocalPackage);
                    MoveHandler = new Move.UtmcMoveAdapter(LocalPackage);
                    ConnectionModeHandler = new ConnectionMode.UtmcConnectionModeAdapter(LocalPackage);
                    AlarmHandler = new Alarms.UtmcAlarmAdapter(LocalPackage);
                }

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "控制層"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "控制層"));
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void LocalPackage_OnLocalPackageComponentIntialDoneEvent(object sender, InitialEventArgs e)
        {
            OnComponentIntialDoneEvent?.Invoke(this, e);
        }

        private void VehicleInitial()
        {
            try
            {
                IsFirstAhGet = Vehicle.MainFlowConfig.IsSimulation;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "台車"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "台車"));
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void EventInitial()
        {
            try
            {
                //來自middleAgent的NewTransCmds訊息, Send to MainFlow(this)'mapHandler
                agvcConnector.OnInstallTransferCommandEvent += AgvcConnector_OnInstallTransferCommandEvent;
                agvcConnector.OnAvoideRequestEvent += AgvcConnector_OnAvoideRequestEvent;
                agvcConnector.OnRenameCassetteIdEvent += AgvcConnector_OnRenameCassetteIdEvent;

                agvcConnector.OnSendRecvTimeoutEvent += AgvcConnector_OnSendRecvTimeoutEvent;
                agvcConnector.OnCstRenameEvent += AgvcConnector_OnCstRenameEvent;

                ConnectionModeHandler.OnModeChangeEvent += ConnectionModeHandler_OnModeChangeEvent;
                ConnectionModeHandler.OnLogDebugEvent += IMessageHandler_OnLogDebugEvent;
                ConnectionModeHandler.OnLogErrorEvent += IMessageHandler_OnLogErrorEvent;

                MoveHandler.OnUpdateMoveStatusEvent += MoveHandler_OnUpdateMoveStatusEvent;
                MoveHandler.OnUpdatePositionArgsEvent += MoveHandler_OnUpdatePositionArgsEvent;
                MoveHandler.OnOpPauseOrResumeEvent += MoveHandler_OnOpPauseOrResumeEvent;
                MoveHandler.OnLogDebugEvent += IMessageHandler_OnLogDebugEvent;
                MoveHandler.OnLogErrorEvent += IMessageHandler_OnLogErrorEvent;

                RobotHandler.OnRobotEndEvent += RobotHandler_OnRobotEndEvent;
                RobotHandler.OnUpdateCarrierSlotStatusEvent += RobotHandler_OnUpdateCarrierSlotStatusEvent;
                RobotHandler.OnUpdateRobotStatusEvent += RobotHandler_OnUpdateRobotStatusEvent;
                RobotHandler.OnLogDebugEvent += IMessageHandler_OnLogDebugEvent;
                RobotHandler.OnLogErrorEvent += IMessageHandler_OnLogErrorEvent;

                BatteryHandler.OnUpdateBatteryStatusEvent += BatteryHandler_OnUpdateBatteryStatusEvent;
                BatteryHandler.OnUpdateChargeStatusEvent += BatteryHandler_OnUpdateChargeStatusEvent;
                BatteryHandler.OnLogDebugEvent += IMessageHandler_OnLogDebugEvent;
                BatteryHandler.OnLogErrorEvent += IMessageHandler_OnLogErrorEvent;

                AlarmHandler.OnSetAlarmToAgvcEvent += AlarmHandler_OnSetAlarmToAgvcEvent;
                AlarmHandler.OnResetAlarmToAgvcEvent += AlarmHandler_OnResetAlarmToAgvcEvent;
                AlarmHandler.OnLogDebugEvent += IMessageHandler_OnLogDebugEvent;
                AlarmHandler.OnLogErrorEvent += IMessageHandler_OnLogErrorEvent;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "事件"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "事件"));

                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }


        private void VehicleLocationInitialAndThreadsInitial()
        {
            MoveHandler.InitialPosition();

            StartVisitTransferSteps();
            StartWatchChargeStage();
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"讀取到的電量為{Vehicle.BatteryLog.InitialSoc}");
        }

        #endregion

        #region Thd Visit TransferSteps

        public void StartVisitTransferSteps()
        {
            thdVisitTransferSteps = new Thread(VisitTransferStepsSwitchCase);
            thdVisitTransferSteps.IsBackground = true;
            thdVisitTransferSteps.Start();

            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"MainFlow : StartVisitTransferSteps");
        }

        public void PauseVisitTransferSteps()
        {
            IsVisitTransferStepPause = true;
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"MainFlow : PauseVisitTransferSteps");
        }

        public void ResumeVisitTransferSteps()
        {
            IsVisitTransferStepPause = false;
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"MainFlow : ResumeVisitTransferSteps");
        }

        private void VisitTransferStepsSwitchCase()
        {
            while (true)
            {
                try
                {
                    if (Vehicle.TransferCommand.IsStopAndClear)
                    {
                        ClearTransferTransferCommand();
                        Thread.Sleep(Vehicle.MainFlowConfig.VisitTransferStepsSleepTimeMs);
                        continue;
                    }
                    else if (IsVisitTransferStepPause)
                    {
                        Thread.Sleep(Vehicle.MainFlowConfig.VisitTransferStepsSleepTimeMs);
                        continue;
                    }

                    switch (Vehicle.TransferCommand.TransferStep)
                    {
                        case EnumTransferStep.Idle:
                            Idle();
                            break;
                        case EnumTransferStep.MoveToLoad:
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令.移動.準備.至取貨站] MoveToLoad.");
                            MoveToAddress(Vehicle.TransferCommand.LoadAddressId, EnumMoveToEndReference.Load);
                            break;
                        case EnumTransferStep.MoveToAddress:
                        case EnumTransferStep.MoveToUnload:
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令.移動.準備.至終站] MoveToUnload or MoveToAddress.");
                            MoveToAddress(Vehicle.TransferCommand.UnloadAddressId, EnumMoveToEndReference.Unload);
                            break;
                        case EnumTransferStep.MoveToAvoid:
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令.移動.準備.至避車站] MoveToAvoid.");
                            MoveToAddress(Vehicle.MovingGuide.ToAddressId, EnumMoveToEndReference.Avoid);
                            break;
                        case EnumTransferStep.MoveToAvoidWaitArrival:
                            if (Vehicle.MoveStatus.IsMoveEnd)
                            {
                                MoveToAddressEnd();
                            }
                            else if (Vehicle.MoveStatus.LastAddress.Id == Vehicle.MovingGuide.ToAddressId)
                            {
                                Vehicle.MovingGuide.IsAvoidMove = true;
                                Vehicle.MovingGuide.MoveComplete = EnumMoveComplete.Success;
                                MoveToAddressEnd();
                            }
                            break;
                        case EnumTransferStep.AvoidMoveComplete:
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[避車.到站.回報.完成] AvoidMoveComplete.");
                            AvoidMoveComplete();
                            break;
                        case EnumTransferStep.MoveToAddressWaitEnd:
                            if (Vehicle.MoveStatus.IsMoveEnd)
                            {
                                MoveToAddressEnd();
                            }
                            break;
                        case EnumTransferStep.LoadArrival:
                            LoadArrival();
                            break;
                        case EnumTransferStep.WaitLoadArrivalReply:
                            if (Vehicle.TransferCommand.IsLoadArrivalReply)
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.到站.回報.成功] AgvcConnector_LoadArrivalReply.");
                                Vehicle.TransferCommand.TransferStep = EnumTransferStep.Load;
                            }
                            break;
                        case EnumTransferStep.Load:
                            Load();
                            break;
                        case EnumTransferStep.LoadWaitEnd:
                            if (Vehicle.TransferCommand.IsRobotEnd)
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.動作.結束] LoadComplete.");
                                LoadComplete();
                            }
                            break;
                        case EnumTransferStep.WaitLoadCompleteReply:
                            if (Vehicle.TransferCommand.IsLoadCompleteReply)
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[取貨.完成.回報.成功] AgvcConnector_LoadCompleteReply.");
                                Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitCstIdReadReply;
                                Vehicle.TransferCommand.IsCstIdReadReply = false;
                                agvcConnector.SendRecv_Cmd136_CstIdReadReport();
                            }
                            break;
                        case EnumTransferStep.WaitCstIdReadReply:
                            if (Vehicle.TransferCommand.IsCstIdReadReply)
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[取貨.貨號回報.成功] AgvcConnector_CstIdReadReply.");

                                LoadEnd();
                            }
                            break;
                        case EnumTransferStep.UnloadArrival:
                            UnloadArrival();
                            break;
                        case EnumTransferStep.WaitUnloadArrivalReply:
                            if (Vehicle.TransferCommand.IsUnloadArrivalReply)
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放貨.到站.回報.成功] AgvcConnector_OnAgvcAcceptUnloadArrivalEvent.");

                                Vehicle.TransferCommand.TransferStep = EnumTransferStep.Unload;
                            }
                            break;
                        case EnumTransferStep.Unload:
                            Unload();
                            break;
                        case EnumTransferStep.UnloadWaitEnd:
                            if (Vehicle.TransferCommand.IsRobotEnd)
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放貨.動作.結束] UnloadComplete.");
                                UnloadComplete();
                            }
                            break;
                        case EnumTransferStep.WaitUnloadCompleteReply:
                            if (Vehicle.TransferCommand.IsUnloadCompleteReply)
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[放貨.完成.回報.成功] AgvcConnector_UnloadCompleteReply.");

                                UnloadEnd();
                            }
                            break;
                        case EnumTransferStep.TransferComplete:
                            TransferCommandComplete();
                            break;
                        case EnumTransferStep.WaitOverrideToContinue:
                            break;
                        case EnumTransferStep.MoveFail:
                        case EnumTransferStep.RobotFail:
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }

                Thread.Sleep(Vehicle.MainFlowConfig.VisitTransferStepsSleepTimeMs);
            }
        }

        public void GetAllStatusReport()
        {
            RobotHandler.GetRobotAndCarrierSlotStatus();
            MoveHandler.GetMoveStatus();
            BatteryHandler.GetBatteryAndChargeStatus();
        }

        #region Move Step

        private void MoveToAddress(string endAddressId, EnumMoveToEndReference endReference)
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令.移動.準備] MoveToAddress.[{endAddressId}].[{endReference}]");

                Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAddressWaitEnd;
                Vehicle.MovingGuide = new MovingGuide();

                if (endAddressId == Vehicle.MoveStatus.LastAddress.Id)
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[原地到站] Same address end.");

                    Vehicle.MovingGuide.MoveComplete = EnumMoveComplete.Success;
                    Vehicle.MoveStatus.IsMoveEnd = true;
                }
                else
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[移動前.斷充] Move Stop Charge");
                    StopCharge();

                    Vehicle.MovingGuide.CommandId = Vehicle.TransferCommand.CommandId;
                    agvcConnector.ReportSectionPass();
                    if (!Vehicle.IsCharging)
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[退出.站點] Move Begin.");
                        Vehicle.MoveStatus.IsMoveEnd = false;
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"IsMoveEnd Need False And Cur IsMoveEnd = {Vehicle.MoveStatus.IsMoveEnd}");

                        agvcConnector.ClearAllReserve();
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[詢問.路線] AskGuideAddressesAndSections.");
                        switch (endReference)
                        {
                            case EnumMoveToEndReference.Load:
                                PrepareToMove(Vehicle.TransferCommand.ToLoadSectionIds, Vehicle.TransferCommand.ToLoadAddressIds);
                                break;
                            case EnumMoveToEndReference.Unload:
                                PrepareToMove(Vehicle.TransferCommand.ToUnloadSectionIds, Vehicle.TransferCommand.ToUnloadAddressIds);
                                break;
                            case EnumMoveToEndReference.Avoid:
                                Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAvoidWaitArrival;
                                break;
                            default:
                                break;
                        }
                        Vehicle.MovingGuide.ToAddressId = endAddressId;
                    }
                    else
                    {
                        AlarmHandler.SetAlarmFromAgvm(58);
                        Thread.Sleep(3000);
                        if (endReference == EnumMoveToEndReference.Avoid)
                        {
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAvoid;
                        }
                        else
                        {
                            switch (Vehicle.TransferCommand.EnrouteState)
                            {
                                case CommandState.LoadEnroute:
                                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                                    break;
                                case CommandState.None:
                                case CommandState.UnloadEnroute:
                                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void PrepareToMove(List<string> sectionIds, List<string> addressIds)
        {
            Vehicle.MovingGuide = new MovingGuide()
            {
                CommandId = Vehicle.TransferCommand.CommandId,
                GuideAddressIds = addressIds,
                GuideSectionIds = sectionIds,
                FromAddressId = Vehicle.MoveStatus.LastAddress.Id
            };
            Vehicle.MoveStatus.IsMoveEnd = false;
            SetupMovingGuideMovingSections();
            agvcConnector.SetupNeedReserveSections();
            agvcConnector.StatusChangeReport();
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, Vehicle.MovingGuide.GetJsonInfo());
        }

        private void MoveToAddressEnd()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[移動.結束] MoveToAddressEnd IsMoveEnd = {Vehicle.MoveStatus.IsMoveEnd}");

                agvcConnector.ClearAllReserve();

                #region Not EnumMoveComplete.Success

                if (Vehicle.MovingGuide.MoveComplete == EnumMoveComplete.Fail)
                {
                    Vehicle.MoveStatus.IsMoveEnd = false;
                    if (Vehicle.MovingGuide.IsAvoidMove)
                    {
                        agvcConnector.AvoidFail();
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[避車.移動.失敗] : Avoid Fail. ");
                        Vehicle.MovingGuide.IsAvoidMove = false;
                    }
                    else if (Vehicle.MovingGuide.IsOverrideMove)
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[變更路徑.移動失敗] :  Override Move Fail. ");
                        Vehicle.MovingGuide.IsOverrideMove = false;
                    }
                    else
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[移動.失敗] : Move Fail. ");
                    }

                    AlarmHandler.SetAlarmFromAgvm(6);
                    agvcConnector.StatusChangeReport();

                    StopClearAndReset();

                    return;
                }

                #endregion

                #region EnumMoveComplete.Success

                if (Vehicle.MovingGuide.MoveComplete == EnumMoveComplete.Success)
                {
                    if (Vehicle.MovingGuide.IsAvoidMove)
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[避車.到站] AvoidMoveComplete.");

                        Vehicle.MovingGuide.IsAvoidMove = false;
                        Vehicle.TransferCommand.TransferStep = EnumTransferStep.AvoidMoveComplete;
                        Vehicle.MovingGuide.IsAvoidComplete = true;
                        agvcConnector.AvoidComplete();
                    }
                    else
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[移動.二次定位.到站] : Move End Ok.");
                        if (!Vehicle.IsCharging) ArrivalStartCharge(Vehicle.MoveStatus.LastAddress);
                        Vehicle.MovingGuide = new MovingGuide();
                        agvcConnector.StatusChangeReport();

                        switch (Vehicle.TransferCommand.EnrouteState)
                        {
                            case CommandState.None:
                                agvcConnector.MoveArrival();
                                Vehicle.TransferCommand.TransferStep = EnumTransferStep.TransferComplete;
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Move Arrival. [AddressId = {Vehicle.MoveStatus.LastAddress.Id}]");
                                break;
                            case CommandState.LoadEnroute:
                                Vehicle.TransferCommand.TransferStep = EnumTransferStep.LoadArrival;
                                break;
                            case CommandState.UnloadEnroute:
                                Vehicle.TransferCommand.TransferStep = EnumTransferStep.UnloadArrival;
                                break;
                            default:
                                break;
                        }
                    }

                }

                #endregion
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AvoidMoveComplete()
        {
            try
            {
                switch (Vehicle.TransferCommand.EnrouteState)
                {
                    case CommandState.None:
                        if (Vehicle.MoveStatus.LastAddress.Id == Vehicle.TransferCommand.UnloadAddressId)
                        {
                            agvcConnector.MoveArrival();
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAddressWaitEnd;
                            Vehicle.MovingGuide.ToAddressId = Vehicle.TransferCommand.UnloadAddressId;
                        }
                        else
                        {
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitOverrideToContinue;
                        }
                        break;
                    case CommandState.LoadEnroute:
                        if (Vehicle.MoveStatus.LastAddress.Id == Vehicle.TransferCommand.LoadAddressId)
                        {
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAddressWaitEnd;
                            Vehicle.MovingGuide.ToAddressId = Vehicle.TransferCommand.LoadAddressId;
                        }
                        else
                        {
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitOverrideToContinue;
                        }
                        break;
                    case CommandState.UnloadEnroute:
                        if (Vehicle.MoveStatus.LastAddress.Id == Vehicle.TransferCommand.UnloadAddressId)
                        {
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAddressWaitEnd;
                            Vehicle.MovingGuide.ToAddressId = Vehicle.TransferCommand.UnloadAddressId;
                        }
                        else
                        {
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitOverrideToContinue;
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public bool CanVehMove()
        {
            try
            {
                if (Vehicle.IsCharging) //dabid
                {
                    StopCharge();
                }
            }
            catch
            {

            }
            return Vehicle.RobotStatus.IsHome && !Vehicle.IsCharging;

        }

        public void SetupMovingGuideMovingSections()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[設定.路線] Setup MovingGuide.");

                MovingGuide movingGuide = new MovingGuide(Vehicle.MovingGuide);
                movingGuide.MovingSections.Clear();
                for (int i = 0; i < Vehicle.MovingGuide.GuideSectionIds.Count; i++)
                {
                    MapSection mapSection = new MapSection();
                    string sectionId = movingGuide.GuideSectionIds[i].Trim();
                    string addressId = movingGuide.GuideAddressIds[i + 1].Trim();
                    if (!Vehicle.MapInfo.sectionMap.ContainsKey(sectionId))
                    {
                        throw new Exception($"Map info has no this section ID.[{sectionId}]");
                    }
                    else if (!Vehicle.MapInfo.addressMap.ContainsKey(addressId))
                    {
                        throw new Exception($"Map info has no this address ID.[{addressId}]");
                    }

                    mapSection = Vehicle.MapInfo.sectionMap[sectionId];
                    mapSection.CmdDirection = addressId == mapSection.TailAddress.Id ? EnumCommandDirection.Forward : EnumCommandDirection.Backward;
                    movingGuide.MovingSections.Add(mapSection);
                }
                Vehicle.MovingGuide = movingGuide;
                MoveHandler.SetMovingGuide(movingGuide);

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[設定.路線.成功] Setup MovingGuide OK.");
            }
            catch (Exception ex)
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[設定.路線.失敗] Setup MovingGuide Fail.");
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                Vehicle.MovingGuide.MovingSections = new List<MapSection>();
                AlarmHandler.SetAlarmFromAgvm(18);
                StopClearAndReset();
            }
        }

        public void SetPositionArgs(PositionArgs positionArgs)
        {
            MoveHandler_OnUpdatePositionArgsEvent(this, positionArgs);
        }

        public void AgvcConnector_GetReserveOkUpdateMoveControlNextPartMovePosition(MapSection mapSection, EnumIsExecute keepOrGo)
        {
            try
            {
                int sectionIndex = Vehicle.MovingGuide.GuideSectionIds.FindIndex(x => x == mapSection.Id);
                MapAddress address = Vehicle.MapInfo.addressMap[Vehicle.MovingGuide.GuideAddressIds[sectionIndex + 1]];

                MoveHandler.ReserveOkPartMove(mapSection);

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Send to MoveControl get reserve {mapSection.Id} ok , next end point [{address.Id}]({Convert.ToInt32(address.Position.X)},{Convert.ToInt32(address.Position.Y)}).");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void MoveHandler_OnUpdatePositionArgsEvent(object sender, PositionArgs positionArgs)
        {
            try
            {
                bool reportAddressPass = positionArgs.EnumLocalArrival != EnumLocalArrival.Arrival || Vehicle.MoveStatus.LastMapPosition.MyDistance(positionArgs.MapPosition) > Vehicle.MapConfig.AddressAreaMm;


                MovingGuide movingGuide = new MovingGuide(Vehicle.MovingGuide);
                MoveStatus moveStatus = new MoveStatus(Vehicle.MoveStatus);
                moveStatus.LastMapPosition = positionArgs.MapPosition;
                CheckPositionUnchangeTimeout(positionArgs);

                if (positionArgs.EnumLocalArrival == EnumLocalArrival.EndArrival || positionArgs.EnumLocalArrival == EnumLocalArrival.Fail)
                {
                    reportAddressPass = true;
                }

                string tempAddressId = Vehicle.MoveStatus.LastAddress.Id.Trim();

                if (movingGuide.GuideSectionIds.Any())
                {
                    if (positionArgs.EnumLocalArrival == EnumLocalArrival.EndArrival)
                    {
                        moveStatus.NearlyAddress = Vehicle.MapInfo.addressMap[movingGuide.ToAddressId];
                        moveStatus.NearlySection = movingGuide.MovingSections.Last();
                    }
                    else
                    {
                        moveStatus.NearlyAddress = (from addressId in movingGuide.GuideAddressIds
                                                    select Vehicle.MapInfo.addressMap[addressId]).ToList()
                                                    .OrderBy(address => address.MyDistance(positionArgs.MapPosition)).ToArray()
                                                    .First();

                        moveStatus.NearlySection = (from sectionId in movingGuide.GuideSectionIds
                                                    select Vehicle.MapInfo.sectionMap[sectionId]).ToList()
                                                    .FirstOrDefault(section => section.InSection(moveStatus.NearlyAddress));

                    }

                    moveStatus.NearlySection.VehicleDistanceSinceHead = moveStatus.NearlyAddress.MyDistance(moveStatus.NearlySection.HeadAddress.Position);
                    moveStatus.LastAddress = moveStatus.NearlyAddress;
                    moveStatus.LastSection = moveStatus.NearlySection;
                    moveStatus.HeadDirection = positionArgs.HeadAngle;
                    moveStatus.MovingDirection = positionArgs.MovingDirection;
                    moveStatus.Speed = positionArgs.Speed;
                    Vehicle.MoveStatus = moveStatus;

                    UpdateMovePassSections(moveStatus.LastSection.Id);

                    for (int i = 0; i < movingGuide.MovingSections.Count; i++)
                    {
                        if (movingGuide.MovingSections[i].Id == moveStatus.LastSection.Id)
                        {
                            Vehicle.MovingGuide.MovingSectionsIndex = i;
                        }
                    }
                }
                else
                {
                    moveStatus.NearlyAddress = Vehicle.MapInfo.addressMap.Values.ToList()
                                               .OrderBy(address => address.MyDistance(positionArgs.MapPosition)).ToArray()
                                               .First();

                    moveStatus.NearlySection = Vehicle.MapInfo.sectionMap.Values.ToList()
                                               .FirstOrDefault(section => section.InSection(moveStatus.NearlyAddress));

                    moveStatus.NearlySection.VehicleDistanceSinceHead = moveStatus.NearlySection.HeadAddress.MyDistance(positionArgs.MapPosition);
                    moveStatus.LastMapPosition = positionArgs.MapPosition;
                    moveStatus.LastAddress = moveStatus.NearlyAddress;
                    moveStatus.LastSection = moveStatus.NearlySection;
                    moveStatus.HeadDirection = positionArgs.HeadAngle;
                    moveStatus.MovingDirection = positionArgs.MovingDirection;
                    moveStatus.Speed = positionArgs.Speed;
                    Vehicle.MoveStatus = moveStatus;
                }

                if (reportAddressPass && tempAddressId != moveStatus.LastAddress.Id)
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[車輛.經過.站點] UpdatePositionArgs. [{positionArgs.EnumLocalArrival}][({(int)positionArgs.MapPosition.X},{(int)positionArgs.MapPosition.Y})][Direction={positionArgs.MovingDirection}][Speed={positionArgs.Speed}]");

                    agvcConnector.ReportSectionPass();
                }

                switch (positionArgs.EnumLocalArrival)
                {
                    case EnumLocalArrival.Fail:
                        Vehicle.MovingGuide.MoveComplete = EnumMoveComplete.Fail;
                        Vehicle.MoveStatus.IsMoveEnd = true;
                        break;
                    case EnumLocalArrival.Arrival:
                        break;
                    case EnumLocalArrival.EndArrival:
                        Vehicle.MovingGuide.MoveComplete = EnumMoveComplete.Success;
                        Vehicle.MoveStatus.IsMoveEnd = true;
                        break;
                    default:
                        break;
                }

                MoveHandler.SetMoveStatusFrom(Vehicle.MoveStatus);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void UpdateMovePassSections(string id)
        {
            int getReserveOkSectionIndex = 0;
            try
            {
                var getReserveOkSections = agvcConnector.GetReserveOkSections();
                getReserveOkSectionIndex = getReserveOkSections.FindIndex(x => x.Id == id);
                if (getReserveOkSectionIndex < 0) return;
                for (int i = 0; i < getReserveOkSectionIndex; i++)
                {
                    //Remove passed section in ReserveOkSection
                    agvcConnector.DequeueGotReserveOkSections();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"FAIL [SecId={id}][Index={getReserveOkSectionIndex}]");
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

        }

        private void MoveHandler_OnUpdateMoveStatusEvent(object sender, MoveStatus moveStatus)
        {
            Vehicle.MoveStatus = moveStatus;
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[移動.狀態.改變] UpdateMoveStatus:[{Vehicle.MoveStatus.EnumMoveState}][Address={Vehicle.MoveStatus.LastAddress.Id}][Section={Vehicle.MoveStatus.LastSection.Id}]");
        }

        private void MoveHandler_OnOpPauseOrResumeEvent(object sender, bool e)
        {
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[遙控器.狀態.改變] MoveHandler_OnOpPauseOrResumeEvent [IsPause={e}].");

            if (e)
            {
                Vehicle.OpPauseStatus = VhStopSingle.StopSingleOn;
                agvcConnector.StatusChangeReport();
            }
            else
            {
                Vehicle.OpPauseStatus = VhStopSingle.StopSingleOff;
                Vehicle.ResetPauseFlags();
                ResumeMiddler();
            }
        }

        private void CheckPositionUnchangeTimeout(PositionArgs positionArgs)
        {
            if (!Vehicle.MoveStatus.IsMoveEnd)
            {
                if (LastIdlePosition.Position.MyDistance(positionArgs.MapPosition) <= Vehicle.MainFlowConfig.IdleReportRangeMm)
                {
                    if ((DateTime.Now - LastIdlePosition.TimeStamp).TotalMilliseconds >= Vehicle.MainFlowConfig.IdleReportIntervalMs)
                    {
                        UpdateLastIdlePositionAndTimeStamp(positionArgs);
                        AlarmHandler.SetAlarmFromAgvm(55);
                    }
                }
                else
                {
                    UpdateLastIdlePositionAndTimeStamp(positionArgs);
                }
            }
            else
            {
                LastIdlePosition.TimeStamp = DateTime.Now;
            }
        }

        private void UpdateLastIdlePositionAndTimeStamp(PositionArgs positionArgs)
        {
            LastIdlePosition lastIdlePosition = new LastIdlePosition();
            lastIdlePosition.Position = positionArgs.MapPosition;
            lastIdlePosition.TimeStamp = DateTime.Now;
            LastIdlePosition = lastIdlePosition;
        }

        #endregion

        #region Robot Step

        private void LoadArrival()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.到站.回報] Load Arrival. [AddressId = {Vehicle.MoveStatus.LastAddress.Id}]");

                Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitLoadArrivalReply;
                Vehicle.TransferCommand.IsLoadArrivalReply = false;
                agvcConnector.ReportLoadArrival();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void Load()
        {
            try
            {
                if (Vehicle.TransferCommand.IsStopAndClear) return;

                Vehicle.TransferCommand.IsRobotEnd = false;
                Vehicle.TransferCommand.TransferStep = EnumTransferStep.LoadWaitEnd;

                if (Vehicle.CarrierSlotStatus.EnumCarrierSlotState == EnumCarrierSlotState.Empty && Vehicle.MainFlowConfig.SlotDisable != EnumSlotSelect.Left)
                {
                    Vehicle.TransferCommand.SlotNumber = EnumSlotNumber.L;
                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                }
                //else if (Vehicle.CarrierSlotRight.EnumCarrierSlotState == EnumCarrierSlotState.Empty && Vehicle.MainFlowConfig.SlotDisable != EnumSlotSelect.Right)
                //{
                //    Vehicle.TransferCommand.SlotNumber = EnumSlotNumber.R;
                //    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                //}
                else
                {
                    //VehicleSlotFullFindFitUnloadCommand();
                    //return;
                    throw new Exception("No slot to Load.");
                }

                LoadCmdInfo loadCmd = GetLoadCommand();
                if (Vehicle.MoveStatus.LastAddress.Id != Vehicle.TransferCommand.LoadAddressId)
                {
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                    return;
                }

                //if (loadCmd.PortNumber == "9")
                //{
                //    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                //    return;
                //}

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[執行.取貨] Loading, [Load Adr={loadCmd.PortAddressId}]");
                agvcConnector.Loading();

                RobotHandler.DoRobotCommandFor(loadCmd);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private LoadCmdInfo GetLoadCommand()
        {
            try
            {
                //MapAddress portAddress = Vehicle.Mapinfo.addressMap[Vehicle.TransferCommand.LoadAddressId];
                LoadCmdInfo robotCommand = new LoadCmdInfo(Vehicle.TransferCommand);
                //robotCommand.PioDirection = portAddress.PioDirection;
                //robotCommand.GateType = portAddress.GateType;
                //string portId = Vehicle.TransferCommand.LoadPortId.Trim();

                //if (!Vehicle.Mapinfo.portMap.ContainsKey(portId))
                //{
                //    robotCommand.PortNumber = "1";
                //}
                //else
                //{
                //    var port = Vehicle.Mapinfo.portMap[portId];
                //    if (port.IsVitualPort)
                //    {
                //        robotCommand.PortNumber = "9";
                //    }
                //    else
                //    {
                //        robotCommand.PortNumber = port.Number;
                //        Vehicle.TransferCommand.LoadAddressId = port.ReferenceAddressId;
                //    }
                //}

                return robotCommand;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return new LoadCmdInfo(Vehicle.TransferCommand);
            }
        }

        private void LoadComplete()
        {
            try
            {
                Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitLoadCompleteReply;
                ConfirmBcrReadResultInLoad(Vehicle.TransferCommand.SlotNumber);
                Vehicle.TransferCommand.IsLoadCompleteReply = false;
                Vehicle.TransferCommand.EnrouteState = CommandState.UnloadEnroute;
                agvcConnector.LoadComplete();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ConfirmBcrReadResultInLoad(EnumSlotNumber slotNumber)
        {
            try
            {
                CarrierSlotStatus slotStatus = Vehicle.GetCarrierSlotStatusFrom(slotNumber);

                if (Vehicle.MainFlowConfig.BcrByPass)
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.關閉] BcrByPass.");

                    switch (slotStatus.EnumCarrierSlotState)
                    {
                        case EnumCarrierSlotState.Empty:
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.失敗] BcrByPass, slot is empty.");

                                slotStatus.CarrierId = "";
                                slotStatus.EnumCarrierSlotState = EnumCarrierSlotState.Empty;
                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.CarrierSlotStatus = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                //else
                                //{
                                //    Vehicle.CarrierSlotRight = slotStatus;
                                //    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;

                                //}
                            }
                            break;
                        case EnumCarrierSlotState.ReadFail:
                        case EnumCarrierSlotState.Loading:
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.成功] BcrByPass, loading is true.");
                                slotStatus.CarrierId = Vehicle.TransferCommand.CassetteId;
                                slotStatus.EnumCarrierSlotState = EnumCarrierSlotState.Loading;

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.CarrierSlotStatus = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrNormal;
                                }
                                //else
                                //{
                                //    Vehicle.CarrierSlotRight = slotStatus;
                                //    Vehicle.RightReadResult = BCRReadResult.BcrNormal;
                                //}
                            }
                            break;
                        case EnumCarrierSlotState.PositionError:
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.凸片] CST Position Error.");

                                slotStatus.CarrierId = "PositionError";
                                slotStatus.EnumCarrierSlotState = EnumCarrierSlotState.PositionError;

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.CarrierSlotStatus = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                //else
                                //{
                                //    Vehicle.CarrierSlotRight = slotStatus;
                                //    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                                //}

                                AlarmHandler.SetAlarmFromAgvm(000051);
                                return;
                            }
                        default:
                            break;
                    }
                }
                else
                {
                    switch (slotStatus.EnumCarrierSlotState)
                    {
                        case EnumCarrierSlotState.Empty:
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.失敗] CST ID is empty.");

                                slotStatus.CarrierId = "";
                                slotStatus.EnumCarrierSlotState = EnumCarrierSlotState.Empty;
                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.CarrierSlotStatus = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                //else
                                //{
                                //    Vehicle.CarrierSlotRight = slotStatus;
                                //    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                                //}

                                AlarmHandler.SetAlarmFromAgvm(000051);
                            }
                            break;
                        case EnumCarrierSlotState.Loading:
                            if (Vehicle.TransferCommand.CassetteId == slotStatus.CarrierId.Trim())
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.成功] CST ID = [{slotStatus.CarrierId.Trim()}] read ok.");

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.LeftReadResult = BCRReadResult.BcrNormal;
                                }
                                //else
                                //{
                                //    Vehicle.RightReadResult = BCRReadResult.BcrNormal;
                                //}
                            }
                            else
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.失敗] Read CST ID = [{slotStatus.CarrierId}], unmatched command CST ID = [{Vehicle.TransferCommand.CassetteId }]");

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.LeftReadResult = BCRReadResult.BcrMisMatch;
                                }
                                //else
                                //{
                                //    Vehicle.RightReadResult = BCRReadResult.BcrMisMatch;
                                //}

                                AlarmHandler.SetAlarmFromAgvm(000028);
                            }
                            break;
                        case EnumCarrierSlotState.ReadFail:
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[取貨.讀取.失敗] ReadFail.");

                                slotStatus.CarrierId = "ReadFail";
                                slotStatus.EnumCarrierSlotState = EnumCarrierSlotState.ReadFail;

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.CarrierSlotStatus = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                //else
                                //{
                                //    Vehicle.CarrierSlotRight = slotStatus;
                                //    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                                //}
                                AlarmHandler.SetAlarmFromAgvm(000004);
                            }
                            break;
                        case EnumCarrierSlotState.PositionError:
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.凸片] CST Position Error.");

                                slotStatus.CarrierId = "PositionError";
                                slotStatus.EnumCarrierSlotState = EnumCarrierSlotState.PositionError;

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.CarrierSlotStatus = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                //else
                                //{
                                //    Vehicle.CarrierSlotRight = slotStatus;
                                //    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                                //}

                                AlarmHandler.SetAlarmFromAgvm(000051);
                                return;
                            }
                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void LoadEnd()
        {
            try
            {
                if (Vehicle.TransferCommand.AgvcTransCommandType == EnumAgvcTransCommandType.Load)
                {
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.TransferComplete;
                }
                else
                {
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                    Vehicle.TransferCommand.EnrouteState = CommandState.UnloadEnroute;

                    if (Vehicle.MainFlowConfig.DualCommandOptimize)
                    {
                        LoadEndOptimize();
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void LoadEndOptimize()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨完成.命令選擇] Load End Optimize");

                var transferCommands = Vehicle.mapTransferCommands.Values.ToArray();

                foreach (var transferCommand in transferCommands)
                {
                    if (transferCommand.IsStopAndClear)
                    {
                        Vehicle.TransferCommand = transferCommand;

                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨完成.命令切換.中止] Load End Stop And Clear.[{Vehicle.TransferCommand.CommandId}]");

                        return;
                    }
                }

                if (Vehicle.mapTransferCommands.Count > 1)
                {
                    bool isEqLoad = string.IsNullOrEmpty(Vehicle.MapInfo.addressMap[Vehicle.TransferCommand.LoadAddressId].AgvStationId);

                    var minDis = DistanceFromLastPosition(Vehicle.TransferCommand.UnloadAddressId);

                    bool foundNextCommand = false;

                    foreach (var transferCommand in transferCommands)
                    {
                        string targetAddressId = "";
                        if (transferCommand.EnrouteState == CommandState.LoadEnroute)
                        {
                            transferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                            targetAddressId = transferCommand.LoadAddressId;
                        }
                        else
                        {
                            transferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                            targetAddressId = transferCommand.UnloadAddressId;
                        }

                        bool isTransferCommandToEq = string.IsNullOrEmpty(Vehicle.MapInfo.addressMap[targetAddressId].AgvStationId);

                        if (isTransferCommandToEq == isEqLoad)
                        {
                            if (targetAddressId == Vehicle.MoveStatus.LastAddress.Id)
                            {
                                Vehicle.TransferCommand = transferCommand;

                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨完成.命令切換] Load End Select Another Transfer Command.[{Vehicle.TransferCommand.CommandId}]");

                                foundNextCommand = true;

                                break;
                            }

                            var disTransferCommand = DistanceFromLastPosition(targetAddressId);

                            if (disTransferCommand < minDis)
                            {
                                minDis = disTransferCommand;
                                Vehicle.TransferCommand = transferCommand;

                                foundNextCommand = true;

                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨完成.命令切換] Load End Select Another Transfer Command.[{Vehicle.TransferCommand.CommandId}]");
                            }
                        }
                    }
                    if (!foundNextCommand)
                    {
                        foreach (var transferCommand in transferCommands)
                        {
                            string targetAddressId = "";
                            if (transferCommand.EnrouteState == CommandState.LoadEnroute)
                            {
                                transferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                                targetAddressId = transferCommand.LoadAddressId;
                            }
                            else
                            {
                                transferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                                targetAddressId = transferCommand.UnloadAddressId;
                            }

                            bool isTransferCommandToEq = string.IsNullOrEmpty(Vehicle.MapInfo.addressMap[targetAddressId].AgvStationId);

                            if (targetAddressId == Vehicle.MoveStatus.LastAddress.Id)
                            {
                                Vehicle.TransferCommand = transferCommand;

                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨完成.命令切換] Load End Select Another Transfer Command.[{Vehicle.TransferCommand.CommandId}]");

                                break;
                            }

                            var disTransferCommand = DistanceFromLastPosition(targetAddressId);

                            if (disTransferCommand < minDis)
                            {
                                minDis = disTransferCommand;
                                Vehicle.TransferCommand = transferCommand;

                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨完成.命令切換] Load End Select Another Transfer Command.[{Vehicle.TransferCommand.CommandId}]");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private int DistanceFromLastPosition(string addressId)
        {
            var lastPosition = Vehicle.MoveStatus.LastMapPosition;
            var addressPosition = Vehicle.MapInfo.addressMap[addressId].Position;
            return (int)mapHandler.GetDistance(lastPosition, addressPosition);
        }

        private void UnloadArrival()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Unload Arrival. [AddressId = {Vehicle.MoveStatus.LastAddress.Id}]");

                Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitUnloadArrivalReply;
                Vehicle.TransferCommand.IsUnloadArrivalReply = false;
                agvcConnector.ReportUnloadArrival();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void Unload()
        {
            try
            {
                if (Vehicle.TransferCommand.IsStopAndClear) return;

                Vehicle.TransferCommand.IsRobotEnd = false;
                Vehicle.TransferCommand.TransferStep = EnumTransferStep.UnloadWaitEnd;

                switch (Vehicle.TransferCommand.SlotNumber)
                {
                    case EnumSlotNumber.L:
                        if (Vehicle.CarrierSlotStatus.EnumCarrierSlotState == EnumCarrierSlotState.Empty)
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放貨前.檢查.失敗] Pre Unload Check Fail. Slot is Empty.");

                            AlarmHandler.SetAlarmFromAgvm(000017);
                            return;
                        }
                        break;
                        //case EnumSlotNumber.R:
                        //    if (Vehicle.CarrierSlotRight.EnumCarrierSlotState == EnumCarrierSlotState.Empty)
                        //    {
                        //        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放貨前.檢查.失敗] Pre Unload Check Fail. Slot is Empty.");

                        //        SetAlarmFromAgvm(000017);
                        //        return;
                        //    }
                        //    break;
                }


                UnloadCmdInfo unloadCmd = GetUnloadCommand();
                if (Vehicle.MoveStatus.LastAddress.Id != Vehicle.TransferCommand.UnloadAddressId)
                {
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                    return;
                }

                //if (unloadCmd.PortNumber == "9")
                //{
                //    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                //    return;
                //}

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[執行.放貨] : Unloading,[Unload Adr={unloadCmd.PortAddressId}]");
                agvcConnector.Unloading();

                RobotHandler.DoRobotCommandFor(unloadCmd);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private UnloadCmdInfo GetUnloadCommand()
        {
            try
            {
                //MapAddress portAddress = Vehicle.Mapinfo.addressMap[Vehicle.TransferCommand.UnloadAddressId];
                UnloadCmdInfo robotCommand = new UnloadCmdInfo(Vehicle.TransferCommand);
                //robotCommand.PioDirection = portAddress.PioDirection;
                //robotCommand.GateType = portAddress.GateType;
                //string portId = Vehicle.TransferCommand.UnloadPortId.Trim();

                //if (!Vehicle.Mapinfo.portMap.ContainsKey(portId))
                //{
                //    robotCommand.PortNumber = "1";
                //}
                //else
                //{
                //    var port = Vehicle.Mapinfo.portMap[portId];
                //    if (port.IsVitualPort)
                //    {
                //        robotCommand.PortNumber = "9";
                //    }
                //    else
                //    {
                //        robotCommand.PortNumber = port.Number;
                //        Vehicle.TransferCommand.UnloadAddressId = port.ReferenceAddressId;
                //    }
                //}

                return robotCommand;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return new UnloadCmdInfo(Vehicle.TransferCommand);
            }
        }

        private void UnloadComplete()
        {
            try
            {
                var slotNumber = Vehicle.TransferCommand.SlotNumber;
                CarrierSlotStatus aseCarrierSlotStatus = Vehicle.GetCarrierSlotStatusFrom(slotNumber);

                switch (aseCarrierSlotStatus.EnumCarrierSlotState)
                {
                    case EnumCarrierSlotState.Empty:
                        {
                            Vehicle.TransferCommand.EnrouteState = CommandState.None;
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitUnloadCompleteReply;
                            Vehicle.TransferCommand.IsUnloadCompleteReply = false;
                            agvcConnector.UnloadComplete();
                        }
                        break;
                    case EnumCarrierSlotState.Loading:
                    case EnumCarrierSlotState.ReadFail:
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放貨.失敗] :[{slotNumber}][{aseCarrierSlotStatus.EnumCarrierSlotState}].");
                            Vehicle.TransferCommand.IsStopAndClear = true;
                        }
                        break;
                    case EnumCarrierSlotState.PositionError:
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放貨.失敗.凸片] : PositionError.");
                            AlarmHandler.SetAlarmFromAgvm(51);
                            Vehicle.TransferCommand.EnrouteState = CommandState.None;
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.RobotFail;
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void UnloadEnd()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令完成 {Vehicle.TransferCommand.AgvcTransCommandType}] TransferComplete.");

                Vehicle.TransferCommand.TransferStep = EnumTransferStep.TransferComplete;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AgvcConnector_OnSendRecvTimeoutEvent(object sender, EventArgs e)
        {
            // SetAlarmFromAgvm(38);
        }

        private void AgvcConnector_OnCstRenameEvent(object sender, EnumSlotNumber slotNumber)
        {
            try
            {
                CarrierSlotStatus slotStatus = Vehicle.GetCarrierSlotStatusFrom(slotNumber);
                RobotHandler.SetCarrierSlotStatusTo(slotStatus);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void RobotHandler_OnUpdateRobotStatusEvent(object sender, RobotStatus robotStatus)
        {
            try
            {
                Vehicle.RobotStatus = robotStatus;

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[手臂.狀態.改變] UpdateRobotStatus:[{Vehicle.RobotStatus.EnumRobotState}][RobotHome={Vehicle.RobotStatus.IsHome}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void RobotHandler_OnUpdateCarrierSlotStatusEvent(object sender, CarrierSlotStatus slotStatus)
        {
            try
            {
                if (slotStatus.ManualDeleteCST)
                {
                    slotStatus.CarrierId = "";
                    slotStatus.EnumCarrierSlotState = EnumCarrierSlotState.Empty;
                    switch (slotStatus.SlotNumber)
                    {
                        case EnumSlotNumber.L:
                            Vehicle.CarrierSlotStatus = slotStatus;
                            break;
                            //case EnumSlotNumber.R:
                            //    Vehicle.CarrierSlotRight = slotStatus;
                            //    break;
                    }

                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[手動.清空.儲位.狀態] OnUpdateCarrierSlotStatus: ManualDeleteCST[{slotStatus.SlotNumber}][{slotStatus.EnumCarrierSlotState}][ID={slotStatus.CarrierId}]");

                    agvcConnector.Send_Cmd136_CstRemove(slotStatus.SlotNumber);
                }
                else
                {
                    switch (slotStatus.SlotNumber)
                    {
                        case EnumSlotNumber.L:
                            Vehicle.CarrierSlotStatus = slotStatus;
                            break;
                            //case EnumSlotNumber.R:
                            //    Vehicle.CarrierSlotRight = slotStatus;
                            //    break;
                    }

                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[儲位.狀態.改變] OnUpdateCarrierSlotStatus:[{slotStatus.SlotNumber}][{slotStatus.EnumCarrierSlotState}][ID={slotStatus.CarrierId}]");
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

            agvcConnector.StatusChangeReport();
        }

        private void RobotHandler_OnRobotEndEvent(object sender, EnumRobotEndType robotEndType)
        {
            try
            {
                if (IsStopChargTimeoutInRobotStep)
                {
                    IsStopChargTimeoutInRobotStep = false;
                    AlarmHandler.SetAlarmFromAgvm(14);
                }

                switch (robotEndType)
                {
                    case EnumRobotEndType.Finished:
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[手臂.命令.完成] AseRobotContorl_OnRobotCommandFinishEvent");

                            Vehicle.TransferCommand.IsRobotEnd = true;
                        }
                        break;
                    case EnumRobotEndType.InterlockError:
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[手臂.交握.失敗] AseRobotControl_OnRobotInterlockErrorEvent");
                            AlarmHandler.ResetAllAlarmsFromAgvm();

                            Vehicle.TransferCommand.CompleteStatus = CompleteStatus.CmpStatusInterlockError;
                            Vehicle.TransferCommand.IsStopAndClear = true;
                        }
                        break;
                    case EnumRobotEndType.RobotError:
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[手臂.命令.失敗] AseRobotControl_OnRobotCommandErrorEvent");
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.RobotFail;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region Handle Transfer Command

        private void ClearTransferTransferCommand()
        {
            Vehicle.TransferCommand.IsStopAndClear = false;

            if (Vehicle.TransferCommand.TransferStep == EnumTransferStep.Idle) return;

            Vehicle.TransferCommand.TransferStep = EnumTransferStep.TransferComplete;
            Vehicle.MovingGuide = new MovingGuide();

            if (!Vehicle.TransferCommand.IsAbortByAgvc())
            {
                Vehicle.TransferCommand.CompleteStatus = CompleteStatus.CmpStatusVehicleAbort;
            }

            TransferCommandComplete();
        }

        private void Idle()
        {
            if (Vehicle.mapTransferCommands.IsEmpty)
            {
                if (Vehicle.ActionStatus == VHActionStatus.Commanding)
                {
                    agvcConnector.NoCommand();
                }
            }
            else
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[發呆.結束.選擇.命令] Idle Pick Command To Do.");

                Vehicle.TransferCommand = Vehicle.mapTransferCommands.Values.ToArray()[0];
            }
        }

        private void TransferCommandComplete()
        {
            try
            {
                WaitingTransferCompleteEnd = true;

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令.結束] TransferComplete. [CommandId = {Vehicle.TransferCommand.CommandId}][CompleteStatus = {Vehicle.TransferCommand.CompleteStatus}]");

                if (AlarmHandler.HasHappeningAlarm())
                {
                    AlarmHandler.ResetAllAlarmsFromAgvm();
                }
                Vehicle.mapTransferCommands.TryRemove(Vehicle.TransferCommand.CommandId, out AgvcTransferCommand transferCommand);
                agvcConnector.TransferComplete(transferCommand);

                TransferCompleteOptimize();

                if (Vehicle.mapTransferCommands.IsEmpty)
                {
                    Vehicle.ResetPauseFlags();

                    agvcConnector.NoCommand();
                }
                else
                {
                    agvcConnector.StatusChangeReport();
                }

                WaitingTransferCompleteEnd = false;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void TransferCompleteOptimize()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令完成.命令選擇] TransferCompleteOptimize");

                bool isEqEnd = string.IsNullOrEmpty(Vehicle.MoveStatus.LastAddress.AgvStationId);

                if (!Vehicle.mapTransferCommands.IsEmpty)
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令選擇.命令選取] Transfer Complete Select Another Transfer Command.");

                    if (Vehicle.mapTransferCommands.Count == 1)
                    {
                        Vehicle.TransferCommand = Vehicle.mapTransferCommands.Values.ToArray()[0];
                    }

                    if (Vehicle.mapTransferCommands.Count > 1)
                    {
                        var minDis = 999999;

                        var transferCommands = Vehicle.mapTransferCommands.Values.ToArray();
                        foreach (var transferCommand in transferCommands)
                        {
                            if (transferCommand.IsStopAndClear)
                            {
                                Vehicle.TransferCommand = transferCommand;

                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令完成.命令選擇.命令切換] Transfer Complete Select Another Transfer Command.[{Vehicle.TransferCommand.CommandId}]");

                                return;
                            }
                        }

                        bool foundNextCommand = false;
                        foreach (var transferCommand in transferCommands)
                        {
                            string targetAddressId = "";

                            if (transferCommand.EnrouteState == CommandState.LoadEnroute)
                            {
                                transferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                                targetAddressId = transferCommand.LoadAddressId;
                            }
                            else
                            {
                                transferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                                targetAddressId = transferCommand.UnloadAddressId;
                            }
                            bool isTransferCommandToEq = string.IsNullOrEmpty(Vehicle.MapInfo.addressMap[targetAddressId].AgvStationId); ;

                            if (isTransferCommandToEq == isEqEnd)
                            {
                                if (targetAddressId == Vehicle.MoveStatus.LastAddress.Id)
                                {
                                    Vehicle.TransferCommand = transferCommand;
                                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令完成.命令選擇.命令切換] Transfer Complete Select Another Transfer Command.[{Vehicle.TransferCommand.CommandId}]");
                                    foundNextCommand = true;
                                    break;
                                }

                                var disTransferCommand = DistanceFromLastPosition(targetAddressId);

                                if (disTransferCommand < minDis)
                                {
                                    minDis = disTransferCommand;
                                    Vehicle.TransferCommand = transferCommand;
                                    foundNextCommand = true;
                                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令完成.命令選擇.命令切換] Transfer Complete Select Another Transfer Command.[{Vehicle.TransferCommand.CommandId}]");
                                }

                            }
                        }

                        if (!foundNextCommand)
                        {
                            foreach (var transferCommand in transferCommands)
                            {
                                string targetAddressId = "";

                                if (transferCommand.EnrouteState == CommandState.LoadEnroute)
                                {
                                    transferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                                    targetAddressId = transferCommand.LoadAddressId;
                                }
                                else
                                {
                                    transferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                                    targetAddressId = transferCommand.UnloadAddressId;
                                }
                                bool isTransferCommandToEq = string.IsNullOrEmpty(Vehicle.MapInfo.addressMap[targetAddressId].AgvStationId);

                                if (targetAddressId == Vehicle.MoveStatus.LastAddress.Id)
                                {
                                    Vehicle.TransferCommand = transferCommand;
                                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令完成.命令選擇.命令切換] Transfer Complete Select Another Transfer Command.[{Vehicle.TransferCommand.CommandId}]");
                                    foundNextCommand = true;
                                    break;
                                }

                                var disTransferCommand = DistanceFromLastPosition(targetAddressId);

                                if (disTransferCommand < minDis)
                                {
                                    minDis = disTransferCommand;
                                    Vehicle.TransferCommand = transferCommand;
                                    foundNextCommand = true;
                                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令完成.命令選擇.命令切換] Transfer Complete Select Another Transfer Command.[{Vehicle.TransferCommand.CommandId}]");
                                }
                            }
                        }
                    }
                }
                else
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令完成.命令選擇.無命令] Transfer Complete into Idle.");
                    Vehicle.TransferCommand = new AgvcTransferCommand();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void StopClearAndReset()
        {
            PauseTransfer();

            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[停止.重置] Stop.Clear.Reset.");

                Thread.Sleep(500);
                agvcConnector.ClearAllReserve();

                if (Vehicle.CarrierSlotStatus.EnumCarrierSlotState == EnumCarrierSlotState.Loading /*|| Vehicle.CarrierSlotRight.EnumCarrierSlotState == EnumCarrierSlotState.Loading*/)
                {
                    RobotHandler.GetRobotAndCarrierSlotStatus();
                }

                foreach (var transCmd in Vehicle.mapTransferCommands.Values.ToList())
                {
                    transCmd.IsStopAndClear = true;
                }

                Vehicle.TransferCommand.IsStopAndClear = true;

                StopVehicle();

                agvcConnector.StatusChangeReport();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

            ResumeTransfer();
        }

        private void PauseTransfer()
        {
            agvcConnector.PauseAskReserve();
            PauseVisitTransferSteps();
        }

        private void ResumeTransfer()
        {
            ResumeVisitTransferSteps();
            agvcConnector.ResumeAskReserve();
        }

        private static object installTransferCommandLocker = new object();

        private void AgvcConnector_OnInstallTransferCommandEvent(object sender, AgvcTransferCommand transferCommand)
        {
            lock (installTransferCommandLocker)
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[檢查搬送命令] Check Transfer Command [{transferCommand.CommandId}]");

                #region 檢查搬送Command
                try
                {
                    if (transferCommand.AgvcTransCommandType == EnumAgvcTransCommandType.Override)
                    {
                        AgvcConnector_OnOverrideCommandEvent(sender, transferCommand);
                        return;
                    }

                    switch (transferCommand.AgvcTransCommandType)
                    {
                        case EnumAgvcTransCommandType.Move:
                        case EnumAgvcTransCommandType.MoveToCharger:
                            CheckVehicleTransferCommandMapEmpty();
                            CheckMoveEndAddress(transferCommand.UnloadAddressId);
                            CheckPathToEnd(transferCommand.ToUnloadSectionIds, transferCommand.ToUnloadAddressIds, transferCommand.UnloadAddressId);
                            break;
                        case EnumAgvcTransCommandType.Load:
                            CheckRobotPortAddress(transferCommand.LoadAddressId, transferCommand.LoadPortId);
                            CheckCstIdDuplicate(transferCommand.CassetteId);
                            CheckVehicleTransferCommandMapEmpty();
                            CheckPathToEnd(transferCommand.ToLoadSectionIds, transferCommand.ToLoadAddressIds, transferCommand.LoadAddressId);
                            break;
                        case EnumAgvcTransCommandType.Unload:
                            CheckRobotPortAddress(transferCommand.UnloadAddressId, transferCommand.UnloadPortId);
                            transferCommand.SlotNumber = CheckUnloadCstId(transferCommand.CassetteId);
                            CheckVehicleTransferCommandMapEmpty();
                            CheckPathToEnd(transferCommand.ToUnloadSectionIds, transferCommand.ToUnloadAddressIds, transferCommand.UnloadAddressId);
                            break;
                        case EnumAgvcTransCommandType.LoadUnload:
                            CheckRobotPortAddress(transferCommand.LoadAddressId, transferCommand.LoadPortId);
                            CheckRobotPortAddress(transferCommand.UnloadAddressId, transferCommand.UnloadPortId);
                            CheckCstIdDuplicate(transferCommand.CassetteId);
                            CheckVehicleTransferCommandMapEmpty();
                            CheckPathToEnd(transferCommand.ToLoadSectionIds, transferCommand.ToLoadAddressIds, transferCommand.LoadAddressId);
                            CheckPathToEnd(transferCommand.ToUnloadSectionIds, transferCommand.ToUnloadAddressIds, transferCommand.UnloadAddressId);
                            break;
                        case EnumAgvcTransCommandType.Else:
                            break;
                        default:
                            break;
                    }

                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[檢查搬送命令 成功] Check Transfer Command Ok. [{transferCommand.CommandId}]");

                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                    agvcConnector.ReplyTransferCommand(transferCommand.CommandId, transferCommand.GetCommandActionType(), transferCommand.SeqNum, (int)EnumAgvcReplyCode.Reject, ex.Message);
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[檢查搬送命令 失敗] Check Transfer Command Fail. [{transferCommand.CommandId}] {ex.Message}");
                    return;
                }
                #endregion

                #region 搬送流程更新
                try
                {
                    var isMapTransferCommandsEmpty = Vehicle.mapTransferCommands.IsEmpty;
                    Vehicle.mapTransferCommands.TryAdd(transferCommand.CommandId, transferCommand);
                    agvcConnector.ReplyTransferCommand(transferCommand.CommandId, transferCommand.GetCommandActionType(), transferCommand.SeqNum, (int)EnumAgvcReplyCode.Accept, "");
                    if (isMapTransferCommandsEmpty) agvcConnector.Commanding();
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[初始化搬送命令 成功] Initial Transfer Command Ok. [{transferCommand.CommandId}]");
                }
                catch (Exception ex)
                {
                    agvcConnector.ReplyTransferCommand(transferCommand.CommandId, transferCommand.GetCommandActionType(), transferCommand.SeqNum, (int)EnumAgvcReplyCode.Reject, "");
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[初始化搬送命令 失敗] Initial Transfer Command Fail. [{transferCommand.CommandId}]");
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }
                #endregion
            }
        }

        private void CheckVehicleTransferCommandMapEmpty()
        {
            if (WaitingTransferCompleteEnd)
            {
                throw new Exception("Vehicle is waiting last transfer commmand end.");
            }

            if (!Vehicle.mapTransferCommands.IsEmpty)
            {
                throw new Exception("Vehicle transfer command map is not empty.");
            }
        }

        private void CheckCstIdDuplicate(string cassetteId)
        {
            var agvcTransCmdBuffer = Vehicle.mapTransferCommands.Values.ToList();
            for (int i = 0; i < agvcTransCmdBuffer.Count; i++)
            {
                if (agvcTransCmdBuffer[i].CassetteId == cassetteId)
                {
                    throw new Exception("Transfer command casette ID duplicate.");
                }
            }
        }

        private void CheckTransferCommandMap(AgvcTransferCommand transferCommand)
        {
            if (Vehicle.mapTransferCommands.Any(x => IsMoveTransferCommand(x.Value.AgvcTransCommandType)))
            {
                throw new Exception("Vehicle has move command, can not do loadunload.");
            }

            if (Vehicle.MainFlowConfig.SlotDisable == EnumSlotSelect.Both)
            {
                throw new Exception($"Vehicle has no empty slot to transfer cst. Left = Disable, Right = Disable.");
            }



            //int existEnroute = 0;
            //foreach (var item in Vehicle.mapTransferCommands.Values.ToArray())
            //{
            //    if (item.EnrouteState == transferCommand.EnrouteState)
            //    {
            //        existEnroute++;
            //    }
            //}
            //if (existEnroute > 1)
            //{
            //    throw new Exception($"Vehicle has no enough slot to transfer. ExistEnroute[{existEnroute}]");
            //}
        }

        private bool IsMoveTransferCommand(EnumAgvcTransCommandType agvcTransCommandType)
        {
            return agvcTransCommandType == EnumAgvcTransCommandType.Move || agvcTransCommandType == EnumAgvcTransCommandType.MoveToCharger;
        }

        private EnumSlotNumber CheckUnloadCstId(string cassetteId)
        {
            if (Vehicle.mapTransferCommands.Any(x => IsMoveTransferCommand(x.Value.AgvcTransCommandType)))
            {
                throw new Exception("Vehicle has move command, can not do loadunload.");
            }
            if (string.IsNullOrEmpty(cassetteId))
            {
                throw new Exception($"Unload CST ID is Empty.");
            }
            if (Vehicle.CarrierSlotStatus.CarrierId.Trim() == cassetteId)
            {
                return EnumSlotNumber.L;
            }
            //else if (Vehicle.CarrierSlotRight.CarrierId.Trim() == cassetteId)
            //{
            //    return EnumSlotNumber.R;
            //}
            else
            {
                throw new Exception($"No [{cassetteId}] to unload.");
            }
        }

        private void CheckOverrideAddress(AgvcTransferCommand transferCommand)
        {
            return;
        }

        private void CheckRobotPortAddress(string portAddressId, string portId)
        {
            CheckMoveEndAddress(portAddressId);
            MapAddress portAddress = Vehicle.MapInfo.addressMap[portAddressId];
            if (!portAddress.IsTransferPort())
            {
                throw new Exception($"{portAddressId} can not unload.");
            }

            if (Vehicle.MapInfo.portMap.ContainsKey(portId))
            {
                var port = Vehicle.MapInfo.portMap[portId];
                if (port.ReferenceAddressId != portAddressId)
                {
                    throw new Exception($"{portAddressId} unmatch {portId}.");
                }
            }
        }

        private void CheckMoveEndAddress(string unloadAddressId)
        {
            if (!Vehicle.MapInfo.addressMap.ContainsKey(unloadAddressId))
            {
                throw new Exception($"{unloadAddressId} is not in the map.");
            }
        }

        private void AgvcConnector_OnOverrideCommandEvent(object sender, AgvcTransferCommand transferCommand)
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[檢查替代命令] Check Override Transfer Command [{transferCommand.CommandId}]");

                #region 替代路徑檢查
                try
                {
                    if (Vehicle.TransferCommand.IsCheckingAvoid)
                    {
                        throw new Exception($"Vehicle is checking avoid request, cant not override.");
                    }
                    else
                    {
                        Vehicle.TransferCommand.IsCheckingOverride = true;
                    }

                    if (Vehicle.TransferCommand.TransferStep == EnumTransferStep.Idle)
                    {
                        throw new Exception($"Vehicle is idle, cant not override.");
                    }

                    if (Vehicle.TransferCommand.TransferStep != EnumTransferStep.MoveToAddressWaitEnd && Vehicle.TransferCommand.TransferStep != EnumTransferStep.WaitOverrideToContinue)
                    {
                        throw new Exception("Vehicle is not in moving step or waiting for override, cant not override.");
                    }

                    if (!(Vehicle.MovingGuide.ReserveStop == VhStopSingle.StopSingleOn || Vehicle.MovingGuide.IsAvoidComplete))
                    {
                        throw new Exception($"Vehicle is not in reserve-stop, cant not override.");
                    }

                    if (Vehicle.TransferCommand.EnrouteState == CommandState.LoadEnroute)
                    {
                        CheckPathToEnd(transferCommand.ToLoadSectionIds, transferCommand.ToLoadAddressIds, transferCommand.LoadAddressId);

                        if (Vehicle.TransferCommand.LoadAddressId != transferCommand.LoadAddressId)
                        {
                            throw new Exception($"Load address id check fail, cant not override.");
                        }

                        if (Vehicle.TransferCommand.AgvcTransCommandType == EnumAgvcTransCommandType.LoadUnload)
                        {
                            CheckPathToEnd(transferCommand.ToUnloadSectionIds, transferCommand.ToUnloadAddressIds, transferCommand.UnloadAddressId);

                            if (Vehicle.TransferCommand.UnloadAddressId != transferCommand.UnloadAddressId)
                            {
                                throw new Exception($"Unload address id check fail, cant not override.");
                            }
                        }
                    }
                    else
                    {
                        CheckLoadInfoIsEmpty(transferCommand);

                        CheckPathToEnd(transferCommand.ToUnloadSectionIds, transferCommand.ToUnloadAddressIds, transferCommand.UnloadAddressId);

                        if (Vehicle.TransferCommand.UnloadAddressId != transferCommand.UnloadAddressId)
                        {
                            throw new Exception($"Unload address id check fail, cant not override.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    agvcConnector.ReplyTransferCommand(transferCommand.CommandId, transferCommand.GetCommandActionType(), transferCommand.SeqNum, (int)EnumAgvcReplyCode.Reject, ex.Message);
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                    Vehicle.TransferCommand.IsCheckingOverride = false;
                    return;
                }
                #endregion

                #region 替代路徑生成
                try
                {
                    PauseVisitTransferSteps();
                    agvcConnector.ClearAllReserve();
                    IsNotAvoid();
                    OverrideTransferCommand(transferCommand);
                    agvcConnector.ReplyTransferCommand(transferCommand.CommandId, transferCommand.GetCommandActionType(), transferCommand.SeqNum, (int)EnumAgvcReplyCode.Accept, "");
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Accept override transfer command, id = [{transferCommand.CommandId}].");
                    ResumeVisitTransferSteps();
                }
                catch (Exception ex)
                {
                    StopClearAndReset();
                    agvcConnector.ReplyTransferCommand(transferCommand.CommandId, transferCommand.GetCommandActionType(), transferCommand.SeqNum, (int)EnumAgvcReplyCode.Reject, ex.Message);
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Override initial fail. reason = {ex.Message}");
                }

                Vehicle.TransferCommand.IsCheckingOverride = false;


                #endregion

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void IsNotAvoid()
        {
            Vehicle.MovingGuide.IsAvoidMove = false;
            Vehicle.MovingGuide.IsAvoidComplete = false;
        }

        private void OverrideTransferCommand(AgvcTransferCommand transferCommand)
        {
            switch (Vehicle.TransferCommand.EnrouteState)
            {
                case CommandState.LoadEnroute:
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                    OverrideTransferCommandToLoad(transferCommand);
                    if (Vehicle.TransferCommand.AgvcTransCommandType == EnumAgvcTransCommandType.LoadUnload)
                    {
                        OverrideTransferCommandToUnload(transferCommand);
                    }
                    break;
                case CommandState.UnloadEnroute:
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                    OverrideTransferCommandToUnload(transferCommand);
                    break;
                case CommandState.None:
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAddress;
                    OverrideTransferCommandToUnload(transferCommand);
                    break;
            }
        }

        private void OverrideTransferCommandToUnload(AgvcTransferCommand transferCommand)
        {
            Vehicle.TransferCommand.ToUnloadSectionIds = transferCommand.ToUnloadSectionIds;
            Vehicle.TransferCommand.ToUnloadAddressIds = transferCommand.ToUnloadAddressIds;
        }

        private void OverrideTransferCommandToLoad(AgvcTransferCommand transferCommand)
        {
            Vehicle.TransferCommand.ToLoadSectionIds = transferCommand.ToLoadSectionIds;
            Vehicle.TransferCommand.ToLoadAddressIds = transferCommand.ToLoadAddressIds;
        }

        private void CheckLoadInfoIsEmpty(AgvcTransferCommand transferCommand)
        {
            if (transferCommand.ToLoadSectionIds.Any())
            {
                throw new Exception($"Load section list is not empty.");
            }

            if (transferCommand.ToLoadAddressIds.Any())
            {
                throw new Exception($"Load address list is not empty.");
            }

            if (!string.IsNullOrEmpty(transferCommand.LoadAddressId))
            {
                throw new Exception($"Load port address is not empty.");
            }
        }

        private void CheckPathToEnd(List<string> sectionIds, List<string> addressIds, string endAddressId)
        {
            if (sectionIds.Count == 0)
            {
                throw new Exception($"Section list is empty.");
            }

            if (addressIds.Count == 0)
            {
                throw new Exception($"Address list is empty.");
            }

            for (int i = 0; i < sectionIds.Count; i++)
            {
                if (!Vehicle.MapInfo.sectionMap.ContainsKey(sectionIds[i]))
                {
                    throw new Exception($"{sectionIds[i]} is not in the map.");
                }

                if (!Vehicle.MapInfo.addressMap.ContainsKey(addressIds[i]))
                {
                    throw new Exception($"{addressIds[i]} is not in the map.");
                }

                MapSection mapSection = Vehicle.MapInfo.sectionMap[sectionIds[i]];
                if (!mapSection.InSection(addressIds[i]))
                {
                    throw new Exception($"{addressIds[i]} is not in {sectionIds[i]}.");
                }
            }

            if (!Vehicle.MapInfo.addressMap.ContainsKey(endAddressId))
            {
                throw new Exception($"{endAddressId} is not in the map.");
            }

            MapSection endSection = Vehicle.MapInfo.sectionMap[sectionIds[sectionIds.Count - 1]];
            if (!endSection.InSection(endAddressId))
            {
                throw new Exception($"{endAddressId} is not in {endSection.Id}.");
            }
        }

        private void AgvcConnector_OnAvoideRequestEvent(object sender, MovingGuide aseMovingGuide)
        {
            #region 避車檢查
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"MainFlow :  Get Avoid Command, End Adr=[{aseMovingGuide.ToAddressId}],  start check .");

                agvcConnector.PauseAskReserve();

                if (Vehicle.TransferCommand.IsCheckingOverride)
                {
                    throw new Exception($"Vehicle is checking avoid request, cant not override.");
                }
                else
                {
                    Vehicle.TransferCommand.IsCheckingAvoid = true;
                }

                if (Vehicle.mapTransferCommands.IsEmpty)
                {
                    throw new Exception("Vehicle has no Command, can not Avoid");
                }

                if (!IsMoveStep() || Vehicle.TransferCommand.TransferStep == EnumTransferStep.WaitOverrideToContinue)
                {
                    throw new Exception("Vehicle is not moving, can not Avoid");
                }

                if (!IsMoveStopByNoReserve() && !Vehicle.MovingGuide.IsAvoidComplete)
                {
                    throw new Exception($"Vehicle is not stop by no reserve, can not Avoid");
                }

                CheckPathToEnd(aseMovingGuide.GuideSectionIds, aseMovingGuide.GuideAddressIds, aseMovingGuide.ToAddressId);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                RejectAvoidCommandAndResume(000036, ex.Message, aseMovingGuide);
            }
            #endregion

            #region 避車Command生成
            try
            {
                Vehicle.MovingGuide.IsAvoidMove = true;
                agvcConnector.ClearAllReserve();
                Vehicle.MovingGuide = aseMovingGuide;
                SetupMovingGuideMovingSections();
                agvcConnector.SetupNeedReserveSections();
                agvcConnector.StatusChangeReport();
                agvcConnector.ReplyAvoidCommand(aseMovingGuide.SeqNum, 0, "");
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"MainFlow : Get 避車Command checked , 終點[{aseMovingGuide.ToAddressId}].");
                agvcConnector.ResumeAskReserve();
            }
            catch (Exception ex)
            {
                StopClearAndReset();
                RejectAvoidCommandAndResume(000036, "避車Exception", aseMovingGuide);
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

            #endregion
        }

        public bool IsMoveStep()
        {
            return Vehicle.TransferCommand.TransferStep == EnumTransferStep.MoveToAddressWaitEnd;
        }

        private bool IsMoveStopByNoReserve()
        {
            return Vehicle.MovingGuide.ReserveStop == VhStopSingle.StopSingleOn;
        }

        private void RejectAvoidCommandAndResume(int alarmCode, string reason, MovingGuide aseMovingGuide)
        {
            try
            {
                AlarmHandler.SetAlarmFromAgvm(alarmCode);
                agvcConnector.ReplyAvoidCommand(aseMovingGuide.SeqNum, 1, reason);
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, string.Concat($"MainFlow : Reject Avoid Command, ", reason));
                agvcConnector.ResumeAskReserve();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }


        #endregion

        #endregion

        #region Thd Watch Charge Stage

        private void WatchChargeStage()
        {
            while (true)
            {
                try
                {
                    BatteryHandler.GetBatteryAndChargeStatus();

                    if (Vehicle.AutoState == EnumAutoState.Auto && IsVehicleIdle() && IsLowPower() && !IsLowPowerStartChargeTimeout)
                    {
                        LowPowerStartCharge(Vehicle.MoveStatus.LastAddress);
                    }
                    //if (Vehicle.BatteryStatus.Percentage < Vehicle.MainFlowConfig.HighPowerPercentage - 21 && !Vehicle.IsCharging) //200701 dabid+
                    //{
                    //    SetAlarmFromAgvm(2);
                    //}
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }

                SpinWait.SpinUntil(() => false, Vehicle.MainFlowConfig.WatchLowPowerSleepTimeMs);
            }
        }

        public bool IsVehicleIdle()
        {
            return Vehicle.TransferCommand.TransferStep == EnumTransferStep.Idle;
        }

        public void StartWatchChargeStage()
        {
            thdWatchChargeStage = new Thread(WatchChargeStage);
            thdWatchChargeStage.IsBackground = true;
            thdWatchChargeStage.Start();
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"StartWatchChargeStage");
        }

        public bool IsLowPower()
        {
            return Vehicle.BatteryStatus.Percentage <= Vehicle.MainFlowConfig.HighPowerPercentage;
        }

        private bool IsHighPower()
        {
            return Vehicle.BatteryStatus.Percentage > Vehicle.MainFlowConfig.HighPowerPercentage;
        }

        public void MainFormStartCharge()
        {
            StartCharge(Vehicle.MoveStatus.LastAddress);
        }

        private void StartCharge(MapAddress endAddress)
        {
            try
            {
                IsArrivalCharge = true;
                var percentage = Vehicle.BatteryStatus.Percentage;
                var highPercentage = Vehicle.MainFlowConfig.HighPowerPercentage;

                if (endAddress.IsCharger())
                {
                    if (IsHighPower())
                    {
                        var msg = $"[電量過高.無法充電] High power not start charge.[Precentage = {percentage}] > [Threshold = {highPercentage}][Vehicle arrival {endAddress.Id},Charge Direction = {endAddress.ChargeDirection}].";
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                        return;
                    }

                    agvcConnector.ChargHandshaking();
                    Vehicle.IsCharging = true;

                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $@"[充電.開始.執行] Start Charge, Vehicle arrival {endAddress.Id},Charge Direction = {endAddress.ChargeDirection},Precentage = {percentage}.");

                    Vehicle.CheckStartChargeReplyEnd = false;
                    BatteryHandler.StartCharge(endAddress.ChargeDirection);

                    SpinWait.SpinUntil(() => Vehicle.CheckStartChargeReplyEnd, 30 * 1000);

                    if (Vehicle.CheckStartChargeReplyEnd)
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[充電.成功] Start Charge success.");
                        agvcConnector.Charging();
                        IsLowPowerStartChargeTimeout = false;
                    }
                    else
                    {
                        Vehicle.IsCharging = false;
                        AlarmHandler.SetAlarmFromAgvm(000013);
                        BatteryHandler.GetBatteryAndChargeStatus();

                        SpinWait.SpinUntil(() => false, 500);
                        BatteryHandler.StopCharge();
                    }

                    Vehicle.CheckStartChargeReplyEnd = true;
                }

                IsArrivalCharge = false;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                IsArrivalCharge = false;
            }
        }

        private void LowPowerStartCharge(MapAddress lastAddress)
        {
            try
            {
                if (IsArrivalCharge) return;

                var pos = Vehicle.MoveStatus.LastMapPosition;
                if (lastAddress.IsCharger())
                {
                    if (Vehicle.IsCharging)
                    {
                        return;
                    }
                    else
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[低電量閒置.自動充電] Addr = {lastAddress.Id},Precentage = {Vehicle.BatteryStatus.Percentage} < {Vehicle.MainFlowConfig.HighPowerPercentage}(Threshold).");
                    }

                    if ((DateTime.Now - LowPowerStartChargeTimeStamp).TotalSeconds >= Vehicle.MainFlowConfig.LowPowerRepeatChargeIntervalSec)
                    {
                        LowPowerStartChargeTimeStamp = DateTime.Now;
                        LowPowerRepeatedlyChargeCounter = 0;
                    }
                    else
                    {
                        LowPowerRepeatedlyChargeCounter++;
                        if (LowPowerRepeatedlyChargeCounter > Vehicle.MainFlowConfig.LowPowerRepeatedlyChargeCounterMax)
                        {
                            Task.Run(() =>
                            {
                                IsLowPowerStartChargeTimeout = true;
                                SpinWait.SpinUntil(() => false, Vehicle.MainFlowConfig.SleepLowPowerWatcherSec * 1000);
                                IsLowPowerStartChargeTimeout = false;
                            });
                            return;
                        }
                    }

                    agvcConnector.ChargHandshaking();

                    Vehicle.IsCharging = true;

                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $@"Start Charge, Vehicle arrival {lastAddress.Id},Charge Direction = {lastAddress.ChargeDirection},Precentage = {Vehicle.BatteryStatus.Percentage}.");

                    Vehicle.CheckStartChargeReplyEnd = false;

                    int retryCount = Vehicle.MainFlowConfig.ChargeRetryTimes;

                    for (int i = 0; i < retryCount; i++)
                    {
                        BatteryHandler.StartCharge(lastAddress.ChargeDirection);

                        SpinWait.SpinUntil(() => Vehicle.CheckStartChargeReplyEnd, Vehicle.MainFlowConfig.StartChargeWaitingTimeoutMs);

                        if (Vehicle.CheckStartChargeReplyEnd)
                        {
                            break;
                        }
                    }

                    if (Vehicle.CheckStartChargeReplyEnd)
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Start Charge success.");
                        agvcConnector.Charging();
                    }
                    else
                    {
                        Vehicle.IsCharging = false;
                        AlarmHandler.SetAlarmFromAgvm(000013);
                        BatteryHandler.GetBatteryAndChargeStatus();

                        SpinWait.SpinUntil(() => false, 500);
                        BatteryHandler.StopCharge();
                    }

                    Vehicle.CheckStartChargeReplyEnd = true;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void StopCharge()
        {
            try
            {
                IsStopCharging = true;

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $@"[斷充.開始] Try STOP charge.[IsCharging = {Vehicle.IsCharging}]");

                if (Vehicle.MoveStatus.LastAddress.IsCharger() || Vehicle.IsCharging)
                {
                    agvcConnector.ChargHandshaking();

                    SpinWait.SpinUntil(() => Vehicle.CheckStartChargeReplyEnd, Vehicle.MainFlowConfig.StopChargeWaitingTimeoutMs);

                    int retryCount = Vehicle.MainFlowConfig.DischargeRetryTimes;
                    Vehicle.IsCharging = true;

                    for (int i = 0; i < retryCount; i++)
                    {
                        BatteryHandler.StopCharge();

                        SpinWait.SpinUntil(() => !Vehicle.IsCharging, Vehicle.MainFlowConfig.StopChargeWaitingTimeoutMs);

                        BatteryHandler.GetBatteryAndChargeStatus();

                        SpinWait.SpinUntil(() => false, 500);

                        if (!Vehicle.IsCharging)
                        {
                            break;
                        }
                    }

                    if (!Vehicle.IsCharging)
                    {
                        agvcConnector.ChargeOff();
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[斷充.成功] Stop Charge success.");
                    }
                    else
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[斷充.逾時] Stop Charge Timeout.");
                        if (IsRobotStep())
                        {
                            IsStopChargTimeoutInRobotStep = true;
                        }
                        else
                        {
                            AlarmHandler.SetAlarmFromAgvm(000014);
                        }
                    }
                }
                IsStopCharging = false;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                IsStopCharging = false;
            }
        }

        private bool IsRobotStep()
        {
            switch (Vehicle.TransferCommand.TransferStep)
            {
                case EnumTransferStep.WaitLoadArrivalReply:
                case EnumTransferStep.Load:
                case EnumTransferStep.WaitLoadCompleteReply:
                case EnumTransferStep.WaitCstIdReadReply:
                case EnumTransferStep.UnloadArrival:
                case EnumTransferStep.WaitUnloadArrivalReply:
                case EnumTransferStep.Unload:
                case EnumTransferStep.WaitUnloadCompleteReply:
                case EnumTransferStep.LoadWaitEnd:
                case EnumTransferStep.UnloadWaitEnd:
                case EnumTransferStep.RobotFail:
                    return true;
                default:
                    return false; ;
            }
        }

        private void ArrivalStartCharge(MapAddress endAddress)
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[到站.充電] : ArrivalStartCharge.");
                int chargeTimeSec = -1;
                if (IsMoveEndRobotStep())
                {
                    int sameAddressRobotCommand = 0;
                    foreach (var transferCommand in Vehicle.mapTransferCommands.Values.ToArray())
                    {
                        if (transferCommand.EnrouteState == CommandState.LoadEnroute)
                        {
                            if (transferCommand.LoadAddressId == Vehicle.MoveStatus.LastAddress.Id)
                            {
                                sameAddressRobotCommand++;
                            }
                        }
                        else if (transferCommand.EnrouteState == CommandState.UnloadEnroute)
                        {
                            if (transferCommand.UnloadAddressId == Vehicle.MoveStatus.LastAddress.Id)
                            {
                                sameAddressRobotCommand++;
                            }
                        }
                    }
                    chargeTimeSec = Vehicle.MainFlowConfig.ChargeIntervalInRobotingSec * sameAddressRobotCommand;
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[手臂命令.提早斷充] : ChargeIntervalInRobotingSec [RobotStepCount ={sameAddressRobotCommand}][ChargeTimeSec = {chargeTimeSec}].");
                }

                Task.Run(() =>
                {
                    StartCharge(endAddress);
                    if (chargeTimeSec > 0)
                    {
                        Thread.Sleep(chargeTimeSec * 1000);
                        StopCharge();
                    }
                });
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private bool IsMoveEndRobotStep()
        {
            try
            {
                switch (Vehicle.TransferCommand.AgvcTransCommandType)
                {
                    case EnumAgvcTransCommandType.Load:
                    case EnumAgvcTransCommandType.Unload:
                    case EnumAgvcTransCommandType.LoadUnload:
                        return true;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

            return false;
        }

        private void BatteryHandler_OnUpdateChargeStatusEvent(object sender, bool isCharging)
        {
            try
            {
                Vehicle.IsCharging = isCharging;

                if (isCharging)
                {
                    Vehicle.CheckStartChargeReplyEnd = true;
                }
                else
                {
                    Vehicle.CheckStopChargeReplyEnd = true;
                }

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[充電.狀態.改變] UpdateChargeStatus:[{isCharging}][Percentage={Vehicle.BatteryStatus.Percentage}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

        }

        private void BatteryHandler_OnUpdateBatteryStatusEvent(object sender, BatteryStatus batteryStatus)
        {
            try
            {
                Vehicle.BatteryStatus = batteryStatus;
                Vehicle.BatteryLog.InitialSoc = batteryStatus.Percentage;
                agvcConnector.BatteryHandler_OnBatteryPercentageChangeEvent(sender, batteryStatus.Percentage);

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[電池.狀態.改變] UpdateBatteryStatus:[{Vehicle.IsCharging}][Percentage={batteryStatus.Percentage}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion


        public void StopVehicle()
        {
            MoveHandler.StopMove();
            RobotHandler.ClearRobotCommand();
            BatteryHandler.StopCharge();

            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"MainFlow : Stop Vehicle, [MoveState={Vehicle.MoveStatus.EnumMoveState}][IsCharging={Vehicle.IsCharging}]");
        }

        public void SetupVehicleSoc(int percentage)
        {
            BatteryHandler.SetPercentageTo(percentage);
        }

        private void AgvcConnector_OnRenameCassetteIdEvent(object sender, CarrierSlotStatus e)
        {
            try
            {
                foreach (var transCmd in Vehicle.mapTransferCommands.Values.ToList())
                {
                    if (transCmd.SlotNumber == e.SlotNumber)
                    {
                        transCmd.CassetteId = e.CarrierId;
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void AgvcConnector_OnCmdPauseEvent(ushort iSeqNum, PauseType pauseType)
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[執行.暫停] [{PauseEvent.Pause}][{pauseType}]");

                Vehicle.PauseFlags[pauseType] = true;
                PauseTransfer();
                MoveHandler.PauseMove();
                agvcConnector.PauseReply(iSeqNum, (int)EnumAgvcReplyCode.Accept, PauseEvent.Pause);
                agvcConnector.StatusChangeReport();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void AgvcConnector_OnCmdResumeEvent(ushort iSeqNum, PauseType pauseType)
        {
            try
            {
                if (pauseType == PauseType.All)
                {
                    Vehicle.ResetPauseFlags();
                    ResumeMiddler(iSeqNum, pauseType);
                }
                else
                {
                    Vehicle.PauseFlags[pauseType] = false;
                    agvcConnector.PauseReply(iSeqNum, (int)EnumAgvcReplyCode.Accept, PauseEvent.Continue);

                    if (!Vehicle.IsPause())
                    {
                        ResumeMiddler(iSeqNum, pauseType);
                    }
                    else
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[尚有.其他.暫停旗標] [{PauseEvent.Continue}][{pauseType}]");
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ResumeMiddler(ushort iSeqNum, PauseType pauseType)
        {
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[執行.續行] [{PauseEvent.Continue}][{pauseType}]");

            agvcConnector.PauseReply(iSeqNum, (int)EnumAgvcReplyCode.Accept, PauseEvent.Continue);
            MoveHandler.ResumeMove();
            ResumeTransfer();
            agvcConnector.StatusChangeReport();
        }

        private void ResumeMiddler()
        {
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[執行.續行] By Op Resume.");

            MoveHandler.ResumeMove();
            ResumeTransfer();
            agvcConnector.StatusChangeReport();
        }

        public void AgvcConnector_OnCmdCancelAbortEvent(ushort iSeqNum, ID_37_TRANS_CANCEL_REQUEST receive)
        {
            PauseTransfer();

            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"MainFlow : Get [{receive.ActType}] Command.");

                string abortCmdId = receive.CmdID.Trim();
                bool IsAbortCurCommand = Vehicle.TransferCommand.CommandId == abortCmdId;
                var targetAbortCmd = Vehicle.mapTransferCommands[abortCmdId];

                if (IsAbortCurCommand)
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放棄.當前.命令] TransferComplete [{targetAbortCmd.CompleteStatus}].");

                    agvcConnector.ClearAllReserve();
                    MoveHandler.StopMove();
                    Vehicle.MovingGuide = new MovingGuide();
                    Vehicle.TransferCommand.CompleteStatus = GetCompleteStatusFromCancelRequest(receive.ActType);
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.TransferComplete;
                }
                else
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放棄.背景.命令] TransferComplete [{targetAbortCmd.CompleteStatus}].");

                    WaitingTransferCompleteEnd = true;

                    targetAbortCmd.TransferStep = EnumTransferStep.Abort;
                    targetAbortCmd.CompleteStatus = GetCompleteStatusFromCancelRequest(receive.ActType);

                    Vehicle.mapTransferCommands.TryRemove(Vehicle.TransferCommand.CommandId, out AgvcTransferCommand transferCommand);
                    agvcConnector.TransferComplete(transferCommand);

                    if (Vehicle.mapTransferCommands.IsEmpty)
                    {
                        Vehicle.ResetPauseFlags();

                        agvcConnector.NoCommand();

                        Vehicle.TransferCommand = new AgvcTransferCommand();
                    }
                    else
                    {
                        agvcConnector.StatusChangeReport();
                    }

                    WaitingTransferCompleteEnd = false;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

            ResumeTransfer();
        }

        private CompleteStatus GetCompleteStatusFromCancelRequest(CMDCancelType cancelAction)
        {
            switch (cancelAction)
            {
                case CMDCancelType.CmdCancel:
                    return CompleteStatus.CmpStatusCancel;
                case CMDCancelType.CmdCancelIdMismatch:
                    return CompleteStatus.CmpStatusIdmisMatch;
                case CMDCancelType.CmdCancelIdReadFailed:
                    return CompleteStatus.CmpStatusIdreadFailed;
                case CMDCancelType.CmdNone:
                case CMDCancelType.CmdEms:
                case CMDCancelType.CmdAbort:
                default:
                    return CompleteStatus.CmpStatusAbort;
            }
        }

        public void AgvcDisconnected()
        {
            try
            {
                if (AlarmHandler != null)
                {
                    AlarmHandler.SetAlarmFromAgvm(56);
                    ConnectionModeHandler.AgvcDisconnect();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #region LocalPackage        

        public object _ModeChangeLocker = new object();

        public void ConnectionModeHandler_OnModeChangeEvent(object sender, EnumAutoState autoState)
        {
            try
            {
                lock (_ModeChangeLocker)
                {
                    StopClearAndReset();

                    if (Vehicle.AutoState != autoState)
                    {
                        switch (autoState)
                        {
                            case EnumAutoState.Auto:
                                GetAllStatusReport();
                                AlarmHandler.ResetAllAlarmsFromAgvm();
                                UpdateSlotStatus();
                                Vehicle.MoveStatus.IsMoveEnd = false;
                                break;
                            case EnumAutoState.Manual:
                                break;
                            case EnumAutoState.None:
                                break;
                            default:
                                break;
                        }

                        Vehicle.AutoState = autoState;
                        agvcConnector.StatusChangeReport();

                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Switch to {autoState}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (autoState == EnumAutoState.Auto)
                {
                    AlarmHandler.SetAlarmFromAgvm(31);
                    ConnectionModeHandler.SetAutoState(EnumAutoState.Manual);
                }
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

        }

        private void CheckCanAuto()
        {

            if (Vehicle.MoveStatus.LastSection == null || string.IsNullOrEmpty(Vehicle.MoveStatus.LastSection.Id))
            {
                CanAutoMsg = "Section Lost";
                throw new Exception("CheckCanAuto fail. Section Lost.");
            }
            else if (Vehicle.MoveStatus.LastAddress == null || string.IsNullOrEmpty(Vehicle.MoveStatus.LastAddress.Id))
            {
                CanAutoMsg = "Address Lost";
                throw new Exception("CheckCanAuto fail. Address Lost.");
            }
            else if (Vehicle.MoveStatus.EnumMoveState != EnumMoveState.Idle && Vehicle.MoveStatus.EnumMoveState != EnumMoveState.Block)
            {
                CanAutoMsg = $"Move State = {Vehicle.MoveStatus.EnumMoveState}";
                throw new Exception($"CheckCanAuto fail. {CanAutoMsg}");
            }
            else if (Vehicle.MoveStatus.LastAddress.MyDistance(Vehicle.MoveStatus.LastMapPosition) >= Vehicle.MainFlowConfig.InitialPositionRangeMm)
            {
                AlarmHandler.SetAlarmFromAgvm(54);
                CanAutoMsg = $"Initial Positon Too Far.";
                throw new Exception($"CheckCanAuto fail. {CanAutoMsg}");
            }

            var aseRobotStatus = Vehicle.RobotStatus;
            if (aseRobotStatus.EnumRobotState != EnumRobotState.Idle)
            {
                CanAutoMsg = $"Robot State = {aseRobotStatus.EnumRobotState}";
                throw new Exception($"CheckCanAuto fail. {CanAutoMsg}");
            }
            else if (!aseRobotStatus.IsHome)
            {
                CanAutoMsg = $"Robot IsHome = {aseRobotStatus.IsHome}";
                throw new Exception($"CheckCanAuto fail. {CanAutoMsg}");
            }

            CanAutoMsg = "OK";
        }

        private void UpdateSlotStatus()
        {
            try
            {
                CarrierSlotStatus leftSlotStatus = new CarrierSlotStatus(Vehicle.CarrierSlotStatus);

                switch (leftSlotStatus.EnumCarrierSlotState)
                {
                    case EnumCarrierSlotState.Empty:
                        {
                            leftSlotStatus.CarrierId = "";
                            leftSlotStatus.EnumCarrierSlotState = EnumCarrierSlotState.Empty;
                            Vehicle.CarrierSlotStatus = leftSlotStatus;
                            Vehicle.LeftReadResult = BCRReadResult.BcrNormal;
                        }
                        break;
                    case EnumCarrierSlotState.Loading:
                        {
                            if (string.IsNullOrEmpty(leftSlotStatus.CarrierId.Trim()))
                            {
                                leftSlotStatus.CarrierId = "ReadFail";
                                leftSlotStatus.EnumCarrierSlotState = EnumCarrierSlotState.ReadFail;
                                Vehicle.CarrierSlotStatus = leftSlotStatus;
                                Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                            }
                            else
                            {
                                Vehicle.LeftReadResult = BCRReadResult.BcrNormal;
                            }
                        }
                        break;
                    case EnumCarrierSlotState.PositionError:
                        {
                            AlarmHandler.SetAlarmFromAgvm(51);
                        }
                        return;
                    case EnumCarrierSlotState.ReadFail:
                        {
                            leftSlotStatus.CarrierId = "ReadFail";
                            leftSlotStatus.EnumCarrierSlotState = EnumCarrierSlotState.ReadFail;
                            Vehicle.CarrierSlotStatus = leftSlotStatus;
                            Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                        }
                        break;
                    default:
                        break;
                }


                agvcConnector.CSTStatusReport(); //200625 dabid#
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region Set / Reset Alarm

        private void AlarmHandler_OnResetAlarmToAgvcEvent(object sender, EventArgs e)
        {
            agvcConnector.ResetAllAlarmsToAgvc();
        }

        private void AlarmHandler_OnSetAlarmToAgvcEvent(object sender, Alarms.AgvcAlarmArgs e)
        {
            agvcConnector.SetlAlarmToAgvc(e.ErrorCode, e.IsAlarm);
        }

        #endregion

        #region Log

        private void IMessageHandler_OnLogErrorEvent(object sender, Tools.MessageHandlerArgs e)
        {
            LogException(e.ClassMethodName, e.Message);
        }

        private void IMessageHandler_OnLogDebugEvent(object sender, Tools.MessageHandlerArgs e)
        {
            LogDebug(e.ClassMethodName, e.Message);
        }

        public void AppendDebugLog(string msg)
        {
            try
            {
                lock (DebugLogMsg)
                {
                    DebugLogMsg = string.Concat(DateTime.Now.ToString("HH:mm:ss.fff"), "  ", msg, "\r\n", DebugLogMsg);

                    if (DebugLogMsg.Length > 65535)
                    {
                        DebugLogMsg = DebugLogMsg.Substring(65535);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void LogException(string classMethodName, string exMsg)
        {
            try
            {
                mirleLogger.Log(new LogFormat("MainError", "5", classMethodName, Vehicle.AgvcConnectorConfig.ClientName, "CarrierID", exMsg));
            }
            catch (Exception) { }
        }

        public void LogDebug(string classMethodName, string msg)
        {
            try
            {
                mirleLogger.Log(new LogFormat("MainDebug", "5", classMethodName, Vehicle.AgvcConnectorConfig.ClientName, "CarrierID", msg));
                AppendDebugLog(msg);
            }
            catch (Exception) { }
        }

        #endregion
    }

    public class LastIdlePosition
    {
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public MapPosition Position { get; set; } = new MapPosition();
    }
}