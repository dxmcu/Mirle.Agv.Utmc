using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Model;
using Mirle.Agv.INX.Model.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Controller
{
    public class SensorSafetyControl
    {
        private LoggerAgent loggerAgent = LoggerAgent.Instance;
        private string normalLogName;
        private string device = MethodInfo.GetCurrentMethod().ReflectedType.Name;
        private ComputeFunction computeFunction = ComputeFunction.Instance;
        private LocalData localData = LocalData.Instance;
        private Dictionary<EnumMoveControlSafetyType, SafetyData> safety = null;

        public SensorSafetyControl(string normalLogName)
        {
            this.normalLogName = normalLogName;

            safety = localData.MoveControlData.MoveControlConfig.Safety;
        }

        private void WriteLog(int logLevel, string carrierId, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            LogFormat logFormat = new LogFormat(normalLogName, logLevel.ToString(), memberName, device, carrierId, message);

            loggerAgent.Log(logFormat.Category, logFormat);

            if (logLevel <= localData.ErrorLevel)
            {
                logFormat = new LogFormat(localData.ErrorLogName, logLevel.ToString(), memberName, device, carrierId, message);
                loggerAgent.Log(logFormat.Category, logFormat);
            }
        }

        #region 路徑保護.
        public EnumMoveCommandControlErrorCode AGVPathSafety_Line()
        {
            if (localData.MoveControlData.ThetaSectionDeviation == null)
                return EnumMoveCommandControlErrorCode.None;

            if (safety[EnumMoveControlSafetyType.OntimeReviseTheta].Enable)
            {
                if (localData.MoveControlData.MoveCommand.OntimeReviseThetaOn)
                {
                    WriteLog(5, "", String.Concat("車資偏差過大,且持續超過設定的delay時間 ",
                                                  localData.MoveControlData.MoveControlConfig.TimeValueConfig.DelayTimeList[EnumDelayTimeType.OntimeReviseAlarmDelayTime].ToString(), "ms"));
                    return EnumMoveCommandControlErrorCode.安全保護停止_角度偏差過大;
                }

                if (Math.Abs(localData.MoveControlData.ThetaSectionDeviation.Theta) > safety[EnumMoveControlSafetyType.OntimeReviseTheta].Range)
                {
                    if (!localData.MoveControlData.MoveCommand.OntimeReviseThetaDelaying)
                    {
                        WriteLog(5, "", String.Concat("角度偏差(開始delay) : ", localData.MoveControlData.ThetaSectionDeviation.Theta.ToString("0.0")));
                        localData.MoveControlData.MoveCommand.OntimeReviseThetaOn = true;
                    }
                }
                else
                {
                    if (localData.MoveControlData.MoveCommand.OntimeReviseThetaDelaying)
                    {
                        WriteLog(5, "", String.Concat("角度偏差(結束delay) : ", localData.MoveControlData.ThetaSectionDeviation.Theta.ToString("0.0")));
                        localData.MoveControlData.MoveCommand.OntimeReviseThetaOn = false;
                    }
                }
            }

            if (safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Enable)
            {
                if (localData.MoveControlData.MoveCommand.OntimeReviseDeviationOn)
                {
                    WriteLog(5, "", String.Concat("軌道偏差過大,且持續超過設定的delay時間 ",
                                                  localData.MoveControlData.MoveControlConfig.TimeValueConfig.DelayTimeList[EnumDelayTimeType.OntimeReviseAlarmDelayTime].ToString(), "ms"));
                    return EnumMoveCommandControlErrorCode.安全保護停止_軌道偏差過大;
                }

                if (Math.Abs(localData.MoveControlData.ThetaSectionDeviation.SectionDeviation) > safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Range)
                {
                    if (!localData.MoveControlData.MoveCommand.OntimeReviseDeviationDelaying)
                    {
                        WriteLog(5, "", String.Concat("軌道偏差(開始delay) : ", localData.MoveControlData.ThetaSectionDeviation.SectionDeviation.ToString("0.0")));
                        localData.MoveControlData.MoveCommand.OntimeReviseDeviationOn = true;
                    }
                }
                else
                {
                    if (localData.MoveControlData.MoveCommand.OntimeReviseDeviationDelaying)
                    {
                        WriteLog(5, "", String.Concat("軌道偏差(結束delay) : ", localData.MoveControlData.ThetaSectionDeviation.SectionDeviation.ToString("0.0")));
                        localData.MoveControlData.MoveCommand.OntimeReviseDeviationOn = false;
                    }
                }
            }

            return EnumMoveCommandControlErrorCode.None;
        }

        public EnumMoveCommandControlErrorCode AGVPathSafety_Line_Turning()
        {
            //if (vehicleData.ThetaAndSectionDeviation == null)
            //    return EnumMoveCommandControlErrorCode.None;

            //if (safety[EnumMoveControlSafetyType.OntimeReviseTheta].Enable)
            //{
            //    if (vehicleData.CommandFlags.OntimeReviseThetaDelaying)
            //    {
            //        if (vehicleData.MoveControlConfig.SafetyStopInTurningWithDelaying)
            //        {
            //            WriteLog(5, "", String.Concat("車資偏差過大,且因入彎和SafetyStopInTurningWithDelaying = true, 因此停車!"));
            //            return EnumMoveCommandControlErrorCode.安全保護停止_角度偏差過大;
            //        }
            //        else
            //        {
            //            WriteLog(5, "", String.Concat("車資偏差過大,但因入彎和SafetyStopInTurningWithDelaying = false, 因此Reset!"));
            //            vehicleData.CommandFlags.OntimeReviseThetaOn = false;
            //        }
            //    }
            //}

            //if (safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Enable)
            //{
            //    if (vehicleData.CommandFlags.OntimeReviseDeviationDelaying)
            //    {
            //        if (vehicleData.MoveControlConfig.SafetyStopInTurningWithDelaying)
            //        {
            //            WriteLog(5, "", String.Concat("軌道偏差過大,且因入彎和SafetyStopInTurningWithDelaying = true, 因此停車!"));
            //            return EnumMoveCommandControlErrorCode.安全保護停止_軌道偏差過大;
            //        }
            //        else
            //        {
            //            WriteLog(5, "", String.Concat("軌道偏差過大,但因入彎和SafetyStopInTurningWithDelaying = false, 因此Reset!"));
            //            vehicleData.CommandFlags.OntimeReviseDeviationOn = false;
            //        }
            //    }
            //}

            return EnumMoveCommandControlErrorCode.None;
        }

        private EnumMoveCommandControlErrorCode AGVPathSafety_RTurn()
        {
            return EnumMoveCommandControlErrorCode.None;
        }

        private EnumMoveCommandControlErrorCode AGVPathSafety_STurn()
        {

            return EnumMoveCommandControlErrorCode.None;
        }

        private EnumMoveCommandControlErrorCode AGVPathSafety()
        {
            if (localData.MoveControlData.MoveCommand.MoveStatus == EnumMoveStatus.Moving)
                return AGVPathSafety_Line();
            else
                //else if (vehicleData.MoveState == EnumMoveState.STurn)
                //    return AGVPathSafety_STurn();
                //else if (vehicleData.MoveState == EnumMoveState.RTurn)
                //    return AGVPathSafety_RTurn();

                return EnumMoveCommandControlErrorCode.None;
        }
        #endregion

        #region 定位更新保護.
        private EnumMoveCommandControlErrorCode SafetyTurnOutAndLineBarcodeIntervalInRange()
        {
            //if (vehicleData.SimulateMode)
            //    return EnumMoveCommandControlErrorCode.None;

            //bool newBarcodeData = (vehicleData.LocateData.NowAGVPosition != null && vehicleData.LocateData.NowAGVPosition.Type >= EnumAGVPositionType.OnlyRead);

            //vehicleData.MovingSafety.NowMoveState = vehicleData.MoveState;

            //if (vehicleData.MovingSafety.NowMoveState != vehicleData.MovingSafety.LastMoveState)
            //{
            //    if (vehicleData.MovingSafety.NowMoveState == EnumMoveState.STurn || vehicleData.MovingSafety.NowMoveState == EnumMoveState.RTurn)
            //        vehicleData.MovingSafety.Type = EnumPositionUpdateSafteyType.Turning;
            //    else if (vehicleData.MovingSafety.NowMoveState == EnumMoveState.Moving)
            //    {
            //        if (vehicleData.MovingSafety.LastMoveState == EnumMoveState.STurn || vehicleData.MovingSafety.LastMoveState == EnumMoveState.RTurn)
            //            vehicleData.MovingSafety.Type = EnumPositionUpdateSafteyType.TurnOut;
            //        else
            //            vehicleData.MovingSafety.Type = EnumPositionUpdateSafteyType.Line;
            //    }
            //    else
            //        vehicleData.MovingSafety.Type = EnumPositionUpdateSafteyType.Line;

            //    vehicleData.MovingSafety.Encoder = vehicleData.MotionData.Encoder;
            //}

            //switch (vehicleData.MovingSafety.Type)
            //{
            //    case EnumPositionUpdateSafteyType.None:
            //    case EnumPositionUpdateSafteyType.Turning:
            //        break;
            //    case EnumPositionUpdateSafteyType.Line:
            //        if (newBarcodeData)
            //            vehicleData.MovingSafety.Encoder = vehicleData.MotionData.Encoder;
            //        else
            //        {
            //            if (Math.Abs(vehicleData.MotionData.Encoder - vehicleData.MovingSafety.Encoder) > safety[EnumMoveControlSafetyType.LineBarcodeInterval].Range)
            //            {
            //                WriteLog(7, "", String.Concat("直線超過", Math.Abs(vehicleData.MotionData.Encoder - vehicleData.MovingSafety.Encoder).ToString("0"),
            //                    "mm未讀取到Barcode,超過安全設置的", safety[EnumMoveControlSafetyType.LineBarcodeInterval].Range.ToString("0"),
            //                    "mm!"));
            //                return EnumMoveCommandControlErrorCode.安全保護停止_直線過久沒讀到Barcode;
            //            }
            //        }

            //        break;

            //    case EnumPositionUpdateSafteyType.TurnOut:
            //        if (newBarcodeData)
            //        {
            //            WriteLog(7, "", String.Concat("出彎", Math.Abs(vehicleData.MotionData.Encoder - vehicleData.MovingSafety.Encoder).ToString("0"), "mm讀取到Barcode!"));
            //            vehicleData.MovingSafety.Type = EnumPositionUpdateSafteyType.Line;
            //            vehicleData.MovingSafety.Encoder = vehicleData.MotionData.Encoder;
            //        }
            //        else
            //        {
            //            if (Math.Abs(vehicleData.MotionData.Encoder - vehicleData.MovingSafety.Encoder) > safety[EnumMoveControlSafetyType.TurnOut].Range)
            //            {
            //                WriteLog(7, "", String.Concat("出彎", Math.Abs(vehicleData.MotionData.Encoder - vehicleData.MovingSafety.Encoder).ToString("0"),
            //                    "mm未讀取到Barcode,超過安全設置的", safety[EnumMoveControlSafetyType.TurnOut].Range.ToString("0"),
            //                    "mm!"));
            //                return EnumMoveCommandControlErrorCode.安全保護停止_出彎過久沒讀到Barcode;
            //            }
            //        }
            //        break;
            //}

            //vehicleData.MovingSafety.LastMoveState = vehicleData.MovingSafety.NowMoveState;
            return EnumMoveCommandControlErrorCode.None;
        }
        #endregion

        private bool VChangeSafetyNormal()
        {
            if (!safety[EnumMoveControlSafetyType.VChangeSafetyDistance].Enable)
                return true;
            else if (localData.SimulateMode)
                return true;

            if (Math.Abs(localData.MoveControlData.MotionControlData.LineVelocity - localData.MoveControlData.MotionControlData.SimulateLineVelocity) > safety[EnumMoveControlSafetyType.VChangeSafetyDistance].Range)
                return false;

            return true;
        }

        private EnumMoveCommandControlErrorCode AxisErrorCheck(EnumControlStatus motionDriverStatus)
        {
            if (motionDriverStatus != EnumControlStatus.Ready)
                return EnumMoveCommandControlErrorCode.安全保護停止_軸異常;

            //if (vehicleData.CommandFlags.AxisError != EnumVehicleSafetyAction.Normal)
            //{
            //    if (vehicleData.CommandFlags.OnlyAxisError &&
            //        vehicleData.CommandFlags.AxisErrorDelayTimer.ElapsedMilliseconds >
            //        vehicleData.MoveControlConfig.TimeValueConfig.DelayTimeList[EnumDelayTimeType.AxisErrorDelayTime])
            //    {
            //        vehicleData.CommandFlags.AxisErrorDelayTimer.Stop();
            //        return EnumMoveCommandControlErrorCode.安全保護停止_軸異常;
            //    }
            //}

            return EnumMoveCommandControlErrorCode.None;
        }

        public EnumMoveCommandControlErrorCode ForkNotHomeCheck()
        {
            //if (!vehicleData.SensorData.ForkHome)
            //{
            //    if (vehicleData.CommandFlags.OnlyForkNotHome &&
            //        vehicleData.CommandFlags.ForkNotHomeDelayTimer.ElapsedMilliseconds >
            //        vehicleData.MoveControlConfig.TimeValueConfig.DelayTimeList[EnumDelayTimeType.ForkNotHomeDelayTime])
            //    {
            //        vehicleData.CommandFlags.ForkNotHomeDelayTimer.Stop();
            //        return EnumMoveCommandControlErrorCode.安全保護停止_Fork不在Home點;
            //    }
            //}

            return EnumMoveCommandControlErrorCode.None;
        }

        private bool EMOStopAndSendAlarm()
        {
            //if (vehicleData.MoveControlConfig.SignalOnStopAndSendAlarm[EnumSignalOnStopAndSendAlarm.EMO] && vehicleData.SensorData.EMO == EnumVehicleSafetyAction.Stop)
            //    return true;
            //else
            return false;
        }

        private bool BumperStopAndSendAlarm()
        {
            //if (vehicleData.MoveControlConfig.SignalOnStopAndSendAlarm[EnumSignalOnStopAndSendAlarm.Bumper] && vehicleData.SensorData.BumpSensorState == EnumVehicleSafetyAction.Stop)
            //    return true;
            //else
            return false;
        }

        private bool UpdateDeltaSafetyError()
        {
            return false;
        }

        public EnumMoveCommandControlErrorCode SensorSafety(EnumControlStatus motionDriverStatus, EnumControlStatus locationControlStatus)
        {
            EnumMoveCommandControlErrorCode temp;

            if (localData.MoveControlData.MoveCommand.CommandStatus != EnumMoveCommandStartStatus.Start)
            {
                return EnumMoveCommandControlErrorCode.None;
            }
            else if (locationControlStatus != EnumControlStatus.Ready)
                return EnumMoveCommandControlErrorCode.安全保護停止_定位Control異常;
            else if (localData.MoveControlData.MoveCommand.VehicleStopFlag)
                return EnumMoveCommandControlErrorCode.安全保護停止_人為控制;
            else if (localData.MIPCData.SafetySensorStatus == EnumSafetyLevel.Alarm)
                return EnumMoveCommandControlErrorCode.安全保護停止_SafetySensorAlarm;
            else if (localData.MIPCData.SafetySensorStatus == EnumSafetyLevel.EMO ||
                     localData.MIPCData.SafetySensorStatus == EnumSafetyLevel.IPCEMO)
                return EnumMoveCommandControlErrorCode.安全保護停止_EMO停止;
            else if (localData.MIPCData.MotionAlarmCount != 0)
                return EnumMoveCommandControlErrorCode.安全保護停止_MotionAlarm;
            else if (!VChangeSafetyNormal())
                return EnumMoveCommandControlErrorCode.安全保護停止_速度變化異常;
            else
            {
                temp = AxisErrorCheck(motionDriverStatus);
                if (temp != EnumMoveCommandControlErrorCode.None)
                    return temp;

                //    temp = ForkNotHomeCheck();
                //    if (temp != EnumMoveCommandControlErrorCode.None)
                //        return temp;

                temp = AGVPathSafety();
                if (temp != EnumMoveCommandControlErrorCode.None)
                    return temp;

                temp = SafetyTurnOutAndLineBarcodeIntervalInRange();
                if (temp != EnumMoveCommandControlErrorCode.None)
                    return temp;
            }

            return EnumMoveCommandControlErrorCode.None;
        }

        public EnumVehicleSafetyAction GetSensorState()
        {
            EnumVehicleSafetyAction sensorState = EnumVehicleSafetyAction.Normal;

            /// 正常三種訊號種類.
            /// 1. AGVC-Pause : 只有Normal/Stop 兩種訊號,在無法Pause的地方下Pause會直接拒絕,不會keeps訊號.
            /// 2. ReserveIndex : Reserve(路權/避車) 只有Normal(=-1)/Stop(!=-1) 兩種訊號.
            /// 3. beamSensor : 有Normal/LowSpeed/Stop 三種訊號,由於只是訊號來源,在無法Stop的地方或把訊號切成LowSpeed.
            /// 4. BumpSensorState : 只有Normal/Stop 兩種訊號. 目前觸發下去後會伴隨軸異常.復規方式串到EMS流程.
            /// 5. localPauseStatus : 只有Normal/Stop 兩種訊號,會串到台車上的實體按鈕.
            /// 6. AxisError : 軸異常,由於EMS會對Elmo驅動器下Y點,會導致軸異常,因此軸異常會當作停止訊號,如果軸異常後config秒內沒有收到Bumper/EMO/EMS訊號,會觸發走行軸異常AlarmCode.
            /// 7. EMS : 只有Normal/Stop 兩種訊號. 軟體急停, 目前無作用,僅套用復歸邏輯使用.
            /// 8. EMO : 只有Normal/Stop 兩種訊號. 硬體急停, 目前EMO套用軟體急停(對Elmo下Y點為急停,非斷電),套用EMS復歸邏輯.

            /// 額外類型.
            /// 1. KeepsLowSpeedStateByEQVChange : 進入EQ前的降速,如果原先速度為1000,但事先觸發到了LowSpeed,原本降速的位置會太早,因此保留當時訊號,把降EQ.Velocity的位置往後拉.
            /// 2. ontimeReviseLowSpeed : 直線偏差製一定差距時,進行降速修正.

            if (localData.MoveControlData.MoveCommand.WaitReserveIndex != -1 && localData.MoveControlData.MoveCommand.ReserveList[localData.MoveControlData.MoveCommand.WaitReserveIndex].GetReserve)
            {
                WriteLog(7, "", String.Concat("因取得Reserve index : ", localData.MoveControlData.MoveCommand.WaitReserveIndex, ", WaitReserve 變回-1"));
                localData.MoveControlData.MoveCommand.WaitReserveIndex = -1;
            }

            if ((int)localData.MIPCData.SafetySensorStatus >= (int)EnumSafetyLevel.EMS || (localData.MoveControlData.MoveCommand != null && localData.MoveControlData.MoveCommand.EMSResetStatus != EnumEMSResetFlow.None))
                sensorState = EnumVehicleSafetyAction.EMS;
            else if (localData.MoveControlData.MoveCommand.AGVPause == EnumVehicleSafetyAction.SlowStop || localData.MoveControlData.MoveCommand.WaitReserveIndex != -1 ||
                     localData.MoveControlData.MoveCommand.Cancel || localData.MoveControlData.SensorStatus.LocalPause == EnumVehicleSafetyAction.SlowStop ||
                     localData.MIPCData.SafetySensorStatus == EnumSafetyLevel.SlowStop)
                sensorState = EnumVehicleSafetyAction.SlowStop;
            else if (localData.MIPCData.SafetySensorStatus == EnumSafetyLevel.LowSpeed_Low)
                sensorState = EnumVehicleSafetyAction.LowSpeed_Low;
            else if (localData.MIPCData.SafetySensorStatus == EnumSafetyLevel.LowSpeed_High)
                sensorState = EnumVehicleSafetyAction.LowSpeed_High;
            else
                sensorState = EnumVehicleSafetyAction.Normal;

            if (localData.MoveControlData.MoveCommand.KeepsLowSpeedStateByEQVChange != EnumVehicleSafetyAction.SlowStop &&
                sensorState != EnumVehicleSafetyAction.SlowStop && sensorState != EnumVehicleSafetyAction.EMS)
                sensorState = localData.MoveControlData.MoveCommand.KeepsLowSpeedStateByEQVChange;

            return sensorState;
        }

        #region 訊號延遲.
        private EnumVehicleSafetyAction ProcessSensorState(EnumVehicleSafetyAction sensorState)
        {
            if (sensorState == localData.MoveControlData.MoveCommand.SensorStatus)
                return sensorState;

            if (sensorState == EnumVehicleSafetyAction.EMS)
                return sensorState;
            else if (sensorState == EnumVehicleSafetyAction.SlowStop)
            {
                if (localData.MoveControlData.MotionControlData.MoveStatus != EnumAxisMoveStatus.Move)
                    return localData.MoveControlData.MoveCommand.SensorStatus;
            }
            else if (localData.MoveControlData.MoveCommand.SensorStatus == EnumVehicleSafetyAction.SlowStop || localData.MoveControlData.MoveCommand.SensorStatus == EnumVehicleSafetyAction.EMS)
            {
                if (localData.MoveControlData.MotionControlData.MoveStatus != EnumAxisMoveStatus.Stop)
                    return localData.MoveControlData.MoveCommand.SensorStatus;
            }
            else
            {
                if (localData.MoveControlData.MotionControlData.MoveStatus != EnumAxisMoveStatus.Move)
                    return localData.MoveControlData.MoveCommand.SensorStatus;
            }

            return sensorState;
        }
        #endregion

        public EnumVehicleSafetyAction UpdateSensorState()
        {
            return ProcessSensorState(GetSensorState());
        }
    }
}