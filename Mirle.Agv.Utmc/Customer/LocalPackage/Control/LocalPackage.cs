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
            thdWatchPosition = new Thread(WatchPosition);
            thdWatchPosition.IsBackground = true;
            thdWatchPosition.Start();

            thdWatchBatteryState = new Thread(WatchBatteryState);
            thdWatchBatteryState.IsBackground = true;
            thdWatchBatteryState.Start();
        }

        #region Threads
        private void WatchPosition()
        {
            while (true)
            {
                try
                {
                    if (IsWatchPositionPause)
                    {
                        SpinWait.SpinUntil(() => !IsWatchPositionPause, Vehicle.MoveConfig.WatchPositionInterval);
                        continue;
                    }

                    if (!ReceivePositionArgsQueue.Any())
                    {
                        SendPositionReportRequest();
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }

                SpinWait.SpinUntil(() => false, Vehicle.MoveConfig.WatchPositionInterval);
            }
        }
        public void SendPositionReportRequest()
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void WatchBatteryState()
        {
            while (true)
            {
                try
                {
                    SendBatteryStatusRequest();
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }

                if (Vehicle.IsCharging)
                {
                    //Thread.Sleep(Vehicle.AseBatteryConfig.WatchBatteryStateIntervalInCharging);
                    SpinWait.SpinUntil(() => false, Vehicle.LocalPackageBatteryConfig.WatchBatteryStateIntervalInCharging);
                }
                else
                {
                    //Thread.Sleep(Vehicle.AseBatteryConfig.WatchBatteryStateInterval);
                    SpinWait.SpinUntil(() => false, Vehicle.LocalPackageBatteryConfig.WatchBatteryStateInterval);
                }
            }
        }
        public void SendBatteryStatusRequest()
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region PrimarySend
        public void DoRobotCommand(string robotCommandInfo)
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void SetAlarmCode(int alarmCode, bool isSet)
        {
            try
            {
                string isSetString = isSet ? "1" : "0";
                string alarmCodeString = alarmCode.ToString(new string('0', 6));
                string psMessage = string.Concat(isSetString, alarmCodeString);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void ResetAllAlarmCode()
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void BuzzerOff()
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void ClearRobotCommand()
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void ReadCarrierId()
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void DoRobotCommand(RobotCommand robotCommand)
        {
            try
            {
                RobotCommand = robotCommand;
                string robotCommandString = GetRobotCommandString();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void RefreshRobotState()
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void RefreshCarrierSlotState()
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private string GetRobotCommandString()
        {
            try
            {
                string pioDirection = ((int)RobotCommand.PioDirection).ToString();
                string fromPort = "";
                string toPort = "";
                switch (RobotCommand.GetTransferStepType())
                {
                    case EnumTransferStepType.Load:
                        fromPort = RobotCommand.PortAddressId.PadLeft(5, '0');
                        toPort = RobotCommand.SlotNumber.ToString().PadLeft(5, '0');
                        break;
                    case EnumTransferStepType.Unload:
                        fromPort = RobotCommand.SlotNumber.ToString().PadLeft(5, '0');
                        toPort = RobotCommand.PortAddressId.PadLeft(5, '0');
                        break;
                    case EnumTransferStepType.Move:
                    case EnumTransferStepType.MoveToCharger:
                    case EnumTransferStepType.Empty:
                    default:
                        throw new Exception($"Robot command type error.[{RobotCommand.GetTransferStepType()}]");
                }

                string gateType = RobotCommand.GateType.Substring(0, 1);
                string portNumber = RobotCommand.PortNumber.Substring(0, 1);

                return string.Concat(pioDirection, fromPort, toPort, gateType, portNumber).PadRight(24, '0');
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return "";
            }
        }

        public void ChargeStatusRequest()
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void PartMove(MapPosition mapPosition, int headAngle, int speed, EnumAseMoveCommandIsEnd isEnd, EnumIsExecute keepOrGo, EnumSlotSelect openSlot = EnumSlotSelect.None)
        {
            try
            {
                string isEndString = ((int)isEnd).ToString();
                string positionX = GetPositionString(mapPosition.X);
                string positionY = GetPositionString(mapPosition.Y);
                string thetaString = GetNumberToString(headAngle, 3);
                string speedString = GetNumberToString(speed, 4);
                string openSlotString = ((int)openSlot).ToString();
                string keepOrGoString = keepOrGo.ToString().Substring(0, 1).ToUpper();
                string message = string.Concat(isEndString, positionX, positionY, thetaString, speedString, openSlotString, keepOrGoString);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void PartMove(EnumAseMoveCommandIsEnd enumAseMoveCommandIsEnd, EnumSlotSelect openSlot = EnumSlotSelect.None)
        {
            try
            {
                MoveStatus moveStatus = new MoveStatus(Vehicle.Instance.MoveStatus);
                string beginString = ((int)enumAseMoveCommandIsEnd).ToString();
                string positionX = GetPositionString(moveStatus.LastAddress.Position.X);
                string positionY = GetPositionString(moveStatus.LastAddress.Position.Y);
                string thetaString = GetNumberToString((int)moveStatus.LastAddress.VehicleHeadAngle, 3);
                string speedString = GetNumberToString((int)moveStatus.LastSection.Speed, 4);
                string openSlotString = ((int)openSlot).ToString();
                string keepOrGoString = "G";
                string message = string.Concat(beginString, positionX, positionY, thetaString, speedString, openSlotString, keepOrGoString);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private string GetPositionString(double x)
        {
            try
            {
                string result = x >= 0 ? "P" : "N";
                string number = GetNumberToString((int)(Math.Abs(x)), 8);
                result = string.Concat(result, number);
                return result;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return "";
            }
        }

        private string GetNumberToString(int value, ushort digit)
        {
            try
            {
                string valueFormat = new string('0', digit);

                return value.ToString(valueFormat);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return "";
            }
        }

        #endregion

        #region PrimaryReceived

        private void ReceiveRobotCommandFinishedReport(string psMessage)
        {
            try
            {
                string finishedMsg = psMessage.Trim();
                if (Enum.TryParse(finishedMsg, out EnumRobotEndType robotEndType))
                {
                    OnRobotEndEvent?.Invoke(this, robotEndType);
                }
                else
                {
                    throw new Exception($"Can not parse robot command finished report.[{finishedMsg}]");
                }
                //switch (finishedMsg)
                //{
                //    case "Finished":
                //        OnRobotCommandFinishEvent?.Invoke(this, RobotCommand);
                //        break;
                //    case "InterlockError":
                //        OnRobotInterlockErrorEvent?.Invoke(this, RobotCommand);
                //        break;
                //    case "RobotError":
                //        OnRobotCommandErrorEvent?.Invoke(this, RobotCommand);
                //        break;
                //    default:
                //        throw new Exception($"Can not parse robot command finished report.[{finishedMsg}]");
                //}
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
                OnRobotEndEvent?.Invoke(this, EnumRobotEndType.RobotError);
            }
        }

        private void ReplyFromMsgMap(PSTransactionXClass psTransaction)
        {
            try
            {
                PSMessageXClass primaryMessage = psTransaction.PSPrimaryMessage;
                int primaryNumber = int.Parse(primaryMessage.Number);
                string secondaryTypeNumber = "S" + (primaryNumber + 1).ToString("00");

                if (psMessageMap.ContainsKey(secondaryTypeNumber))
                {
                    psTransaction.PSSecondaryMessage = psMessageMap[secondaryTypeNumber];
                    //psWrapper.SecondarySent(ref psTransaction);
                    SecondarySendQueue.Enqueue(psTransaction);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private bool IsPrimaryInMessageMap(PSTransactionXClass psTransaction)
        {
            try
            {
                if (psMessageMap.Count <= 0)
                {
                    throw new Exception("PsMessageMap is empty");
                }

                PSMessageXClass primaryMessage = psTransaction.PSPrimaryMessage;
                if (primaryMessage.Type.ToUpper() != "P")
                {
                    throw new Exception($"PrimaryReceive transaction.primaryMessage.type is not P.[{primaryMessage.Type}]");
                }

                return psMessageMap.ContainsKey(primaryMessage.Type + primaryMessage.Number);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return false;
            }
        }

        private bool IsPrimaryEmpty(PSTransactionXClass psTransaction)
        {
            return psTransaction.PSPrimaryMessage == null;
        }

        private void ShowTestMsg()
        {
            ImportantPspLog?.Invoke(this, "A test msg from AGVL.");
        }

        private void ReceiveMoveAppendArrivalReport(string psMessage)
        {
            try
            {
                PositionArgs positionArgs = new PositionArgs();

                positionArgs.EnumLocalArrival = GetArrivalStatus(psMessage.Substring(0, 1));

                double x = GetPositionFromPsMessage(psMessage.Substring(1, 9));
                double y = GetPositionFromPsMessage(psMessage.Substring(10, 9));
                positionArgs.MapPosition = new MapPosition(x, y);

                if (int.TryParse(psMessage.Substring(19, 3), out int headAngle))
                {
                    positionArgs.HeadAngle = headAngle;
                }

                if (int.TryParse(psMessage.Substring(22, 3), out int movingDirection))
                {
                    positionArgs.MovingDirection = movingDirection;
                }

                if (int.TryParse(psMessage.Substring(25, 4), out int speed))
                {
                    positionArgs.Speed = speed;
                }

                if (positionArgs.EnumLocalArrival != EnumLocalArrival.Arrival)
                {
                    ImportantPspLog?.Invoke(this, $"ReceiveMoveAppendArrivalReport. [{psMessage.Substring(0, 1)}][{positionArgs.EnumLocalArrival}][({(int)x},{(int)y})]");
                }

                ReceivePositionArgsQueue.Enqueue(positionArgs);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private EnumLocalArrival GetArrivalStatus(string v)
        {
            switch (v)
            {
                case "0":
                    return EnumLocalArrival.Fail;
                case "1":
                    return EnumLocalArrival.Arrival;
                case "2":
                    return EnumLocalArrival.EndArrival;
                default:
                    throw new Exception($"Can not parse arrival report.[{v}]");
            }
        }

        private int GetIntTryParse(string v)
        {
            try
            {
                return int.Parse(v);
            }
            catch (Exception)
            {
                throw new Exception($"Can not parse int.[{v}]");
            }
        }

        private double GetPositionFromPsMessage(string v)
        {
            try
            {
                string isPositive = v.Substring(0, 1);
                double value = double.Parse(v.Substring(1, 8));
                return IsValuePositive(isPositive) ? value : -value;
            }
            catch (Exception)
            {
                throw new Exception($"Can not parse position report.[{v}]");
            }
        }

        private void AllAlarmReset()
        {
            try
            {
                OnStatusChangeReportEvent?.Invoke(this, $"AllAlarmReset:");

                OnAlarmCodeAllResetEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private void AlarmReport(string psMessage)
        {
            try
            {
                bool isAlarmSet = psMessage.Substring(0, 1) == "1";
                //int alarmCode = int.Parse(psMessage.Substring(1, Vehicle.AsePackageConfig.ErrorCodeLength));                

                //if (isAlarmSet)
                //{
                //    OnAlarmCodeSetEvent?.Invoke(this, alarmCode);
                //}
                //else
                //{
                //    OnAlarmCodeResetEvent?.Invoke(this, alarmCode);
                //}
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private void UpdateChargeStatus(string psMessage)
        {
            try
            {
                bool isCharging = psMessage.Substring(0, 1) == "1";
                if (isCharging)
                {
                    Vehicle.CheckStartChargeReplyEnd = true;
                }
                else
                {
                    Vehicle.CheckStopChargeReplyEnd = true;
                }
                Vehicle.IsCharging = isCharging;
                OnStatusChangeReportEvent?.Invoke(this, $"Local Update Charge Status :[{ Vehicle.IsCharging }]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private void UpdateCarrierSlotStatus(string psMessage, uint systemByte)
        {
            EnumSlotNumber slotNumber = EnumSlotNumber.L;
            CarrierSlotStatus carrierSlotStatus = new CarrierSlotStatus();

            try
            {
                slotNumber = psMessage.Substring(1, 1) == "L" ? EnumSlotNumber.L : EnumSlotNumber.R;

                carrierSlotStatus.SlotNumber = slotNumber;

                bool manualDeleteCst = psMessage.Substring(0, 1) == "1";
                carrierSlotStatus.ManualDeleteCST = manualDeleteCst;
                if (manualDeleteCst)
                {
                    OnUpdateSlotStatusEvent?.Invoke(this, carrierSlotStatus);
                    return;
                }

                carrierSlotStatus.EnumCarrierSlotState = GetCarrierSlotStatus(psMessage.Substring(2, 1));
                carrierSlotStatus.CarrierId = psMessage.Substring(3);
                if (carrierSlotStatus.EnumCarrierSlotState == EnumCarrierSlotState.Loading)
                {
                    if (string.IsNullOrEmpty(carrierSlotStatus.CarrierId.Trim()))
                    {
                        carrierSlotStatus.EnumCarrierSlotState = EnumCarrierSlotState.ReadFail;
                    }
                    else if (carrierSlotStatus.CarrierId == "ReadIdFail")
                    {
                        carrierSlotStatus.EnumCarrierSlotState = EnumCarrierSlotState.ReadFail;
                    }
                    else if (carrierSlotStatus.CarrierId == "PositionError")
                    {
                        carrierSlotStatus.EnumCarrierSlotState = EnumCarrierSlotState.PositionError;
                    }
                }

                OnUpdateSlotStatusEvent?.Invoke(this, carrierSlotStatus);
            }
            catch (Exception ex)
            {
                string msg = "Carrier Slot Report, " + ex.Message;
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                ImportantPspLog?.Invoke(this, msg);
            }
        }

        private EnumCarrierSlotState GetCarrierSlotStatus(string v)
        {
            switch (v)
            {
                case "0":
                    return EnumCarrierSlotState.Empty;
                case "1":
                    return EnumCarrierSlotState.Loading;
                case "2":
                    return EnumCarrierSlotState.PositionError;
                default:
                    throw new Exception($"Can not parse position report.[{v}]");
            }
        }

        private void UpdateRobotStatus(string psMessage)
        {
            try
            {
                RobotStatus aseRobotStatus = new RobotStatus();
                aseRobotStatus.EnumRobotState = GetRobotStatus(psMessage.Substring(0, 1));
                aseRobotStatus.IsHome = psMessage.Substring(1, 1) == "1";

                Vehicle.RobotStatus = aseRobotStatus;

                OnStatusChangeReportEvent?.Invoke(this, $"UpdateRobotStatus:[{Vehicle.RobotStatus.EnumRobotState}][RobotHome={Vehicle.RobotStatus.IsHome}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private EnumRobotState GetRobotStatus(string v)
        {
            switch (v)
            {
                case "0":
                    return EnumRobotState.Idle;
                case "1":
                    return EnumRobotState.Busy;
                case "2":
                    return EnumRobotState.Error;
                default:
                    return EnumRobotState.Error;
            }
        }

        private void UpdateMoveStatus(string psMessage)
        {
            try
            {
                MoveStatus aseMoveStatus = new MoveStatus(Vehicle.MoveStatus);
                aseMoveStatus.EnumMoveState = GetMoveState(psMessage.Substring(0, 1));
                aseMoveStatus.HeadDirection = GetIntTryParse(psMessage.Substring(1, 3));
                Vehicle.MoveStatus = aseMoveStatus;

                OnStatusChangeReportEvent?.Invoke(this, $"UpdateMoveStatus:[{Vehicle.MoveStatus.EnumMoveState}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);

            }
        }

        private EnumMoveState GetMoveState(string v)
        {
            switch (v)
            {
                case "0":
                    return EnumMoveState.Idle;
                case "1":
                    return EnumMoveState.Working;
                case "2":
                    return EnumMoveState.Pausing;
                case "3":
                    return EnumMoveState.Pause;
                case "4":
                    return EnumMoveState.Stoping;
                case "5":
                    return EnumMoveState.Block;
                case "6":
                    return EnumMoveState.Error;
                default:
                    return EnumMoveState.Error;
            }
        }

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
            AllAgvlStatusReportRequest();
            SpinWait.SpinUntil(() => false, 50);

            SendPositionReportRequest();
            SpinWait.SpinUntil(() => false, 50);

            SendBatteryStatusRequest();
            SpinWait.SpinUntil(() => false, 50);
        }

        public void AllAgvlStatusReportRequest()
        {
             
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

        public void CstRename(CarrierSlotStatus slotStatus)
        {
            try
            {
                string slotNumber = slotStatus.SlotNumber.ToString().Substring(0, 1);
                string cstRenameString = string.Concat(slotNumber, slotStatus.CarrierId);
               
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region SecondaryReceived

        private void ReceiveBatteryStatusRequestAck(string psMessage)
        {
            try
            {
                BatteryStatus aseBatteryStatus = new BatteryStatus(Vehicle.BatteryStatus);
                aseBatteryStatus.Percentage = GetIntTryParse(psMessage.Substring(0, 3));
                aseBatteryStatus.Voltage = GetIntTryParse(psMessage.Substring(3, 4)) * 0.01;
                aseBatteryStatus.Temperature = GetIntTryParse(psMessage.Substring(7, 3));
                //200522 dabid+ To Report 144 While Percentage diff
                if (Vehicle.BatteryStatus.Percentage != aseBatteryStatus.Percentage)
                {
                    SetPercentage(aseBatteryStatus.Percentage);
                }

                Vehicle.BatteryStatus = aseBatteryStatus;

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private void ReceivePositionReportRequestAck(string psMessage)
        {
            try
            {
                EnumMoveState moveState = GetMoveState(psMessage.Substring(0, 1));
                Vehicle.MoveStatus.EnumMoveState = moveState;

                PositionArgs positionArgs = new PositionArgs();

                EnumLocalArrival arrival = EnumLocalArrival.Arrival;
                switch (moveState)
                {
                    case EnumMoveState.Idle:
                    case EnumMoveState.Working:
                    case EnumMoveState.Pausing:
                    case EnumMoveState.Pause:
                    case EnumMoveState.Block:
                        break;
                    case EnumMoveState.Stoping:
                    case EnumMoveState.Error:
                        {
                            arrival = EnumLocalArrival.Fail;
                        }
                        break;
                    default:
                        break;
                }

                positionArgs.EnumLocalArrival = arrival;

                double x = GetPositionFromPsMessage(psMessage.Substring(1, 9));
                double y = GetPositionFromPsMessage(psMessage.Substring(10, 9));
                positionArgs.MapPosition = new MapPosition(x, y);

                if (int.TryParse(psMessage.Substring(19, 3), out int headAngle))
                {
                    positionArgs.HeadAngle = headAngle;
                }

                if (int.TryParse(psMessage.Substring(22, 3), out int movingDirection))
                {
                    positionArgs.MovingDirection = movingDirection;
                }

                if (int.TryParse(psMessage.Substring(25, 4), out int speed))
                {
                    positionArgs.Speed = speed;
                }

                if (arrival == EnumLocalArrival.Fail)
                {
                    ImportantPspLog?.Invoke(this, $"ReceivePositionReportRequestAck. [{psMessage.Substring(0, 1)}][{arrival.ToString()}][({x.ToString("F0")},{y.ToString("F0")})]");
                }

                ReceivePositionArgsQueue.Enqueue(positionArgs);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                //ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private bool IsValuePositive(string v)
        {
            switch (v)
            {
                case "P":
                    return true;
                case "N":
                    return false;
                default:
                    throw new Exception($"My positive parse fail. {v}");
            }
        }

        #endregion

        #region PsWrapper

        public void ReportAgvcDisConnect()
        {
            
        }

        #endregion

        #region Battery Control

        public void SetPercentage(int percentage)
        {
            try
            {
                if (Math.Abs(percentage - Vehicle.BatteryStatus.Percentage) >= 1)
                {
                    Vehicle.BatteryStatus.Percentage = percentage;
                    OnBatteryPercentageChangeEvent?.Invoke(this, percentage);
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
                
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void StartCharge(EnumAddressDirection chargeDirection)
        {
            try
            {
                string chargeDirectionString;
                switch (chargeDirection)
                {
                    case EnumAddressDirection.Left:
                        chargeDirectionString = "1";
                        break;
                    case EnumAddressDirection.Right:
                        chargeDirectionString = "2";
                        break;
                    case EnumAddressDirection.None:
                    default:
                        throw new Exception($"Start charge command direction error.[{chargeDirection}]");
                }               
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region Move Control

        public void MoveStop()
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void MoveContinue()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void MovePause()
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void RefreshMoveState()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

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
