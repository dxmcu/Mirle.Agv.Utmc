using com.mirle.aka.sc.ProtocolFormat.ase.agvMessage;
using Mirle.Agv.Utmc.Model;
using Mirle.Agv.Utmc.Model.Configs;
using Mirle.Agv.Utmc.Model.TransferSteps;
using Mirle.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Controller
{
    public class MainFlowHandler
    {
        #region TransCmds
        public bool IsOverrideMove { get; set; }
        public bool IsAvoidMove { get; set; }
        public bool IsArrivalCharge { get; set; } = false;

        #endregion

        #region Controller

        public AgvcConnector agvcConnector;
        public MirleLogger mirleLogger = null;
        public AlarmHandler alarmHandler;
        public MapHandler mapHandler;
        public LocalPackage localPackage;
        public UserAgent UserAgent { get; set; } = new UserAgent();
        public Robot.IRobotHandler RobotHandler { get; set; }
        public Battery.IBatteryHandler BatteryHandler { get; set; }
        public Move.IMoveHandler MoveHandler { get; set; }

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

        private void ConfigInitial()
        {
            try
            {
                //Main Configs 
                int minThreadSleep = 100;
                string allText = System.IO.File.ReadAllText("MainFlowConfig.json");
                Vehicle.MainFlowConfig = JsonConvert.DeserializeObject<MainFlowConfig>(allText);
                if (Vehicle.MainFlowConfig.IsSimulation)
                {
                    Vehicle.LoginLevel = EnumLoginLevel.Admin;
                }
                Vehicle.MainFlowConfig.VisitTransferStepsSleepTimeMs = Math.Max(Vehicle.MainFlowConfig.VisitTransferStepsSleepTimeMs, minThreadSleep);
                Vehicle.MainFlowConfig.TrackPositionSleepTimeMs = Math.Max(Vehicle.MainFlowConfig.TrackPositionSleepTimeMs, minThreadSleep);
                Vehicle.MainFlowConfig.WatchLowPowerSleepTimeMs = Math.Max(Vehicle.MainFlowConfig.WatchLowPowerSleepTimeMs, minThreadSleep);

                allText = System.IO.File.ReadAllText("MapConfig.json");
                Vehicle.MapConfig = JsonConvert.DeserializeObject<MapConfig>(allText);

                allText = System.IO.File.ReadAllText("AgvcConnectorConfig.json");
                Vehicle.AgvcConnectorConfig = JsonConvert.DeserializeObject<AgvcConnectorConfig>(allText);
                Vehicle.AgvcConnectorConfig.ScheduleIntervalMs = Math.Max(Vehicle.AgvcConnectorConfig.ScheduleIntervalMs, minThreadSleep);
                Vehicle.AgvcConnectorConfig.AskReserveIntervalMs = Math.Max(Vehicle.AgvcConnectorConfig.AskReserveIntervalMs, minThreadSleep);

                allText = System.IO.File.ReadAllText("AlarmConfig.json");
                Vehicle.AlarmConfig = JsonConvert.DeserializeObject<AlarmConfig>(allText);

                allText = System.IO.File.ReadAllText("BatteryLog.json");
                Vehicle.BatteryLog = JsonConvert.DeserializeObject<BatteryLog>(allText);
                InitialSoc = Vehicle.BatteryLog.InitialSoc;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "讀寫設定檔"));
            }
            catch (Exception)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "讀寫設定檔"));
            }
        }

        private void LoggersInitial()
        {
            try
            {
                string loggerConfigPath = "Log.ini";
                if (File.Exists(loggerConfigPath))
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

        private void ControllersInitial()
        {
            try
            {
                alarmHandler = new AlarmHandler();
                mapHandler = new MapHandler();
                agvcConnector = new AgvcConnector(this);

                localPackage = new LocalPackage();
                RobotHandler = new Robot.NullObjRobotHandler(Vehicle.RobotStatus, Vehicle.CarrierSlotLeft);
                BatteryHandler = new Battery.NullObjBatteryHandler(Vehicle.BatteryStatus);
                MoveHandler = new Move.NullObjMoveHandler(Vehicle.MoveStatus, Vehicle.Mapinfo);

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "控制層"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "控制層"));
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
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

                localPackage.OnModeChangeEvent += LocalPackage_OnModeChangeEvent;
                localPackage.ImportantPspLog += LocalPackage_ImportantPspLog;
                localPackage.OnStatusChangeReportEvent += LocalPackage_OnStatusChangeReportEvent;
                localPackage.OnAlarmCodeSetEvent += LocalPackage_OnAlarmCodeSetEvent1;
                localPackage.OnAlarmCodeResetEvent += LocalPackage_OnAlarmCodeResetEvent;

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
                        case EnumTransferStep.MoveToAddressWaitArrival:
                            if (Vehicle.MoveStatus.IsMoveEnd)
                            {
                                MoveToAddressEnd();
                            }
                            else if (Vehicle.MoveStatus.LastAddress.Id == Vehicle.MovingGuide.ToAddressId)
                            {
                                MoveToAddressArrival();
                            }
                            break;
                        case EnumTransferStep.WaitMoveArrivalVitualPortReply:
                            if (Vehicle.TransferCommand.IsVitualPortUnloadArrivalReply)
                            {
                                DealVitualPortUnloadArrivalReply();
                            }
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

                Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAddressWaitArrival;
                Vehicle.MovingGuide = new MovingGuide();
                Vehicle.MovingGuide.ToAddressId = endAddressId;

                if (endAddressId == Vehicle.MoveStatus.LastAddress.Id)
                {
                    if (Vehicle.MoveStatus.IsMoveEnd)
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[原地到站] Same address end.");

                        Vehicle.MovingGuide.MoveComplete = EnumMoveComplete.Success;
                    }
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
                        if (Vehicle.MoveStatus.LastAddress.TransferPortDirection != EnumAddressDirection.None)
                        {
                            //localPackage.PartMove(EnumAseMoveCommandIsEnd.Begin);
                            MoveHandler.PartMoveBegin();
                        }

                        agvcConnector.ClearAllReserve();
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[詢問.路線] AskGuideAddressesAndSections.");
                        agvcConnector.AskGuideAddressesAndSections(endAddressId);

                        if (endReference == EnumMoveToEndReference.Avoid)
                        {
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAvoidWaitArrival;
                        }
                    }
                    else
                    {
                        SetAlarmFromAgvm(58);
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

        private void MoveToAddressArrival()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[移動.到達.站點] MoveToAddressArrival Address = {Vehicle.MoveStatus.LastAddress.Id}");

                agvcConnector.ClearAllReserve();
                if (Vehicle.IsCharging) StopCharge();

                if (!IsFirstOrderDealVitualPort())
                {
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAddressWaitEnd;

                    switch (Vehicle.TransferCommand.EnrouteState)
                    {
                        case CommandState.None:
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[二次定位] Move end.");
                                //localPackage.PartMove(EnumAseMoveCommandIsEnd.End);
                                MoveHandler.PartMoveEnd();
                            }
                            break;
                        case CommandState.LoadEnroute:
                            {
                                if (Vehicle.CarrierSlotLeft.EnumCarrierSlotState == EnumCarrierSlotState.Empty && Vehicle.MainFlowConfig.SlotDisable != EnumSlotSelect.Left)
                                {
                                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[二次定位.左開蓋] Move end. Open slot left.");
                                    Vehicle.TransferCommand.SlotNumber = EnumSlotNumber.L;
                                    //localPackage.PartMove(EnumAseMoveCommandIsEnd.End, EnumSlotSelect.Left);
                                    MoveHandler.PartMoveEnd(EnumSlotSelect.Left);
                                }
                                else if (Vehicle.CarrierSlotRight.EnumCarrierSlotState == EnumCarrierSlotState.Empty && Vehicle.MainFlowConfig.SlotDisable != EnumSlotSelect.Right)
                                {
                                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[二次定位.右開蓋] Move end. Open slot right.");
                                    Vehicle.TransferCommand.SlotNumber = EnumSlotNumber.R;
                                    //localPackage.PartMove(EnumAseMoveCommandIsEnd.End, EnumSlotSelect.Right);
                                    MoveHandler.PartMoveEnd(EnumSlotSelect.Right);
                                }
                                else
                                {
                                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[儲位已滿.無法取貨] Move end. No slot to load.");
                                    VehicleSlotFullFindFitUnloadCommand();
                                }
                            }
                            break;
                        case CommandState.UnloadEnroute:
                            if (Vehicle.TransferCommand.SlotNumber == EnumSlotNumber.L)
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[二次定位.左開蓋] Move end. Open slot left.");
                                //localPackage.PartMove(EnumAseMoveCommandIsEnd.End, EnumSlotSelect.Left);
                                MoveHandler.PartMoveEnd(EnumSlotSelect.Left);
                            }
                            else
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[二次定位.右開蓋] Move end. Open slot right.");
                                //localPackage.PartMove(EnumAseMoveCommandIsEnd.End, EnumSlotSelect.Right);
                                MoveHandler.PartMoveEnd(EnumSlotSelect.Right);
                            }
                            break;
                    }
                }
                else
                {
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitMoveArrivalVitualPortReply;
                    Vehicle.TransferCommand.IsVitualPortUnloadArrivalReply = false;
                    agvcConnector.ReportUnloadArrival();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void VehicleSlotFullFindFitUnloadCommand()
        {
            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToLoad;

            bool foundNextCommand = false;

            if (!string.IsNullOrEmpty(Vehicle.MoveStatus.LastAddress.AgvStationId))
            {
                if (Vehicle.Mapinfo.agvStationMap.ContainsKey(Vehicle.MoveStatus.LastAddress.AgvStationId))
                {
                    foreach (var transferCommand in Vehicle.mapTransferCommands.Values.ToArray())
                    {
                        if (transferCommand.EnrouteState == CommandState.UnloadEnroute)
                        {
                            if (transferCommand.CommandId != Vehicle.TransferCommand.CommandId)
                            {
                                if (Vehicle.Mapinfo.agvStationMap[Vehicle.MoveStatus.LastAddress.AgvStationId].AddressIds.Contains(transferCommand.UnloadAddressId))
                                {
                                    foundNextCommand = true;
                                    transferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                                    Vehicle.TransferCommand = transferCommand;

                                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨檢查.儲位已滿] Vehicle slot full. Switch unlaod command.[{Vehicle.TransferCommand.CommandId}]");
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (!foundNextCommand)
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨前.檢查.失敗] Pre Load Check Fail. Slot is not Empty.");

                //SetAlarmFromAgvm(000016);

                Thread.Sleep(2000);
                //Vehicle.TransferCommand.TransferStep = EnumTransferStep.Abort;
            }
        }

        private bool IsFirstOrderDealVitualPort()
        {
            if (Vehicle.TransferCommand.EnrouteState == CommandState.UnloadEnroute)
            {
                if (string.IsNullOrEmpty(Vehicle.TransferCommand.UnloadPortId)) return false;
                if (!Vehicle.Mapinfo.portMap.ContainsKey(Vehicle.TransferCommand.UnloadPortId)) return false;
                return Vehicle.Mapinfo.portMap[Vehicle.TransferCommand.UnloadPortId].IsVitualPort;
            }
            return false;
        }

        private void DealVitualPortUnloadArrivalReply()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Vitual Port Unload Arrival Replyed. [{Vehicle.PortInfos.Count}]");
                Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;

                bool foundUnloadPort = false;
                var readyPorts = Vehicle.PortInfos.Where(portInfo => portInfo.IsInputMode && portInfo.IsAGVPortReady).ToList();
                if (readyPorts.Any())
                {
                    var sameAddressReadyPorts = readyPorts.Where(portInfo => portInfo.ID == Vehicle.MoveStatus.LastAddress.Id).ToList();
                    if (sameAddressReadyPorts.Any())
                    {
                        Vehicle.TransferCommand.UnloadPortId = sameAddressReadyPorts[0].ID;
                        foundUnloadPort = true;
                    }
                    else
                    {
                        foreach (var portInfo in readyPorts)
                        {
                            if (Vehicle.Mapinfo.portMap.ContainsKey(portInfo.ID))
                            {
                                var port = Vehicle.Mapinfo.portMap[portInfo.ID];
                                Vehicle.TransferCommand.UnloadAddressId = port.ReferenceAddressId;
                                Vehicle.TransferCommand.UnloadPortId = portInfo.ID;
                                foundUnloadPort = true;
                                break;
                            }
                        }
                    }
                }

                if (!foundUnloadPort)
                {
                    VitualPortReplyUnreadyFindFitLoadCommand();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void VitualPortReplyUnreadyFindFitLoadCommand()
        {
            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;

            bool foundNextCommand = false;

            if (!string.IsNullOrEmpty(Vehicle.MoveStatus.LastAddress.AgvStationId))
            {
                if (Vehicle.Mapinfo.agvStationMap.ContainsKey(Vehicle.MoveStatus.LastAddress.AgvStationId))
                {
                    foreach (var transferCommand in Vehicle.mapTransferCommands.Values.ToArray())
                    {
                        if (transferCommand.EnrouteState == CommandState.LoadEnroute)
                        {
                            if (transferCommand.CommandId != Vehicle.TransferCommand.CommandId)
                            {
                                if (Vehicle.Mapinfo.agvStationMap[Vehicle.MoveStatus.LastAddress.AgvStationId].AddressIds.Contains(transferCommand.LoadAddressId))
                                {
                                    foundNextCommand = true;
                                    transferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                                    Vehicle.TransferCommand = transferCommand;

                                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Vitual port unload unready. Switch Load Command. [{Vehicle.TransferCommand.CommandId}][{Vehicle.PortInfos.Count}]");

                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (!foundNextCommand)
            {
                Thread.Sleep(2000);
            }
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

                    SetAlarmFromAgvm(6);
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
                                if (!IsFirstOrderDealVitualPort())
                                {
                                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.UnloadArrival;
                                }
                                else
                                {
                                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitMoveArrivalVitualPortReply;
                                    Vehicle.TransferCommand.IsVitualPortUnloadArrivalReply = false;
                                    agvcConnector.ReportUnloadArrival();
                                }
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
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAddressWaitArrival;
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
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAddressWaitArrival;
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
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAddressWaitArrival;
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

        private void AgvcConnector_OnOverrideCommandEvent(object sender, AgvcTransferCommand transferCommand)
        {
            try
            {
                var msg = $"MainFlow :  Get [ Override ]Command[{transferCommand.CommandId}],  start check .";
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AgvcConnector_OnAvoideRequestEvent(object sender, MovingGuide aseMovingGuide)
        {
            #region 避車檢查
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"MainFlow :  Get Avoid Command, End Adr=[{aseMovingGuide.ToAddressId}],  start check .");

                agvcConnector.PauseAskReserve();

                if (Vehicle.mapTransferCommands.IsEmpty)
                {
                    throw new Exception("Vehicle has no Command, can not Avoid");
                }

                if (!IsMoveStep())
                {
                    throw new Exception("Vehicle is not moving, can not Avoid");
                }

                if (!IsMoveStopByNoReserve() && !Vehicle.MovingGuide.IsAvoidComplete)
                {
                    throw new Exception($"Vehicle is not stop by no reserve, can not Avoid");
                }

                //if (!IsAvoidCommandMatchTheMap(agvcMoveCmd))
                //{
                //    var reason = "避車路徑中 Port Adr 與路段不合圖資";
                //    RejectAvoidCommandAndResume(000018, reason, agvcMoveCmd);
                //    return;
                //}
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
                IsAvoidMove = true;
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
                var reason = "避車Exception";
                RejectAvoidCommandAndResume(000036, reason, aseMovingGuide);
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

            #endregion
        }

        public bool IsMoveStep()
        {
            return Vehicle.TransferCommand.TransferStep == EnumTransferStep.MoveToAddressWaitEnd || Vehicle.TransferCommand.TransferStep == EnumTransferStep.MoveToAddressWaitArrival;
        }

        private bool IsMoveStopByNoReserve()
        {
            return Vehicle.MovingGuide.ReserveStop == VhStopSingle.On;
        }

        private void RejectAvoidCommandAndResume(int alarmCode, string reason, MovingGuide aseMovingGuide)
        {
            try
            {
                SetAlarmFromAgvm(alarmCode);
                agvcConnector.ReplyAvoidCommand(aseMovingGuide.SeqNum, 1, reason);
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, string.Concat($"MainFlow : Reject Avoid Command, ", reason));
                agvcConnector.ResumeAskReserve();
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
                    if (!Vehicle.Mapinfo.sectionMap.ContainsKey(sectionId))
                    {
                        throw new Exception($"Map info has no this section ID.[{sectionId}]");
                    }
                    else if (!Vehicle.Mapinfo.addressMap.ContainsKey(addressId))
                    {
                        throw new Exception($"Map info has no this address ID.[{addressId}]");
                    }

                    mapSection = Vehicle.Mapinfo.sectionMap[sectionId];
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
                SetAlarmFromAgvm(18);
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
                MapAddress address = Vehicle.Mapinfo.addressMap[Vehicle.MovingGuide.GuideAddressIds[sectionIndex + 1]];

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
                        moveStatus.NearlyAddress = Vehicle.Mapinfo.addressMap[movingGuide.ToAddressId];
                        moveStatus.NearlySection = movingGuide.MovingSections.Last();
                    }
                    else
                    {
                        //var nearlyDistance = 999999;
                        //var reserveOkSections = agvcConnector.queReserveOkSections.ToList();
                        //if (!reserveOkSections.Any())
                        //{
                        //    foreach (string addressId in movingGuide.GuideAddressIds)
                        //    {
                        //        MapAddress mapAddress = Vehicle.Mapinfo.addressMap[addressId];
                        //        var dis = moveStatus.LastMapPosition.MyDistance(mapAddress.Position);

                        //        if (dis < nearlyDistance)
                        //        {
                        //            nearlyDistance = dis;
                        //            moveStatus.NearlyAddress = mapAddress;
                        //        }
                        //    }

                        //    foreach (string sectionId in movingGuide.GuideSectionIds)
                        //    {
                        //        MapSection mapSection = Vehicle.Mapinfo.sectionMap[sectionId];
                        //        if (mapSection.InSection(moveStatus.NearlyAddress.Id))
                        //        {
                        //            moveStatus.NearlySection = mapSection;
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    List<MapAddress> reserveOkAddrs = new List<MapAddress>();
                        //    foreach (var mapSection in reserveOkSections)
                        //    {
                        //        reserveOkAddrs.AddRange(mapSection.InsideAddresses);
                        //    }

                        //    foreach (var mapAddress in reserveOkAddrs)
                        //    {
                        //        var dis = moveStatus.LastMapPosition.MyDistance(mapAddress.Position);

                        //        if (dis < nearlyDistance)
                        //        {
                        //            nearlyDistance = dis;
                        //            moveStatus.NearlyAddress = mapAddress;
                        //        }
                        //    }

                        //    foreach (var mapSection in reserveOkSections)
                        //    {
                        //        if (mapSection.InSection(moveStatus.NearlyAddress.Id))
                        //        {
                        //            moveStatus.NearlySection = mapSection;
                        //        }
                        //    }

                        //}


                        moveStatus.NearlyAddress = (from addressId in movingGuide.GuideAddressIds
                                                    select Vehicle.Mapinfo.addressMap[addressId]).ToList()
                                                    .OrderBy(address => address.MyDistance(positionArgs.MapPosition)).ToArray()
                                                    .First();

                        moveStatus.NearlySection = (from sectionId in movingGuide.GuideSectionIds
                                                    select Vehicle.Mapinfo.sectionMap[sectionId]).ToList()
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
                    moveStatus.NearlyAddress = Vehicle.Mapinfo.addressMap.Values.ToList()
                                               .OrderBy(address => address.MyDistance(positionArgs.MapPosition)).ToArray()
                                               .First();

                    moveStatus.NearlySection = Vehicle.Mapinfo.sectionMap.Values.ToList()
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
                Vehicle.OpPauseStatus = VhStopSingle.On;
                agvcConnector.StatusChangeReport();
            }
            else
            {
                Vehicle.OpPauseStatus = VhStopSingle.Off;
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
                        SetAlarmFromAgvm(55);
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

                if (Vehicle.CarrierSlotLeft.EnumCarrierSlotState == EnumCarrierSlotState.Empty && Vehicle.MainFlowConfig.SlotDisable != EnumSlotSelect.Left)
                {
                    Vehicle.TransferCommand.SlotNumber = EnumSlotNumber.L;
                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                }
                else if (Vehicle.CarrierSlotRight.EnumCarrierSlotState == EnumCarrierSlotState.Empty && Vehicle.MainFlowConfig.SlotDisable != EnumSlotSelect.Right)
                {
                    Vehicle.TransferCommand.SlotNumber = EnumSlotNumber.R;
                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                }
                else
                {
                    VehicleSlotFullFindFitUnloadCommand();
                    return;
                }

                LoadCmdInfo loadCmd = GetLoadCommand();
                if (Vehicle.MoveStatus.LastAddress.Id != Vehicle.TransferCommand.LoadAddressId)
                {
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                    return;
                }

                if (loadCmd.PortNumber == "9")
                {
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                    return;
                }

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[執行.取貨] Loading, [Direction={loadCmd.PioDirection}][SlotNum={loadCmd.SlotNumber}][Load Adr={loadCmd.PortAddressId}][Load Port Num={loadCmd.PortNumber}][PortId = {Vehicle.TransferCommand.LoadPortId}]");
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
                MapAddress portAddress = Vehicle.Mapinfo.addressMap[Vehicle.TransferCommand.LoadAddressId];
                LoadCmdInfo robotCommand = new LoadCmdInfo(Vehicle.TransferCommand);
                robotCommand.PioDirection = portAddress.PioDirection;
                robotCommand.GateType = portAddress.GateType;
                string portId = Vehicle.TransferCommand.LoadPortId.Trim();

                if (!Vehicle.Mapinfo.portMap.ContainsKey(portId))
                {
                    robotCommand.PortNumber = "1";
                }
                else
                {
                    var port = Vehicle.Mapinfo.portMap[portId];
                    if (port.IsVitualPort)
                    {
                        robotCommand.PortNumber = "9";
                    }
                    else
                    {
                        robotCommand.PortNumber = port.Number;
                        Vehicle.TransferCommand.LoadAddressId = port.ReferenceAddressId;
                    }
                }

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
                                    Vehicle.CarrierSlotLeft = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                else
                                {
                                    Vehicle.CarrierSlotRight = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;

                                }
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
                                    Vehicle.CarrierSlotLeft = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrNormal;
                                }
                                else
                                {
                                    Vehicle.CarrierSlotRight = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrNormal;
                                }
                            }
                            break;
                        case EnumCarrierSlotState.PositionError:
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.凸片] CST Position Error.");

                                slotStatus.CarrierId = "PositionError";
                                slotStatus.EnumCarrierSlotState = EnumCarrierSlotState.PositionError;

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.CarrierSlotLeft = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                else
                                {
                                    Vehicle.CarrierSlotRight = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                                }

                                SetAlarmFromAgvm(000051);
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
                                    Vehicle.CarrierSlotLeft = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                else
                                {
                                    Vehicle.CarrierSlotRight = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                                }

                                SetAlarmFromAgvm(000051);
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
                                else
                                {
                                    Vehicle.RightReadResult = BCRReadResult.BcrNormal;
                                }
                            }
                            else
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.失敗] Read CST ID = [{slotStatus.CarrierId}], unmatched command CST ID = [{Vehicle.TransferCommand.CassetteId }]");

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.LeftReadResult = BCRReadResult.BcrMisMatch;
                                }
                                else
                                {
                                    Vehicle.RightReadResult = BCRReadResult.BcrMisMatch;
                                }

                                SetAlarmFromAgvm(000028);
                            }
                            break;
                        case EnumCarrierSlotState.ReadFail:
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[取貨.讀取.失敗] ReadFail.");

                                slotStatus.CarrierId = "ReadFail";
                                slotStatus.EnumCarrierSlotState = EnumCarrierSlotState.ReadFail;

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.CarrierSlotLeft = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                else
                                {
                                    Vehicle.CarrierSlotRight = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                                }
                                SetAlarmFromAgvm(000004);
                            }
                            break;
                        case EnumCarrierSlotState.PositionError:
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.凸片] CST Position Error.");

                                slotStatus.CarrierId = "PositionError";
                                slotStatus.EnumCarrierSlotState = EnumCarrierSlotState.PositionError;

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.CarrierSlotLeft = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                else
                                {
                                    Vehicle.CarrierSlotRight = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                                }

                                SetAlarmFromAgvm(000051);
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
                    bool isEqLoad = string.IsNullOrEmpty(Vehicle.Mapinfo.addressMap[Vehicle.TransferCommand.LoadAddressId].AgvStationId);

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

                        bool isTransferCommandToEq = string.IsNullOrEmpty(Vehicle.Mapinfo.addressMap[targetAddressId].AgvStationId);

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

                            bool isTransferCommandToEq = string.IsNullOrEmpty(Vehicle.Mapinfo.addressMap[targetAddressId].AgvStationId);

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
            var addressPosition = Vehicle.Mapinfo.addressMap[addressId].Position;
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
                        if (Vehicle.CarrierSlotLeft.EnumCarrierSlotState == EnumCarrierSlotState.Empty)
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放貨前.檢查.失敗] Pre Unload Check Fail. Slot is Empty.");

                            SetAlarmFromAgvm(000017);
                            return;
                        }
                        break;
                    case EnumSlotNumber.R:
                        if (Vehicle.CarrierSlotRight.EnumCarrierSlotState == EnumCarrierSlotState.Empty)
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放貨前.檢查.失敗] Pre Unload Check Fail. Slot is Empty.");

                            SetAlarmFromAgvm(000017);
                            return;
                        }
                        break;
                }


                UnloadCmdInfo unloadCmd = GetUnloadCommand();
                if (Vehicle.MoveStatus.LastAddress.Id != Vehicle.TransferCommand.UnloadAddressId)
                {
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                    return;
                }

                if (unloadCmd.PortNumber == "9")
                {
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                    return;
                }

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[執行.放貨] : Unloading, [Direction{unloadCmd.PioDirection}][SlotNum={unloadCmd.SlotNumber}][Unload Adr={unloadCmd.PortAddressId}][Unload Port Num={unloadCmd.PortNumber}][PortId = {Vehicle.TransferCommand.UnloadPortId}]");
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
                MapAddress portAddress = Vehicle.Mapinfo.addressMap[Vehicle.TransferCommand.UnloadAddressId];
                UnloadCmdInfo robotCommand = new UnloadCmdInfo(Vehicle.TransferCommand);
                robotCommand.PioDirection = portAddress.PioDirection;
                robotCommand.GateType = portAddress.GateType;
                string portId = Vehicle.TransferCommand.UnloadPortId.Trim();

                if (!Vehicle.Mapinfo.portMap.ContainsKey(portId))
                {
                    robotCommand.PortNumber = "1";
                }
                else
                {
                    var port = Vehicle.Mapinfo.portMap[portId];
                    if (port.IsVitualPort)
                    {
                        robotCommand.PortNumber = "9";
                    }
                    else
                    {
                        robotCommand.PortNumber = port.Number;
                        Vehicle.TransferCommand.UnloadAddressId = port.ReferenceAddressId;
                    }
                }

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
                            SetAlarmFromAgvm(51);
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
                            Vehicle.CarrierSlotLeft = slotStatus;
                            break;
                        case EnumSlotNumber.R:
                            Vehicle.CarrierSlotRight = slotStatus;
                            break;
                    }

                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[手動.清空.儲位.狀態] OnUpdateCarrierSlotStatus: ManualDeleteCST[{slotStatus.SlotNumber}][{slotStatus.EnumCarrierSlotState}][ID={slotStatus.CarrierId}]");

                    agvcConnector.Send_Cmd136_CstRemove(slotStatus.SlotNumber);
                }
                else
                {
                    switch (slotStatus.SlotNumber)
                    {
                        case EnumSlotNumber.L:
                            Vehicle.CarrierSlotLeft = slotStatus;
                            break;
                        case EnumSlotNumber.R:
                            Vehicle.CarrierSlotRight = slotStatus;
                            break;
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
                    SetAlarmFromAgvm(14);
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
                            ResetAllAlarmsFromAgvm();

                            Vehicle.TransferCommand.CompleteStatus = CompleteStatus.InterlockError;
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
                Vehicle.TransferCommand.CompleteStatus = CompleteStatus.VehicleAbort;
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

                if (Vehicle.mapTransferCommands.Count == 1)
                {
                    Vehicle.TransferCommand = Vehicle.mapTransferCommands.Values.ToArray()[0];
                }
                else
                {
                    //Vehicle.TransferCommand = PickACommand();
                    Vehicle.TransferCommand = Vehicle.mapTransferCommands.Values.ToArray()[0];
                }
            }
        }

        private void TransferCommandComplete()
        {
            try
            {
                WaitingTransferCompleteEnd = true;

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令.結束] TransferComplete. [CommandId = {Vehicle.TransferCommand.CommandId}][CompleteStatus = {Vehicle.TransferCommand.CompleteStatus}]");

                if (!alarmHandler.dicHappeningAlarms.IsEmpty)
                {
                    ResetAllAlarmsFromAgvm();
                }
                Vehicle.mapTransferCommands.TryRemove(Vehicle.TransferCommand.CommandId, out AgvcTransferCommand transferCommand);
                agvcConnector.TransferComplete(transferCommand);
                //asePackage.SetTransferCommandInfoRequest(transferCommand, EnumCommandInfoStep.End);

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
                            bool isTransferCommandToEq = string.IsNullOrEmpty(Vehicle.Mapinfo.addressMap[targetAddressId].AgvStationId); ;

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
                                bool isTransferCommandToEq = string.IsNullOrEmpty(Vehicle.Mapinfo.addressMap[targetAddressId].AgvStationId);

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

                if (Vehicle.CarrierSlotLeft.EnumCarrierSlotState == EnumCarrierSlotState.Loading || Vehicle.CarrierSlotRight.EnumCarrierSlotState == EnumCarrierSlotState.Loading)
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
                            break;
                        case EnumAgvcTransCommandType.Load:
                            CheckRobotPortAddress(transferCommand.LoadAddressId, transferCommand.LoadPortId);
                            CheckCstIdDuplicate(transferCommand.CassetteId);
                            CheckTransferCommandMap(transferCommand);
                            break;
                        case EnumAgvcTransCommandType.Unload:
                            CheckRobotPortAddress(transferCommand.UnloadAddressId, transferCommand.UnloadPortId);
                            transferCommand.SlotNumber = CheckUnloadCstId(transferCommand.CassetteId);
                            CheckTransferCommandMap(transferCommand);
                            break;
                        case EnumAgvcTransCommandType.LoadUnload:
                            CheckRobotPortAddress(transferCommand.LoadAddressId, transferCommand.LoadPortId);
                            CheckRobotPortAddress(transferCommand.UnloadAddressId, transferCommand.UnloadPortId);
                            CheckCstIdDuplicate(transferCommand.CassetteId);
                            CheckTransferCommandMap(transferCommand);
                            break;
                        case EnumAgvcTransCommandType.Override:
                            CheckOverrideAddress(transferCommand);
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
                    //asePackage.SetTransferCommandInfoRequest(transferCommand, EnumCommandInfoStep.Begin);
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

            int existEnroute = 0;
            foreach (var item in Vehicle.mapTransferCommands.Values.ToArray())
            {
                if (item.EnrouteState == transferCommand.EnrouteState)
                {
                    existEnroute++;
                }
            }
            if (existEnroute > 1)
            {
                throw new Exception($"Vehicle has no enough slot to transfer. ExistEnroute[{existEnroute}]");
            }
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
            if (Vehicle.CarrierSlotLeft.CarrierId.Trim() == cassetteId)
            {
                return EnumSlotNumber.L;
            }
            else if (Vehicle.CarrierSlotRight.CarrierId.Trim() == cassetteId)
            {
                return EnumSlotNumber.R;
            }
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
            MapAddress portAddress = Vehicle.Mapinfo.addressMap[portAddressId];
            if (!portAddress.IsTransferPort())
            {
                throw new Exception($"{portAddressId} can not unload.");
            }

            if (Vehicle.Mapinfo.portMap.ContainsKey(portId))
            {
                var port = Vehicle.Mapinfo.portMap[portId];
                if (port.ReferenceAddressId != portAddressId)
                {
                    throw new Exception($"{portAddressId} unmatch {portId}.");
                }
            }
        }

        private void CheckMoveEndAddress(string unloadAddressId)
        {
            if (!Vehicle.Mapinfo.addressMap.ContainsKey(unloadAddressId))
            {
                throw new Exception($"{unloadAddressId} is not in the map.");
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
                    if (Vehicle.AutoState == EnumAutoState.Auto && IsVehicleIdle() && IsLowPower() && !IsLowPowerStartChargeTimeout)
                    {
                        LowPowerStartCharge(Vehicle.MoveStatus.LastAddress);
                    }
                    if (Vehicle.BatteryStatus.Percentage < Vehicle.MainFlowConfig.HighPowerPercentage - 21 && !Vehicle.IsCharging) //200701 dabid+
                    {
                        SetAlarmFromAgvm(2);
                    }
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
                        SetAlarmFromAgvm(000013);
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
                        SetAlarmFromAgvm(000013);
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
                            SetAlarmFromAgvm(000014);
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
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"MainFlow : Get [{receive.CancelAction}] Command.");

                string abortCmdId = receive.CmdID.Trim();
                bool IsAbortCurCommand = Vehicle.TransferCommand.CommandId == abortCmdId;
                var targetAbortCmd = Vehicle.mapTransferCommands[abortCmdId];

                if (IsAbortCurCommand)
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放棄.當前.命令] TransferComplete [{targetAbortCmd.CompleteStatus}].");

                    agvcConnector.ClearAllReserve();
                    MoveHandler.StopMove();
                    Vehicle.MovingGuide = new MovingGuide();
                    Vehicle.TransferCommand.CompleteStatus = GetCompleteStatusFromCancelRequest(receive.CancelAction);
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.TransferComplete;
                }
                else
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放棄.背景.命令] TransferComplete [{targetAbortCmd.CompleteStatus}].");

                    WaitingTransferCompleteEnd = true;

                    targetAbortCmd.TransferStep = EnumTransferStep.Abort;
                    targetAbortCmd.CompleteStatus = GetCompleteStatusFromCancelRequest(receive.CancelAction);

                    Vehicle.mapTransferCommands.TryRemove(Vehicle.TransferCommand.CommandId, out AgvcTransferCommand transferCommand);
                    agvcConnector.TransferComplete(transferCommand);
                    //asePackage.SetTransferCommandInfoRequest(transferCommand, EnumCommandInfoStep.End);

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

        private CompleteStatus GetCompleteStatusFromCancelRequest(CancelActionType cancelAction)
        {
            switch (cancelAction)
            {
                case CancelActionType.CmdCancel:
                    return CompleteStatus.Cancel;
                case CancelActionType.CmdCancelIdMismatch:
                    return CompleteStatus.IdmisMatch;
                case CancelActionType.CmdCancelIdReadFailed:
                    return CompleteStatus.IdreadFailed;
                case CancelActionType.CmdNone:
                case CancelActionType.CmdEms:
                case CancelActionType.CmdAbort:
                default:
                    return CompleteStatus.Abort;
            }
        }

        public void AgvcDisconnected()
        {
            try
            {
                SetAlarmFromAgvm(56);
                localPackage.ReportAgvcDisConnect();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #region AsePackage        

        public object _ModeChangeLocker = new object();

        public void LocalPackage_OnModeChangeEvent(object sender, EnumAutoState autoState)
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
                                localPackage.SetVehicleAutoScenario();
                                ResetAllAlarmsFromAgvm();
                                Thread.Sleep(3000);  //500-->3000
                                CheckCanAuto();
                                UpdateSlotStatus();
                                Vehicle.MoveStatus.IsMoveEnd = false;
                                break;
                            case EnumAutoState.Manual:
                                localPackage.RequestVehicleToManual();
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
                    SetAlarmFromAgvm(31);
                    localPackage.RequestVehicleToManual();
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
                SetAlarmFromAgvm(54);
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
                CarrierSlotStatus leftSlotStatus = new CarrierSlotStatus(Vehicle.CarrierSlotLeft);
                CarrierSlotStatus rightSlotStatus = new CarrierSlotStatus(Vehicle.CarrierSlotRight);

                switch (leftSlotStatus.EnumCarrierSlotState)
                {
                    case EnumCarrierSlotState.Empty:
                        {
                            leftSlotStatus.CarrierId = "";
                            leftSlotStatus.EnumCarrierSlotState = EnumCarrierSlotState.Empty;
                            Vehicle.CarrierSlotLeft = leftSlotStatus;
                            Vehicle.LeftReadResult = BCRReadResult.BcrNormal;
                        }
                        break;
                    case EnumCarrierSlotState.Loading:
                        {
                            if (string.IsNullOrEmpty(leftSlotStatus.CarrierId.Trim()))
                            {
                                leftSlotStatus.CarrierId = "ReadFail";
                                leftSlotStatus.EnumCarrierSlotState = EnumCarrierSlotState.ReadFail;
                                Vehicle.CarrierSlotLeft = leftSlotStatus;
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
                            SetAlarmFromAgvm(51);
                            // AsePackage_OnModeChangeEvent(this, EnumAutoState.Manual);
                        }
                        return;
                    case EnumCarrierSlotState.ReadFail:
                        {
                            leftSlotStatus.CarrierId = "ReadFail";
                            leftSlotStatus.EnumCarrierSlotState = EnumCarrierSlotState.ReadFail;
                            Vehicle.CarrierSlotLeft = leftSlotStatus;
                            Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                        }
                        break;
                    default:
                        break;
                }

                switch (rightSlotStatus.EnumCarrierSlotState)
                {
                    case EnumCarrierSlotState.Empty:
                        {
                            rightSlotStatus.CarrierId = "";
                            rightSlotStatus.EnumCarrierSlotState = EnumCarrierSlotState.Empty;
                            Vehicle.CarrierSlotRight = rightSlotStatus;
                            Vehicle.RightReadResult = BCRReadResult.BcrNormal;
                        }
                        break;
                    case EnumCarrierSlotState.Loading:
                        {
                            if (string.IsNullOrEmpty(rightSlotStatus.CarrierId.Trim()))
                            {
                                rightSlotStatus.CarrierId = "ReadFail";
                                rightSlotStatus.EnumCarrierSlotState = EnumCarrierSlotState.ReadFail;
                                Vehicle.CarrierSlotRight = rightSlotStatus;
                                Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                            }
                            else
                            {
                                Vehicle.RightReadResult = BCRReadResult.BcrNormal;
                            }
                        }
                        break;
                    case EnumCarrierSlotState.PositionError:
                        {
                            SetAlarmFromAgvm(51);
                            // AsePackage_OnModeChangeEvent(this, EnumAutoState.Manual);
                        }
                        return;
                    case EnumCarrierSlotState.ReadFail:
                        {
                            rightSlotStatus.CarrierId = "ReadFail";
                            rightSlotStatus.EnumCarrierSlotState = EnumCarrierSlotState.ReadFail;
                            Vehicle.CarrierSlotRight = rightSlotStatus;
                            Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
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

        private void LocalPackage_ImportantPspLog(object sender, string msg)
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }
            catch (System.Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void LocalPackage_OnAlarmCodeResetEvent(object sender, int e)
        {
            ResetAllAlarmsFromAgvl();
        }

        private void LocalPackage_OnAlarmCodeSetEvent1(object sender, int id)
        {
            SetAlarmFromAgvl(id);
        }

        private void LocalPackage_OnStatusChangeReportEvent(object sender, string e)
        {
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, e);
            agvcConnector.StatusChangeReport();
        }

        #endregion

        #region Set / Reset Alarm

        public void SetAlarmFromAgvm(int errorCode)
        {
            if (!alarmHandler.dicHappeningAlarms.ContainsKey(errorCode))
            {
                alarmHandler.SetAlarm(errorCode);
                //localPackage.SetAlarmCode(errorCode, true);
                var IsAlarm = alarmHandler.IsAlarm(errorCode);

                agvcConnector.SetlAlarmToAgvc(errorCode, IsAlarm);
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, alarmHandler.GetAlarmText(errorCode));
            }
        }

        public void SetAlarmFromAgvl(int errorCode)
        {
            if (!alarmHandler.dicHappeningAlarms.ContainsKey(errorCode))
            {
                alarmHandler.SetAlarm(errorCode);
                var IsAlarm = alarmHandler.IsAlarm(errorCode);
                agvcConnector.SetlAlarmToAgvc(errorCode, IsAlarm);
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, alarmHandler.GetAlarmText(errorCode));
            }
        }

        public void ResetAllAlarmsFromAgvm()
        {
            alarmHandler.ResetAllAlarms();
            //localPackage.ResetAllAlarmCode();
            agvcConnector.ResetAllAlarmsToAgvc();
        }

        public void ResetAllAlarmsFromAgvc()
        {
            alarmHandler.ResetAllAlarms();
            //localPackage.ResetAllAlarmCode();
        }

        public void ResetAllAlarmsFromAgvl()
        {
            alarmHandler.ResetAllAlarms();
            agvcConnector.ResetAllAlarmsToAgvc();
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
                mirleLogger.Log(new LogFormat("Error", "5", classMethodName, Vehicle.AgvcConnectorConfig.ClientName, "CarrierID", exMsg));
            }
            catch (Exception) { }
        }

        public void LogDebug(string classMethodName, string msg)
        {
            try
            {
                mirleLogger.Log(new LogFormat("Debug", "5", classMethodName, Vehicle.AgvcConnectorConfig.ClientName, "CarrierID", msg));
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