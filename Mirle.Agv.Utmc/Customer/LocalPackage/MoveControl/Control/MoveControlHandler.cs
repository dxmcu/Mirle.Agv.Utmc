using Mirle.Agv.INX.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Mirle.Agv.INX.Model.Configs;
using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Controller;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace Mirle.Agv.INX.Controller
{
    public class MoveControlHandler
    {
        private ComputeFunction computeFunction = ComputeFunction.Instance;
        private LoggerAgent loggerAgent = LoggerAgent.Instance;
        private LocalData localData = LocalData.Instance;
        private string device = MethodInfo.GetCurrentMethod().ReflectedType.Name;
        private string normalLogName = "MoveControl";
        //private string errorLogName = "";
        public EnumControlStatus Status { get; set; } = EnumControlStatus.Ready;

        private Logger logger = LoggerAgent.Instance.GetLooger("MoveControlCSV");
        private Logger preCheckRecordLogger = LoggerAgent.Instance.GetLooger("PreCheckRecord");

        private Logger lineLogger = LoggerAgent.Instance.GetLooger("LineCSV");
        private Logger spinTurnLogger = LoggerAgent.Instance.GetLooger("SpinTurnCSV");

        public CreateMoveControlList CreateMoveCommandList { get; set; }
        private MIPCControlHandler mipcControl;
        private AlarmHandler alarmHandler;


        private Stopwatch mainThreadSleepTimer = new Stopwatch();
        private Thread thread;
        private Thread csvThread;

        private MoveCommandData preCommand = null;

        public event EventHandler<EnumMoveComplete> MoveCompleteEvent;
        public event EventHandler<string> PassAddressEvent;

        public MotionControlHandler MotionControl { get; set; }
        public LocateControlHandler LocateControl { get; set; }
        private UpdateControlHandler updateControl;
        private SensorSafetyControl sensorSafetyControl;

        private const int debugFlowLogMaxLength = 10000;
        public string DebugFlowLog { get; set; }

        public double LoopTime { get; set; } = 0;

        private MoveControlConfig config;

        private bool resetAlarm = false;

        public void StartCommand()
        {
            MoveCommandData temp = localData.MoveControlData.MoveCommand;

            if (temp != null && temp.CommandStatus == EnumMoveCommandStartStatus.WaitStart)
                temp.CommandStatus = EnumMoveCommandStartStatus.Start;
        }

        #region Read-XML.
        private void ReadTimeoutValueListXML(XmlElement element)
        {
            EnumTimeoutValueType temp;
            int value;

            foreach (XmlNode item in element.ChildNodes)
            {
                if (int.TryParse(item.InnerText, out value) && Enum.TryParse(item.Name, out temp))
                    config.TimeValueConfig.TimeoutValueList[temp] = value;
                else
                    WriteLog(3, "", String.Concat("TryPase fail, Name : ", item.Name, ", Value : ", item.InnerText));
            }
        }

        private void ReadIntervalTimeListXML(XmlElement element)
        {
            EnumIntervalTimeType temp;
            int value;

            foreach (XmlNode item in element.ChildNodes)
            {
                if (int.TryParse(item.InnerText, out value) && Enum.TryParse(item.Name, out temp))
                    config.TimeValueConfig.IntervalTimeList[temp] = value;
                else
                    WriteLog(3, "", String.Concat("TryPase fail, Name : ", item.Name, ", Value : ", item.InnerText));
            }
        }

        private void ReadDelayTimeListXML(XmlElement element)
        {
            EnumDelayTimeType temp;
            int value;

            foreach (XmlNode item in element.ChildNodes)
            {
                if (int.TryParse(item.InnerText, out value) && Enum.TryParse(item.Name, out temp))
                    config.TimeValueConfig.DelayTimeList[temp] = value;
                else
                    WriteLog(3, "", String.Concat("TryPase fail, Name : ", item.Name, ", Value : ", item.InnerText));
            }
        }

        private void ReadTimeValueConfigXML(string path)
        {
            if (path == null || path == "")
            {
                WriteLog(1, "", "TimeValueConfig.xml 路徑錯誤為null或空值,請檢查程式內部的string.");
                return;
            }

            XmlDocument doc = new XmlDocument();

            if (!File.Exists(path))
            {
                WriteLog(1, "", "找不到TimeValueConfig.xml.");
                return;
            }

            doc.Load(path);
            XmlElement rootNode = doc.DocumentElement;

            string locatePath = new DirectoryInfo(path).Parent.FullName;

            foreach (XmlNode item in rootNode.ChildNodes)
            {
                switch (item.Name)
                {
                    case "TimeoutValueList":
                        ReadTimeoutValueListXML((XmlElement)item);
                        break;
                    case "IntervalTimeList":
                        ReadIntervalTimeListXML((XmlElement)item);
                        break;
                    case "DelayTimeList":
                        ReadDelayTimeListXML((XmlElement)item);
                        break;
                    default:
                        break;
                }
            }
        }

        private void ReadMoveControlConfigXML(string path)
        {
            try
            {
                config = new MoveControlConfig();
                localData.MoveControlData.MoveControlConfig = config;

                if (path == null || path == "")
                {
                    WriteLog(1, "", "MoveControlConfig 路徑錯誤為null或空值,請檢查程式內部的string.");
                    return;
                }

                XmlDocument doc = new XmlDocument();

                if (!File.Exists(path))
                {
                    WriteLog(1, "", "找不到moveControlConfig.xml.");
                    return;
                }

                doc.Load(path);
                XmlElement rootNode = doc.DocumentElement;

                string locatePath = new DirectoryInfo(path).Parent.FullName;

                foreach (XmlNode item in rootNode.ChildNodes)
                {
                    switch (item.Name)
                    {
                        case "AGVType":
                            config.AGVType = item.InnerText;
                            break;
                        case "VChangeWitchIsokinetic":
                            config.VChangeWitchIsokinetic = bool.Parse(item.InnerText);
                            break;
                        case "UsingSimulateVelocity":
                            config.UsingSimulateVelocity = bool.Parse(item.InnerText);
                            break;
                        case "SafetyStopInTurningWithDelaying":
                            config.SafetyStopInTurningWithDelaying = bool.Parse(item.InnerText);
                            break;
                        case "LosePositionSetNullAddressSection":
                            config.LosePositionSetNullAddressSection = bool.Parse(item.InnerText);
                            break;
                        case "InPositionRange":
                            config.InPositionRange = double.Parse(item.InnerText);
                            break;
                        case "SectionWidthRange":
                            config.SectionWidthRange = double.Parse(item.InnerText);
                            break;
                        case "SectionRange":
                            config.SectionRange = double.Parse(item.InnerText);
                            break;
                        case "SectionAllowDeltaTheta":
                            config.SectionAllowDeltaTheta = double.Parse(item.InnerText);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        private SafetyData ReadSafetyDataXML(XmlElement element)
        {
            SafetyData temp = new SafetyData();

            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "Enable":
                        temp.Enable = bool.Parse(item.InnerText);
                        break;
                    case "Range":
                        temp.Range = double.Parse(item.InnerText);
                        break;
                    default:
                        break;
                }
            }

            return temp;
        }

        private void ReadSafetyXML(XmlElement element)
        {
            SafetyData temp;
            EnumMoveControlSafetyType enumTemp;

            foreach (XmlNode item in element.ChildNodes)
            {
                if (Enum.TryParse(item.Name, out enumTemp))
                {
                    temp = new SafetyData();
                    temp = ReadSafetyDataXML((XmlElement)item);
                    config.Safety[enumTemp] = temp;
                }
            }
        }

        private void ReadSensorByBpassXML(XmlElement element)
        {
            EnumSensorSafetyType enumTemp;

            foreach (XmlNode item in element.ChildNodes)
            {
                if (Enum.TryParse(item.Name, out enumTemp))
                    config.SensorByPass[enumTemp] = (string.Compare(item.InnerText, "enable", true) == 0);
            }
        }

        private void ReadSensorSafetyConfigXML(string path)
        {
            try
            {
                if (path == null || path == "")
                {
                    WriteLog(1, "", "MoveControlConfig 路徑錯誤為null或空值,請檢查程式內部的string.");
                    return;
                }

                XmlDocument doc = new XmlDocument();

                if (!File.Exists(path))
                {
                    WriteLog(1, "", "找不到moveControlConfig.xml.");
                    return;
                }

                doc.Load(path);
                XmlElement rootNode = doc.DocumentElement;

                string locatePath = new DirectoryInfo(path).Parent.FullName;

                foreach (XmlNode item in rootNode.ChildNodes)
                {
                    switch (item.Name)
                    {
                        case "Safety":
                            ReadSafetyXML((XmlElement)item);
                            break;
                        case "SensorByPass":
                            ReadSensorByBpassXML((XmlElement)item);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        private void ReadXML()
        {
            ReadMoveControlConfigXML(@"D:\MecanumConfigs\MoveControl\MoveControlConfig.xml");
            ReadSensorSafetyConfigXML(@"D:\MecanumConfigs\MoveControl\SensorSafetyConfig.xml");
            ReadTimeValueConfigXML(@"D:\MecanumConfigs\MoveControl\TimeValueConfig.xml");
        }
        #endregion

        public MoveControlHandler(MIPCControlHandler mipcControl, AlarmHandler alarmHandler)
        {
            this.alarmHandler = alarmHandler;
            this.mipcControl = mipcControl;

            localData.MoveControlData.SimulateBypassLog = true;

            ReadXML();
            LocateControl = new LocateControlHandler(alarmHandler, "Locate");
            updateControl = new UpdateControlHandler(alarmHandler, normalLogName);

            CreateMoveCommandList = new CreateMoveControlList(alarmHandler, normalLogName);
            MotionControl = new MotionControlHandler(mipcControl, alarmHandler, "Motion");
            CreateMoveCommandList.SimulateControl = MotionControl.SimulateControl;
            sensorSafetyControl = new SensorSafetyControl(normalLogName);
            mainThreadSleepTimer.Restart();
            thread = new Thread(MoveControlThread);
            thread.Start();
            csvThread = new Thread(WriteLogCSV);
            csvThread.Start();
        }

        #region Close.
        public void CloseMoveControlHandler()
        {
            Status = EnumControlStatus.Closing;

            Stopwatch closeTimer = new Stopwatch();
            closeTimer.Restart();

            VehicleStop();

            while (localData.MoveControlData.MoveCommand != null)
            {
                if (closeTimer.ElapsedMilliseconds > config.TimeValueConfig.TimeoutValueList[EnumTimeoutValueType.EndTimeoutValue])
                {
                    // log.
                    // System EMS.
                    break;
                }

                Thread.Sleep(10);
            }

            Status = EnumControlStatus.WaitThreadStop;

            closeTimer.Restart();
            while (thread != null && thread.IsAlive)
            {
                if (closeTimer.ElapsedMilliseconds > config.TimeValueConfig.TimeoutValueList[EnumTimeoutValueType.EndTimeoutValue])
                {
                    // log.
                    // abort 
                    break;
                }

                Thread.Sleep(10);
            }

            Status = EnumControlStatus.Closed;
        }
        #endregion

        #region AlarmCode/WritleLog.
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

        private void SetDebugFlowLog(string functionName, string message)
        {
            DebugFlowLog = String.Concat(DateTime.Now.ToString("HH:mm:ss.fff"), "\t", functionName, "\t", message, "\r\n", DebugFlowLog);

            if (DebugFlowLog.Length > debugFlowLogMaxLength)
                DebugFlowLog = DebugFlowLog.Substring(0, debugFlowLogMaxLength);
        }

        public void WriteLog(int logLevel, string carrierId, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            LogFormat logFormat = new LogFormat(normalLogName, logLevel.ToString(), memberName, device, carrierId, message);

            loggerAgent.Log(logFormat.Category, logFormat);
            SetDebugFlowLog(memberName, message);

            if (logLevel <= localData.ErrorLevel)
            {
                logFormat = new LogFormat(localData.ErrorLogName, logLevel.ToString(), memberName, device, carrierId, message);
                loggerAgent.Log(logFormat.Category, logFormat);
            }
        }
        #endregion

        #region ResetAlarm.
        public void ResetAlarm()
        {
            WriteLog(7, "", "收到ResetAlarm");
            resetAlarm = true;
        }

        private void CheckResetAlarm()
        {
            if (resetAlarm && localData.MoveControlData.SensorStatus.ForkReady)
            {
                resetAlarm = false;

                if (localData.MoveControlData.MoveCommand != null && localData.MoveControlData.MoveCommand.EMSResetStatus == EnumEMSResetFlow.EMS_WaitReset)
                {
                    localData.MoveControlData.MoveCommand.EMSResetStatus = EnumEMSResetFlow.EMS_WaitStart;
                    WriteLog(7, "", String.Concat("EMSResetStatus 切換成 ", localData.MoveControlData.MoveCommand.EMSResetStatus.ToString()));
                }

                if (localData.AutoManual == EnumAutoState.Manual)
                {
                    if (localData.MoveControlData.MoveCommand == null)
                    {
                        ChangeMovingDirection(EnumMovingDirection.None);
                        ChangeBuzzerType(EnumBuzzerType.None);
                        ChangeDirectionLight(EnumDirectionLight.None);
                    }

                    MotionControl.ResetAlarm();
                    LocateControl.ResetAlarm();
                    mipcControl.ResetAlarm();

                    localData.MoveControlData.ErrorBit = false;
                }
            }

            if (localData.MoveControlData.MoveCommand != null && localData.MoveControlData.MoveCommand.EMSResetStatus == EnumEMSResetFlow.EMS_WaitStart && localData.MIPCData.Start)
            {
                localData.MoveControlData.MoveCommand.EMSResetStatus = EnumEMSResetFlow.None;
                WriteLog(7, "", String.Concat("EMSResetStatus 切換成 ", localData.MoveControlData.MoveCommand.EMSResetStatus.ToString()));
            }
        }
        #endregion

        #region Update MoveControl Ready & Command.
        private void UpdateCanAuto()
        {
            if (localData.AutoManual == EnumAutoState.Auto)
            {
                localData.MoveControlData.MoveControlCanAuto = true;
                localData.MoveControlData.MoveControlCantAutoReason = "";
            }
            else if (!localData.MoveControlData.Ready)
            {
                localData.MoveControlData.MoveControlCanAuto = false;
                localData.MoveControlData.MoveControlCantAutoReason = localData.MoveControlData.MoveControlNotReadyReason;
            }
            else if (localData.MoveControlData.ErrorBit)
            {
                localData.MoveControlData.MoveControlCanAuto = false;
                localData.MoveControlData.MoveControlCantAutoReason = "MoveControl ErrorBit On";
            }
            else if (localData.Real == null)
            {
                localData.MoveControlData.MoveControlCanAuto = false;
                localData.MoveControlData.MoveControlCantAutoReason = "MoveControl 迷航中(Real)";
            }
            else
            {
                VehicleLocation temp = localData.Location;

                if (temp == null)
                {
                    localData.MoveControlData.MoveControlCanAuto = false;
                    localData.MoveControlData.MoveControlCantAutoReason = "MoveControl 迷航中(Address、Section)";
                }
                else if (localData.TheMapInfo.AllAddress.ContainsKey(temp.LastAddress) && localData.TheMapInfo.AllSection.ContainsKey(temp.NowSection))
                {
                    if (localData.TheMapInfo.AllSection[temp.NowSection].FromVehicleAngle != localData.TheMapInfo.AllSection[temp.NowSection].ToVehicleAngle && !temp.InAddress)
                    {
                        localData.MoveControlData.MoveControlCanAuto = false;
                        localData.MoveControlData.MoveControlCantAutoReason = "MoveControl 在RTurn路線上";
                    }
                    else
                    {
                        ThetaSectionDeviation revise = localData.MoveControlData.ThetaSectionDeviation;

                        if (revise == null)
                        {
                            localData.MoveControlData.MoveControlCanAuto = false;
                            localData.MoveControlData.MoveControlCantAutoReason = "MoveControl 迷航中(偏差數值)";
                        }
                        else
                        {
                            if (Math.Abs(revise.Theta) <= config.Safety[EnumMoveControlSafetyType.OntimeReviseTheta].Range / 2 &&
                                Math.Abs(revise.SectionDeviation) <= config.Safety[EnumMoveControlSafetyType.OntimeReviseTheta].Range / 2)
                            {
                                localData.MoveControlData.MoveControlCanAuto = true;
                                localData.MoveControlData.MoveControlCantAutoReason = "";
                            }
                            else
                            {
                                localData.MoveControlData.MoveControlCanAuto = false;
                                localData.MoveControlData.MoveControlCantAutoReason = "MoveControl 偏離路線";
                            }
                        }
                    }
                }
                else
                {
                    localData.MoveControlData.MoveControlCanAuto = false;
                    localData.MoveControlData.MoveControlCantAutoReason = "MoveControl 迷航中(Address、Section)";
                }
            }
        }

        private void UpdateMoveControlReadyAndCommand(bool isMainForLoopCall)
        {
            bool resetPreCommand = false;

            if (isMainForLoopCall)
            {
                if (localData.MoveControlData.MoveCommand != null && localData.MoveControlData.MoveCommand.CommandStatus == EnumMoveCommandStartStatus.End)
                {
                    WriteLog(7, "", String.Concat("移動完成上報結束, 切回無命令狀態"));
                    localData.MoveControlData.MoveCommand = null;
                }
                else if (localData.MoveControlData.MoveCommand == null && preCommand != null)
                {
                    WriteLog(7, "", String.Concat("Command : ", preCommand.CommandID, " 切換至moveCommand"));
                    localData.MoveControlData.MoveCommand = preCommand;
                    localData.MoveControlData.MoveCommand.WaitReserveIndex = localData.MoveControlData.MoveCommand.CommandList[0].ReserveNumber;
                    preCommand = null;
                    resetPreCommand = true;
                }

                if (localData.MoveControlData.MoveCommand != null && localData.MoveControlData.MoveCommand.CommandStatus == EnumMoveCommandStartStatus.WaitStart && localData.MoveControlData.MoveCommand.VehicleStopFlag)
                {
                    WriteLog(7, "", String.Concat("WaitStart 因VehcileStop flag, 切回無命令狀態"));
                    localData.MoveControlData.MoveCommand = null;
                }
            }

            if (localData.MoveControlData.MoveCommand != null)
            {
                localData.MoveControlData.Ready = false;
                localData.MoveControlData.MoveControlNotReadyReason = "命令移動中";
            }
            else if (localData.MoveControlData.MotionControlData.JoystickMode)
            {
                localData.MoveControlData.Ready = false;
                localData.MoveControlData.MoveControlNotReadyReason = "搖桿控制中";
            }
            else if (localData.MoveControlData.SensorStatus.Charging)
            {
                localData.MoveControlData.Ready = false;
                localData.MoveControlData.MoveControlNotReadyReason = "充電中";
            }
            else if (!localData.MoveControlData.SensorStatus.ForkReady)
            {
                localData.MoveControlData.Ready = false;
                localData.MoveControlData.MoveControlNotReadyReason = "Fork不在原點";
            }
            else if (Status != EnumControlStatus.Ready)
            {
                localData.MoveControlData.Ready = false;
                localData.MoveControlData.MoveControlNotReadyReason = "程式關閉中";
            }
            else if (LocateControl.Status != EnumControlStatus.Ready)
            {
                localData.MoveControlData.Ready = false;
                localData.MoveControlData.MoveControlNotReadyReason = "定位裝置Not Ready";
            }
            else if (MotionControl.Status != EnumControlStatus.Ready)
            {
                localData.MoveControlData.Ready = false;
                localData.MoveControlData.MoveControlNotReadyReason = "Motion Not Ready";
            }
            else
            {
                localData.MoveControlData.Ready = true;
                localData.MoveControlData.MoveControlNotReadyReason = "";
            }

            if (resetPreCommand)
                localData.MoveControlData.CreateCommanding = false;
        }
        #endregion

        private void PollingAllData(bool isMainForLoopCall)
        {
            mipcControl.MoveControlHeartBeat++;
            LocateControl.UpdateLocateControlData();
            MotionControl.UpdateMotionControlData();

            UpdateMoveControlReadyAndCommand(isMainForLoopCall);
            UpdateCanAuto();
            updateControl.UpdateAllData((localData.MoveControlData.MoveCommand == null ? null : localData.MoveControlData.MoveCommand.SectionLineList[localData.MoveControlData.MoveCommand.IndexOflisSectionLine]));
        }

        #region CommandControl
        private void CommandControl_Move(Command data)
        {
            EnumMoveStartType moveType = (EnumMoveStartType)data.Type;
            WriteLog(7, "", String.Concat("目前位置 : ", computeFunction.GetMapAGVPositionStringWithAngle(data.TriggerAGVPosition),
                                          ", 目標位置 : ", computeFunction.GetMapAGVPositionStringWithAngle(data.EndAGVPosition),
                                           ", vel : ", data.Velocity.ToString("0"), ", moveType : ", moveType.ToString()));

            if (data.Velocity < localData.MoveControlData.CreateMoveCommandConfig.EQ.Velocity)
                WriteLog(3, "", String.Concat("Fatal Error : Move Velcotiy < EQ.Velocity : ", data.Velocity.ToString("0.0")));

            localData.MoveControlData.MoveCommand.EndAGVPosition = data.EndAGVPosition;

            if (moveType == EnumMoveStartType.FirstMove)
            {
                SetMovingDirectionByEndAGVPosition(data.EndAGVPosition);
                SetBuzzerTypeByEndAGVPosition(data.EndAGVPosition);
                SetDirectionLightByEndAGVPosition(data.EndAGVPosition);

                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                timer.Restart();

                #region Enable All Axis.
                WriteLog(7, "", "為第一次移動, ServoOn All");
                MotionControl.ServoOn_All();

                while (!MotionControl.AllServoOn)
                {
                    if (timer.ElapsedMilliseconds > config.TimeValueConfig.TimeoutValueList[EnumTimeoutValueType.EnableTimeoutValue])
                    {
                        EMSControl(EnumMoveCommandControlErrorCode.Move_EnableTimeout);
                        return;
                    }

                    IntervalSleepAndPollingAllData();
                }
                #endregion

                #region 喊前進Config ms.
                //if (MoveControlVehicleData.CommandFlags.MoveStartNoWaitTime)
                //{
                //    WriteLog(7, "", "為Override的First Move移動,因此取消等待2秒!");
                //    MoveControlVehicleData.CommandFlags.MoveStartNoWaitTime = false;
                //}
                //else
                {
                    while (timer.ElapsedMilliseconds < config.TimeValueConfig.DelayTimeList[EnumDelayTimeType.CommandStartDelayTime])
                        IntervalSleepAndPollingAllData();
                }
                #endregion
            }
            else if (moveType == EnumMoveStartType.ChangeDirFlagMove)
            {
                CommandControl_ChangeSection();
                #region 設定方向燈和BeamSensor.
                #endregion
            }

            localData.MoveControlData.MoveCommand.SensorStatus = sensorSafetyControl.GetSensorState();

            if (moveType != EnumMoveStartType.SensorStopMove)
                localData.MoveControlData.MoveCommand.NormalVelocity = data.Velocity;

            if (localData.MoveControlData.MoveCommand.SensorStatus == EnumVehicleSafetyAction.SlowStop)
            {
                localData.MoveControlData.MoveCommand.NowVelocity = 0;
                WriteLog(7, "", "由於啟動時 Sensor State 為SlowStop,因此不啟動!");
            }
            else if (localData.MoveControlData.MoveCommand.SensorStatus == EnumVehicleSafetyAction.EMS)
            {
                localData.MoveControlData.MoveCommand.EMSResetStatus = EnumEMSResetFlow.EMS_WaitReset;
                WriteLog(7, "", String.Concat("EMSResetStatus 切換成 ", localData.MoveControlData.MoveCommand.EMSResetStatus.ToString()));
                localData.MoveControlData.MoveCommand.NowVelocity = 0;
                WriteLog(7, "", "由於啟動時 Sensor State 為EMS,因此不啟動!");
            }
            else
            {
                double vChangeVelocity = data.Velocity;

                if (localData.MoveControlData.MoveCommand.SensorStatus == EnumVehicleSafetyAction.LowSpeed_High)
                {
                    if (vChangeVelocity > localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_High)
                        vChangeVelocity = localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_High;
                }
                else if (localData.MoveControlData.MoveCommand.SensorStatus == EnumVehicleSafetyAction.LowSpeed_Low)
                {
                    if (vChangeVelocity > localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_High)
                        vChangeVelocity = localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_High;
                }

                if (!MotionControl.Move_Line(data.EndAGVPosition, data.Velocity))
                {
                    EMSControl(EnumMoveCommandControlErrorCode.MoveMethod層_DriverReturnFalse);
                    return;
                }

                localData.MoveControlData.MoveCommand.NowVelocity = vChangeVelocity;
            }

            WriteLog(7, "", "end");
        }

        private void CommandControl_VChange(Command data)
        {
            EnumVChangeType vChangeType = (EnumVChangeType)data.Type;
            WriteLog(7, "", String.Concat("start, Velocity : ", data.Velocity.ToString("0")));

            #region 如果降速類別為終點站或反折點,避免因為BeamSensor降速過但降速點不變造成80速度走行過長的狀況
            //if (MoveControlVehicleData.CommandFlags.SensorState != EnumVehicleSafetyAction.Stop && (vChangeType == EnumVChangeType.EQ || vChangeType == EnumVChangeType.SlowStop))
            //{
            //    if (data.NowVelocity != 0 && data.NowVelocity > MoveControlVehicleData.CommandFlags.RealVelocity)
            //    {
            //        double oldDistance = CreateMoveCommandList.GetAccDecDistanceFormMove(data.NowVelocity, data.Velocity);
            //        double newDistance = CreateMoveCommandList.GetAccDecDistanceFormMove(MoveControlVehicleData.CommandFlags.RealVelocity, data.Velocity);

            //        double triggerEncoder = command.CommandList[command.IndexOfCmdList].TriggerEncoder + (MoveControlVehicleData.CommandFlags.DirFlag ? oldDistance - newDistance : -(oldDistance - newDistance));
            //        WriteLog(7, "", String.Concat("vChange Type : ", vChangeType.ToString(), " 時因為目前Velcotiy比預計的低,因此延後 ", Math.Abs(data.TriggerEncoder - triggerEncoder).ToString("0"), "mm 進行降速!"));
            //        Command temp = CreateMoveCommandList.NewVChangeCommand(command.CommandList[command.IndexOfCmdList].Position, triggerEncoder, data.Velocity, MoveControlVehicleData.CommandFlags.DirFlag, vChangeType);
            //        temp.NowVelocity = MoveControlVehicleData.CommandFlags.RealVelocity;
            //        command.CommandList.Insert(command.IndexOfCmdList + 1, temp);

            //        MoveControlVehicleData.CommandFlags.KeepsLowSpeedStateByEQVChange = MoveControlVehicleData.CommandFlags.SensorState;
            //        MoveControlVehicleData.CommandFlags.VelocityCommand = MoveControlVehicleData.CommandFlags.RealVelocity;
            //        WriteLog(7, "", "end");
            //        return;
            //    }

            //    MoveControlVehicleData.CommandFlags.KeepsLowSpeedStateByEQVChange = EnumVehicleSafetyAction.Stop;
            //}
            #endregion

            if (vChangeType != EnumVChangeType.SensorSlow)
                localData.MoveControlData.MoveCommand.NormalVelocity = data.Velocity;

            #region 下變速指令.
            if (localData.MoveControlData.MoveCommand.SensorStatus != EnumVehicleSafetyAction.SlowStop)
            {
                double vChangeVelocity = data.Velocity;

                if (localData.MoveControlData.MoveCommand.SensorStatus == EnumVehicleSafetyAction.LowSpeed_High)
                {
                    if (vChangeVelocity > localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_High)
                        vChangeVelocity = localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_High;
                }
                else if (localData.MoveControlData.MoveCommand.SensorStatus == EnumVehicleSafetyAction.LowSpeed_Low)
                {
                    if (vChangeVelocity > localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_Low)
                        vChangeVelocity = localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_Low;
                }

                if (vChangeVelocity != localData.MoveControlData.MoveCommand.NowVelocity)
                {
                    double finalVelocity = vChangeVelocity;

                    if (vChangeType != EnumVChangeType.STurn && vChangeType != EnumVChangeType.RTurn &&
                        vChangeVelocity > localData.MoveControlData.MoveCommand.NowVelocity &&
                        vChangeVelocity > localData.MoveControlData.CreateMoveCommandConfig.EQ.Velocity)
                        finalVelocity = CreateMoveCommandList.GetVChangeVelocity(localData.MoveControlData.MoveCommand.NowVelocity, vChangeVelocity, true);

                    if (!MotionControl.Move_VelocityChange(finalVelocity))
                    {
                        EMSControl(EnumMoveCommandControlErrorCode.MoveMethod層_DriverReturnFalse);
                        return;
                    }

                    WriteLog(7, "", String.Concat("VChange Command : ", finalVelocity.ToString("0")));
                    localData.MoveControlData.MoveCommand.NowVelocity = finalVelocity;
                }
            }
            #endregion

            #region 方向燈/BeamSensor/其他東西.
            switch (vChangeType)
            {
                case EnumVChangeType.STurn:
                    ChangeBuzzerType(EnumBuzzerType.Turning);
                    //SetDirectionLightByEndAGVPosition();
                    break;
                case EnumVChangeType.RTurn:
                    ChangeBuzzerType(EnumBuzzerType.Turning);
                    //ChangeDirectionLight();
                    /// 喊轉彎,TR,R2000後記得喊回正常.
                    break;
                case EnumVChangeType.EQ:
                    /// ???
                    break;
                default:
                    break;
            }
            #endregion

            WriteLog(7, "", "end");
        }

        private void CommandControl_ChangeSection()
        {
            WriteLog(7, "", String.Concat("SectionLine : ",
                  localData.MoveControlData.MoveCommand.SectionLineList[localData.MoveControlData.MoveCommand.IndexOflisSectionLine].Section.Id, " cahnge to ",
                  localData.MoveControlData.MoveCommand.SectionLineList[localData.MoveControlData.MoveCommand.IndexOflisSectionLine + 1].Section.Id));

            if (localData.MoveControlData.MoveCommand.IsAutoCommand)
                PassAddressEvent?.Invoke(this, localData.MoveControlData.MoveCommand.SectionLineList[localData.MoveControlData.MoveCommand.IndexOflisSectionLine].End.Id);

            localData.MoveControlData.MoveCommand.IndexOflisSectionLine++;
        }

        private void CommandControl_Stop(Command data)
        {
            if (data.ReserveNumber >= localData.MoveControlData.MoveCommand.ReserveList.Count)
                WriteLog(3, "", "Reserve_指令動作的ReserveIndex超過ReserveList範圍");
            else if (localData.MoveControlData.MoveCommand.ReserveList[data.ReserveNumber].GetReserve)
                WriteLog(7, "", "取得下段Reserve點, 因此直接通過");
            else
            {
                localData.MoveControlData.MoveCommand.WaitReserveIndex = data.ReserveNumber;
                WriteLog(7, "", String.Concat("因未取得Reserve index = ", data.ReserveNumber.ToString(), ", 因此停車 !"));
            }
        }

        private void CommandControl_SlowStop(Command data)
        {
            EnumSlowStopType slowStopType = (EnumSlowStopType)data.Type;
            WriteLog(7, "", String.Concat("start, SensorState : ", localData.MoveControlData.MoveCommand.SensorStatus.ToString(), ", type : ", slowStopType.ToString(), ", NowVelocity : ", localData.MoveControlData.MotionControlData.LineVelocity.ToString("0")));

            MotionControl.Stop_Normal();

            #region 等待停止.
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Restart();


            if (localData.SimulateMode)
            {
                while (localData.MoveControlData.MotionControlData.LineVelocity != 0)
                {
                    if (timer.ElapsedMilliseconds > config.TimeValueConfig.TimeoutValueList[EnumTimeoutValueType.SlowStopTimeoutValue])
                    {
                        EMSControl(EnumMoveCommandControlErrorCode.SlowStop_Timeout);
                        return;
                    }

                    IntervalSleepAndPollingAllData();
                }
            }
            else
            {
                while (localData.MoveControlData.MotionControlData.MoveStatus != EnumAxisMoveStatus.Stop)
                {
                    if (timer.ElapsedMilliseconds > config.TimeValueConfig.TimeoutValueList[EnumTimeoutValueType.SlowStopTimeoutValue])
                    {
                        EMSControl(EnumMoveCommandControlErrorCode.SlowStop_Timeout);
                        return;
                    }

                    IntervalSleepAndPollingAllData();
                }
            }
            #endregion

            #region 如果是變向前的緩停, 檢查是否有超過預停位置且換SectionLine.
            switch (slowStopType)
            {
                case EnumSlowStopType.ChangeMovingAngle:
                    if (Math.Abs(data.EndEncoder - localData.MoveControlData.MoveCommand.CommandEncoder) > localData.MoveControlData.CreateMoveCommandConfig.SafteyDistance[EnumCommandType.Move] / 2)
                    {
                        EMSControl(EnumMoveCommandControlErrorCode.超過觸發區間);
                        return;
                    }

                    //CommandControl_ChangeSection();
                    break;
                default:
                case EnumSlowStopType.End:
                    break;
            }
            #endregion
            WriteLog(7, "", "end");
        }

        private void RecordPreCheckData()
        {
            try
            {

                if (localData.MoveControlData.MoveCommand.EndAddress.Id[0] == '2' && localData.MoveControlData.MoveCommand.EndAddress.Id[1] == '0')
                    ;
                else
                    return;

                /// time,AddressID,X,Y,Theta,RealDeltaX,RealDeltaY,RealDeltaTheta,BarcodeDeltaX,BarcodeDeltaY,BarcodeDeltaTheta
                string preCheckCsvString = String.Concat(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                                                         ",", localData.MoveControlData.MoveCommand.EndAddress.Id,
                                                         ",", localData.MoveControlData.MoveCommand.EndAddress.AGVPosition.Position.X.ToString("0"),
                                                         ",", localData.MoveControlData.MoveCommand.EndAddress.AGVPosition.Position.Y.ToString("0"),
                                                         ",", localData.MoveControlData.MoveCommand.EndAddress.AGVPosition.Angle.ToString("0"));

                LocateAGVPosition now = localData.MoveControlData.LocateControlData.LocateAGVPosition;

                if (now != null)
                    preCheckCsvString = String.Concat(preCheckCsvString,
                                                      ",", (now.AGVPosition.Position.X - localData.MoveControlData.MoveCommand.EndAddress.AGVPosition.Position.X).ToString("0.0"),
                                                      ",", (now.AGVPosition.Position.Y - localData.MoveControlData.MoveCommand.EndAddress.AGVPosition.Position.Y).ToString("0.0"),
                                                      ",", (now.AGVPosition.Angle - localData.MoveControlData.MoveCommand.EndAddress.AGVPosition.Angle).ToString("0.0"));
                else
                    preCheckCsvString = String.Concat(preCheckCsvString, ",,,");

                now = LocateControl.GetLocateAGVPositionByLocateType(EnumLocateDriverType.BarcodeMapSystem);

                if (now != null)
                    preCheckCsvString = String.Concat(preCheckCsvString,
                                                      ",", (now.AGVPosition.Position.X - localData.MoveControlData.MoveCommand.EndAddress.AGVPosition.Position.X).ToString("0.0"),
                                                      ",", (now.AGVPosition.Position.Y - localData.MoveControlData.MoveCommand.EndAddress.AGVPosition.Position.Y).ToString("0.0"),
                                                      ",", (now.AGVPosition.Angle - localData.MoveControlData.MoveCommand.EndAddress.AGVPosition.Angle).ToString("0.0"));
                else
                    preCheckCsvString = String.Concat(preCheckCsvString, ",,,");

                preCheckRecordLogger.LogString(preCheckCsvString);
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        private void CommandControl_End(Command data)
        {
            WriteLog(7, "", String.Concat("start, nowEncoder : ", localData.MoveControlData.MoveCommand.CommandEncoder.ToString("0"), ", endEncoder : ", data.EndEncoder.ToString("0")));

            double endEncoderDelta = data.EndEncoder - localData.MoveControlData.MoveCommand.CommandEncoder;

            #region 終點位置差距超過Config進行原本定位系統二修.
            if (Math.Abs(endEncoderDelta) > localData.MoveControlData.CreateMoveCommandConfig.SecondCorrectionX)
            {
                if (!AutoMoveWithEQVelocity(data.EndAGVPosition))
                    return;

                endEncoderDelta = data.EndEncoder - localData.MoveControlData.MoveCommand.CommandEncoder;
                WriteLog(7, "", String.Concat("修正完 delta = ", endEncoderDelta.ToString("0")));
            }
            #endregion

            /// 使用其他定位三修??
            /// 

            #region Disable MoveAxis.
            MotionControl.ServoOff_All();

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Restart();

            while (!MotionControl.AllServoOff)
            {
                if (timer.ElapsedMilliseconds > config.TimeValueConfig.TimeoutValueList[EnumTimeoutValueType.DisableTimeoutValue])
                {
                    EMSControl(EnumMoveCommandControlErrorCode.End_ServoOffeTimeout);
                    return;
                }

                IntervalSleepAndPollingAllData();
            }
            #endregion

            RecordPreCheckData();
            localData.MoveControlData.MoveCommand.CommandStatus = EnumMoveCommandStartStatus.Reporting;
            VehicleLocation newLocation = new VehicleLocation();
            newLocation.DistanceFormSectionHead = localData.Location.DistanceFormSectionHead;
            newLocation.LastAddress = localData.MoveControlData.MoveCommand.SectionLineList[localData.MoveControlData.MoveCommand.SectionLineList.Count - 1].End.Id;
            newLocation.InAddress = true;
            newLocation.NowSection = localData.Location.NowSection;

            localData.Location = newLocation;
            ReportMoveCommandResult(EnumMoveComplete.End);
            WriteLog(7, "", "end, Move Compelete !");
        }

        private void CommandControl_STurn(Command data)
        {
            CommandControl_ChangeSection();
        }

        private void CommandControl_RTurn(Command data)
        {
            CommandControl_ChangeSection();
            CommandControl_ChangeSection();
        }

        private void CommandControl_SpinTurn(Command data)
        {
            WriteLog(7, "", String.Concat("SpinTurn 目標位置 : ", computeFunction.GetMapAGVPositionStringWithAngle(data.EndAGVPosition)));
            localData.MoveControlData.MoveCommand.MoveStatus = EnumMoveStatus.SpinTurn;
            CommandControl_ChangeSection();

            Stopwatch timer = new Stopwatch();
            timer.Restart();

            double turnAngle = Math.Abs(computeFunction.GetCurrectAngle(localData.Real.Angle - data.EndAGVPosition.Angle));

            EnumVehicleSafetyAction lastStatus = localData.MoveControlData.MoveCommand.SensorStatus;

            ChangeMovingDirection(EnumMovingDirection.SpinTurn);
            ChangeBuzzerType(EnumBuzzerType.SpinTurn);
            ChangeDirectionLight(EnumDirectionLight.SpinTurn);

            if (!MotionControl.AllServoOn)
            {
                WriteLog(7, "", "為第一次移動, ServoOn All");

                timer.Restart();

                #region Enable All Axis.
                MotionControl.ServoOn_All();

                while (!MotionControl.AllServoOn)
                {
                    if (timer.ElapsedMilliseconds > config.TimeValueConfig.TimeoutValueList[EnumTimeoutValueType.EnableTimeoutValue])
                    {
                        EMSControl(EnumMoveCommandControlErrorCode.Move_EnableTimeout);
                        return;
                    }

                    IntervalSleepAndPollingAllData();
                }
                #endregion
            }

            DateTime startTime = DateTime.Now;

            timer.Restart();

            MotionControl.Turn_SpinTurn(data.EndAGVPosition);

            if (!localData.SimulateMode)
            {
                while (!(localData.MoveControlData.MotionControlData.MoveStatus == EnumAxisMoveStatus.Stop && localData.MoveControlData.MoveCommand.SensorStatus != EnumVehicleSafetyAction.SlowStop))
                {
                    if (timer.ElapsedMilliseconds > localData.MoveControlData.MoveControlConfig.TimeValueConfig.TimeoutValueList[EnumTimeoutValueType.SpinTurnFlowTimeoutValue])
                    {
                        EMSControl(EnumMoveCommandControlErrorCode.SpinTurn_Timeout);
                        return;
                    }

                    SensorSafety();

                    if (localData.MoveControlData.MoveCommand.MoveStatus == EnumMoveStatus.Error)
                        return;

                    if (lastStatus != localData.MoveControlData.MoveCommand.SensorStatus)
                    {
                        if (lastStatus == EnumVehicleSafetyAction.SlowStop)
                            timer.Start();
                        else if (localData.MoveControlData.MoveCommand.SensorStatus == EnumVehicleSafetyAction.SlowStop)
                            timer.Stop();
                    }

                    lastStatus = localData.MoveControlData.MoveCommand.SensorStatus;

                    IntervalSleepAndPollingAllData();
                }
            }
            else
            {
                while (timer.ElapsedMilliseconds < localData.MoveControlData.MoveControlConfig.TimeValueConfig.TimeoutValueList[EnumTimeoutValueType.SpinTurnFlowTimeoutValue] / 2)
                    IntervalSleepAndPollingAllData();
            }

            DateTime endTime = DateTime.Now;
            string csvLog = String.Concat(endTime.ToString("yyyy/MM/dd HH:mm:ss.fff"), ",", turnAngle.ToString("0"), ",", (endTime - startTime).TotalSeconds.ToString("0.00"));

            spinTurnLogger.LogString(csvLog);


            localData.MoveControlData.MoveCommand.MoveStatus = EnumMoveStatus.Moving;
            WriteLog(7, "", "end");
        }

        private void SensorStopControl(EnumVehicleSafetyAction status)
        {
            WriteLog(7, "", String.Concat("start, status : ", status.ToString()));

            if (localData.MoveControlData.MoveCommand.MoveStatus == EnumMoveStatus.Stop)
            {
                WriteLog(3, "", String.Concat("狀態已經是Stop, 流程不應該進到這邊."));
            }
            else
            {
                if (status == EnumVehicleSafetyAction.EMS)
                {
                    if (!MotionControl.Stop_EMS())
                        EMSControl(EnumMoveCommandControlErrorCode.MoveMethod層_DriverReturnFalse);
                    else
                    {
                        localData.MoveControlData.MoveCommand.EMSResetStatus = EnumEMSResetFlow.EMS_Stopping;
                        WriteLog(7, "", String.Concat("EMSResetStatus 切換成 ", localData.MoveControlData.MoveCommand.EMSResetStatus.ToString()));
                    }
                }
                else
                {
                    if (localData.MoveControlData.MotionControlData.MoveStatus == EnumAxisMoveStatus.Move ||
                        localData.MoveControlData.MotionControlData.MoveStatus == EnumAxisMoveStatus.PreMove)
                    {
                        if (!MotionControl.Stop_Normal())
                            EMSControl(EnumMoveCommandControlErrorCode.MoveMethod層_DriverReturnFalse);
                    }
                }
            }

            WriteLog(7, "", "end");
        }

        private void SendEMO()
        {

            MotionControl.EMO();
        }

        private void EMSControl(EnumMoveCommandControlErrorCode errorCode)
        {
            WriteLog(7, "", "start, EMS Stop : " + errorCode.ToString());

            localData.MoveControlData.MoveCommand.MoveStatus = EnumMoveStatus.Error;

            double startStopEncoder = localData.MoveControlData.MoveCommand.CommandEncoder;

            if (!MotionControl.Stop_EMS())
                SendEMO();

            while (localData.MoveControlData.MotionControlData.MoveStatus != EnumAxisMoveStatus.Stop &&
                   Math.Abs(localData.MoveControlData.MotionControlData.SimulateLineVelocity -
                            localData.MoveControlData.MotionControlData.LineVelocity) <= config.Safety[EnumMoveControlSafetyType.VChangeSafetyDistance].Range)
                IntervalSleepAndPollingAllData();

            if (localData.MoveControlData.MotionControlData.MoveStatus != EnumAxisMoveStatus.Stop)
            {
                SendEMO();
            }
            else
            {
                MotionControl.ServoOff_All();

                Stopwatch timer = new Stopwatch();
                timer.Restart();

                while (!MotionControl.AllServoOff)
                {
                    if (timer.ElapsedMilliseconds > config.TimeValueConfig.TimeoutValueList[EnumTimeoutValueType.DisableTimeoutValue])
                    {
                        SendAlarmCode(EnumMoveCommandControlErrorCode.End_ServoOffeTimeout);
                        break;
                    }

                    IntervalSleepAndPollingAllData();
                }
            }

            SendAlarmCode(errorCode);

            if (alarmHandler.AllAlarms.ContainsKey((int)(errorCode)) && alarmHandler.AllAlarms[(int)(errorCode)].Level == EnumAlarmLevel.Alarm)
                localData.MoveControlData.ErrorBit = true;

            localData.MoveControlData.MoveCommand.CommandStatus = EnumMoveCommandStartStatus.Reporting;
            ReportMoveCommandResult(EnumMoveComplete.Error);
            WriteLog(7, "", "end");
        }
        #endregion

        #region 上報.
        private void ReportMoveCommandResult(EnumMoveComplete status)
        {
            ChangeMovingDirection(EnumMovingDirection.None);
            ChangeBuzzerType(EnumBuzzerType.None);
            ChangeDirectionLight(EnumDirectionLight.None);

            localData.MoveControlData.AddMoveCOmmandRecordList(localData.MoveControlData.MoveCommand, status);

            if (localData.MoveControlData.MoveCommand.IsAutoCommand)
            {
                MoveCompleteEvent?.Invoke(this, status);

                while (localData.MoveControlData.MoveCommand.CommandStatus == EnumMoveCommandStartStatus.Reporting)
                {
                    IntervalSleepAndPollingAllData();
                }
            }
            else
            {
                localData.MoveControlData.MoveCommand.CommandStatus = EnumMoveCommandStartStatus.End;
            }
        }
        #endregion

        #region AutoMove:EQ.
        public bool AutoMoveWithEQVelocity(MapAGVPosition end)
        {
            if (localData.SimulateMode)
                return true;

            localData.MoveControlData.MoveCommand.NormalVelocity = localData.MoveControlData.CreateMoveCommandConfig.EQ.Velocity;
            localData.MoveControlData.MoveCommand.NowVelocity = localData.MoveControlData.CreateMoveCommandConfig.EQ.Velocity;
            MotionControl.Move_EQ(end);

            Stopwatch timer = new Stopwatch();
            timer.Restart();

            EnumVehicleSafetyAction lastStatus = localData.MoveControlData.MoveCommand.SensorStatus;

            while (!(localData.MoveControlData.MotionControlData.MoveStatus == EnumAxisMoveStatus.Stop && localData.MoveControlData.MoveCommand.SensorStatus != EnumVehicleSafetyAction.SlowStop))
            {
                if (timer.ElapsedMilliseconds > localData.MoveControlData.MoveControlConfig.TimeValueConfig.TimeoutValueList[EnumTimeoutValueType.EndTimeoutValue])
                {
                    EMSControl(EnumMoveCommandControlErrorCode.End_SecondCorrectionTimeout);
                    return false;
                }

                SensorSafety();

                if (localData.MoveControlData.MoveCommand.MoveStatus == EnumMoveStatus.Error)
                    return false;

                if (lastStatus != localData.MoveControlData.MoveCommand.SensorStatus)
                {
                    if (lastStatus == EnumVehicleSafetyAction.SlowStop)
                        timer.Start();
                    else if (localData.MoveControlData.MoveCommand.SensorStatus == EnumVehicleSafetyAction.SlowStop)
                        timer.Stop();
                }

                lastStatus = localData.MoveControlData.MoveCommand.SensorStatus;
                IntervalSleepAndPollingAllData();
            }

            return true;
        }
        #endregion

        #region 檢查觸發.
        private bool TriggerCommand(Command cmd)
        {
            if (localData.MoveControlData.MoveCommand.SensorStatus == EnumVehicleSafetyAction.SlowStop &&
                (cmd.CmdType == EnumCommandType.Move || cmd.CmdType == EnumCommandType.End || cmd.CmdType == EnumCommandType.STurn ||
                 cmd.CmdType == EnumCommandType.RTurn || cmd.CmdType == EnumCommandType.SpinTurn))
            {
                return false;
            }

            if (cmd.TriggerAGVPosition == null)
            {
                WriteLog(7, "", String.Concat("Command : " + cmd.CmdType.ToString() + ", 觸發,為立即觸發"));
                return true;
            }
            else
            {
                if (localData.MoveControlData.MoveCommand.CommandEncoder > cmd.TriggerEncoder + cmd.SafetyDistance)
                {
                    WriteLog(3, "", (String.Concat("Command : ", cmd.CmdType.ToString(), ", 超過Triiger觸發區間,EMS.. ",
                                ", Encoder : ", localData.MoveControlData.MoveCommand.CommandEncoder.ToString("0.0"),
                                ", triggerEncoder : ", cmd.TriggerEncoder.ToString("0.0"))));
                    EMSControl(EnumMoveCommandControlErrorCode.超過觸發區間);
                    return false;
                }
                else if (localData.MoveControlData.MoveCommand.CommandEncoder > cmd.TriggerEncoder)
                {
                    WriteLog(7, "", String.Concat("Command : ", cmd.CmdType.ToString(), ", 觸發, Encoder : ", localData.MoveControlData.MoveCommand.CommandEncoder.ToString("0.0"),
                              ", triggerEncoder : ", cmd.TriggerEncoder.ToString("0.0")));
                    return true;
                }
            }

            return false;
        }
        #endregion

        private void ExecuteCommandList()
        {
            if (localData.MoveControlData.MoveCommand.IndexOfCommandList >= localData.MoveControlData.MoveCommand.CommandList.Count)
            {
                WriteLog(3, "", "IndexOfCommandList == CommandList.Count, 不該觸發");
            }
            else if (TriggerCommand(localData.MoveControlData.MoveCommand.CommandList[localData.MoveControlData.MoveCommand.IndexOfCommandList]))
            {
                WriteLog(7, "", String.Concat("real : ", computeFunction.GetMapAGVPositionStringWithAngle(localData.Real),
                                              ",LocateAGVPosition : ", computeFunction.GetLocateAGVPositionStringWithAngle(localData.MoveControlData.LocateControlData.LocateAGVPosition)));

                localData.MoveControlData.MoveCommand.IndexOfCommandList++;

                switch (localData.MoveControlData.MoveCommand.CommandList[localData.MoveControlData.MoveCommand.IndexOfCommandList - 1].CmdType)
                {
                    case EnumCommandType.Move:
                        CommandControl_Move(localData.MoveControlData.MoveCommand.CommandList[localData.MoveControlData.MoveCommand.IndexOfCommandList - 1]);
                        break;
                    case EnumCommandType.Vchange:
                        CommandControl_VChange(localData.MoveControlData.MoveCommand.CommandList[localData.MoveControlData.MoveCommand.IndexOfCommandList - 1]);
                        break;
                    case EnumCommandType.ChangeSection:
                        CommandControl_ChangeSection();
                        break;
                    case EnumCommandType.STurn:
                        CommandControl_STurn(localData.MoveControlData.MoveCommand.CommandList[localData.MoveControlData.MoveCommand.IndexOfCommandList - 1]);
                        break;
                    case EnumCommandType.RTurn:
                        CommandControl_RTurn(localData.MoveControlData.MoveCommand.CommandList[localData.MoveControlData.MoveCommand.IndexOfCommandList - 1]);
                        break;
                    case EnumCommandType.SpinTurn:
                        CommandControl_SpinTurn(localData.MoveControlData.MoveCommand.CommandList[localData.MoveControlData.MoveCommand.IndexOfCommandList - 1]);
                        break;
                    case EnumCommandType.Stop:
                        CommandControl_Stop(localData.MoveControlData.MoveCommand.CommandList[localData.MoveControlData.MoveCommand.IndexOfCommandList - 1]);
                        break;
                    case EnumCommandType.SlowStop:
                        MotionControl.SetPositionFlag = false;
                        CommandControl_SlowStop(localData.MoveControlData.MoveCommand.CommandList[localData.MoveControlData.MoveCommand.IndexOfCommandList - 1]);
                        MotionControl.SetPositionFlag = true;
                        break;
                    case EnumCommandType.End:
                        CommandControl_End(localData.MoveControlData.MoveCommand.CommandList[localData.MoveControlData.MoveCommand.IndexOfCommandList - 1]);
                        break;
                    default:
                        break;
                }
            }
        }

        private void IntervalSleepAndPollingAllData(bool isMainForLoopCall = false)
        {
            CheckResetAlarm();
            while (mainThreadSleepTimer.ElapsedMilliseconds < config.TimeValueConfig.IntervalTimeList[EnumIntervalTimeType.ThreadSleepTime])
                Thread.Sleep(1);

            LoopTime = mainThreadSleepTimer.ElapsedMilliseconds;

            if (localData.SimulateMode && localData.MoveControlData.SimulateBypassLog)
            {
                if (LoopTime > config.TimeValueConfig.IntervalTimeList[EnumIntervalTimeType.ThreadSleepTime] * 2)
                    WriteLog(3, "", String.Concat("PollingAddData interval : ", LoopTime.ToString("0")));
            }

            mainThreadSleepTimer.Restart();

            PollingAllData(isMainForLoopCall);
        }

        private void MoveControlThread()
        {
            try
            {
                while (Status != EnumControlStatus.WaitThreadStop)
                {
                    if (localData.MoveControlData.MoveCommand != null)
                    {
                        if (localData.MoveControlData.MoveCommand.CommandStatus == EnumMoveCommandStartStatus.Start)
                        {
                            ExecuteCommandList();
                            SensorSafety();
                        }

                        if (localData.MoveControlData.MoveCommand.EMSResetStatus == EnumEMSResetFlow.EMS_Stopping && localData.MoveControlData.MotionControlData.MoveStatus == EnumAxisMoveStatus.Stop)
                        {
                            localData.MoveControlData.MoveCommand.EMSResetStatus = EnumEMSResetFlow.EMS_WaitReset;
                            WriteLog(7, "", String.Concat("EMSResetStatus 切換成 ", localData.MoveControlData.MoveCommand.EMSResetStatus.ToString()));
                        }
                    }

                    IntervalSleepAndPollingAllData(true);
                }
            }
            catch (Exception ex)
            {
                SendAlarmCode(EnumMoveCommandControlErrorCode.MoveControl主Thread跳Exception);
                WriteLog(1, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        #region Safety.
        private void SensorSafety()
        {
            EnumMoveCommandControlErrorCode errorCode = sensorSafetyControl.SensorSafety(MotionControl.Status, LocateControl.Status);

            if (errorCode != EnumMoveCommandControlErrorCode.None)
            {
                EMSControl(errorCode);
            }
            else
            {
                SensorAction(sensorSafetyControl.UpdateSensorState());
            }
        }

        private void SensorAction(EnumVehicleSafetyAction newSensorStatus)
        {
            if (newSensorStatus == localData.MoveControlData.MoveCommand.SensorStatus)
                return;
            else
                WriteLog(7, "", String.Concat("SensorStatus 從 ", localData.MoveControlData.MoveCommand.SensorStatus.ToString(), " 變更為 ", newSensorStatus.ToString()));

            IntervalSleepAndPollingAllData();

            switch (newSensorStatus)
            {
                case EnumVehicleSafetyAction.Normal:
                    SensorActionToNormal();
                    break;
                case EnumVehicleSafetyAction.LowSpeed_Low:
                case EnumVehicleSafetyAction.LowSpeed_High:
                    SensorActionToSlow(newSensorStatus);
                    break;
                case EnumVehicleSafetyAction.SlowStop:
                case EnumVehicleSafetyAction.EMS:
                    SensorStopControl(newSensorStatus);
                    break;
                default:
                    break;
            }

            localData.MoveControlData.MoveCommand.SensorStatus = newSensorStatus;
        }

        private void SensorActionToNormal()
        {
            switch (localData.MoveControlData.MoveCommand.SensorStatus)
            {
                case EnumVehicleSafetyAction.LowSpeed_High:
                    if (localData.MoveControlData.MoveCommand.NormalVelocity > localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_High)
                    {
                        WriteLog(7, "", String.Concat("Sensor切換至Normal,插入升速, velocityCommand : ", localData.MoveControlData.MoveCommand.NormalVelocity.ToString("0")));
                        CommandControl_VChange(CreateMoveCommandList.NewVChangeCommand(null, 0, localData.MoveControlData.MoveCommand.NormalVelocity));
                    }

                    break;
                case EnumVehicleSafetyAction.LowSpeed_Low:
                    if (localData.MoveControlData.MoveCommand.NormalVelocity > localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_Low)
                    {
                        WriteLog(7, "", String.Concat("Sensor切換至Normal,插入升速, velocityCommand : ", localData.MoveControlData.MoveCommand.NormalVelocity.ToString("0")));
                        CommandControl_VChange(CreateMoveCommandList.NewVChangeCommand(null, 0, localData.MoveControlData.MoveCommand.NormalVelocity));
                    }

                    break;
                case EnumVehicleSafetyAction.SlowStop:
                case EnumVehicleSafetyAction.EMS:
                    // 加入啟動.
                    if (localData.MoveControlData.MoveCommand.MoveStatus == EnumMoveStatus.STurn)
                        SensorStartMoveSTurnAction();
                    else if (localData.MoveControlData.MoveCommand.MoveStatus == EnumMoveStatus.RTurn)
                        SensorStartMoveRTurnAction();
                    else if (localData.MoveControlData.MoveCommand.MoveStatus == EnumMoveStatus.SpinTurn)
                        SensorStartMoveSpinTurnAction();
                    else
                        SensorStartMove(EnumVehicleSafetyAction.Normal);
                    break;
                default:
                    break;
            }
        }

        private void SensorActionToSlow(EnumVehicleSafetyAction action)
        {
            switch (localData.MoveControlData.MoveCommand.SensorStatus)
            {
                case EnumVehicleSafetyAction.Normal:
                    // 加入降速.
                    if (localData.MoveControlData.MoveCommand.NowVelocity > localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_High)
                    {
                        WriteLog(7, "", String.Concat("Sensor切換至LowSpeed, 降速至", localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_High.ToString("0")));
                        CommandControl_VChange(CreateMoveCommandList.NewVChangeCommand(null, 0, localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_High, EnumVChangeType.SensorSlow));
                    }
                    else
                        WriteLog(7, "", String.Concat("Sensor切換至LowSpeed,但目前速度小於等於", localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_High.ToString("0"), ", 不做降速"));

                    break;
                case EnumVehicleSafetyAction.SlowStop:
                    // 加入啟動且降速.
                    if (localData.MoveControlData.MoveCommand.MoveStatus == EnumMoveStatus.STurn)
                        SensorStartMoveSTurnAction();
                    else if (localData.MoveControlData.MoveCommand.MoveStatus == EnumMoveStatus.RTurn)
                        SensorStartMoveRTurnAction();
                    else if (localData.MoveControlData.MoveCommand.MoveStatus == EnumMoveStatus.SpinTurn)
                        SensorStartMoveSpinTurnAction();
                    else
                        SensorStartMove(action);
                    break;
                default:
                    break;
            }
        }

        #region SensorStart.
        private void SensorStartMoveSTurnAction()
        {
            EMSControl(EnumMoveCommandControlErrorCode.SlowStop_Timeout);
        }

        private void SensorStartMoveRTurnAction()
        {
            EMSControl(EnumMoveCommandControlErrorCode.SlowStop_Timeout);
        }

        private void SensorStartMoveSpinTurnAction()
        {
            EMSControl(EnumMoveCommandControlErrorCode.SlowStop_Timeout);
        }

        private bool NextCommandIsXXX(EnumCommandType type, ref int index)
        {
            for (int i = localData.MoveControlData.MoveCommand.IndexOfCommandList; i < localData.MoveControlData.MoveCommand.CommandList.Count; i++)
            {
                if (localData.MoveControlData.MoveCommand.CommandList[i].CmdType == type)
                {
                    index = i;
                    return true;
                }
                else if (localData.MoveControlData.MoveCommand.CommandList[i].TriggerAGVPosition != null)
                    return false;
            }

            return false;
        }

        private void SensorStartMove(EnumVehicleSafetyAction nowAction)
        {
            ///狀況1. 下筆命令是移動且無法觸發..當作二修.
            /// 
            ///狀況2. 下筆命令是移動且可以觸發..不做事情. 
            /// 
            ///狀況3. 其他狀況..直接下動令+VChange.
            ///
            ///如果是 SlowVelocity 要加入降速.
            ///
            int index = 0;

            if (NextCommandIsXXX(EnumCommandType.Move, ref index))
            {
                WriteLog(7, "", String.Concat("下筆命令為移動命令,因此進行微修, 終點座標 ", computeFunction.GetMapAGVPositionStringWithAngle(localData.MoveControlData.MoveCommand.CommandList[index].TriggerAGVPosition, "0")));
                AutoMoveWithEQVelocity(localData.MoveControlData.MoveCommand.CommandList[index].TriggerAGVPosition);
            }
            else if (NextCommandIsXXX(EnumCommandType.SpinTurn, ref index))
            {
                WriteLog(7, "", String.Concat("下筆移動為SpinTurn,因此進行微修, 終點座標 ", computeFunction.GetMapAGVPositionStringWithAngle(localData.MoveControlData.MoveCommand.CommandList[index].TriggerAGVPosition, "0")));
                AutoMoveWithEQVelocity(localData.MoveControlData.MoveCommand.CommandList[index].TriggerAGVPosition);
            }
            else if (!NextCommandIsXXX(EnumCommandType.End, ref index))
            {   // 狀況3.
                WriteLog(7, "", String.Concat("一般情況,插入移動命令 : 終點座標 : ", computeFunction.GetMapAGVPositionStringWithAngle(localData.MoveControlData.MoveCommand.EndAGVPosition, "0")));

                double vChangeVelocity = localData.MoveControlData.MoveCommand.NormalVelocity;

                if (nowAction != EnumVehicleSafetyAction.Normal)
                {
                    if (nowAction == EnumVehicleSafetyAction.LowSpeed_High)
                    {
                        if (vChangeVelocity > localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_High)
                            vChangeVelocity = localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_High;
                    }
                    else if (nowAction == EnumVehicleSafetyAction.LowSpeed_Low)
                    {
                        if (vChangeVelocity > localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_Low)
                            vChangeVelocity = localData.MoveControlData.CreateMoveCommandConfig.LowVelocity_Low;
                    }
                }

                double finalVelocity = (vChangeVelocity == localData.MoveControlData.CreateMoveCommandConfig.EQ.Velocity ? vChangeVelocity : CreateMoveCommandList.GetVChangeVelocity(0, vChangeVelocity));


                CommandControl_Move(CreateMoveCommandList.NewMoveCommand(null, localData.MoveControlData.MoveCommand.EndAGVPosition, 0, vChangeVelocity, EnumMoveStartType.SensorStopMove));
            }
            else
            {
                WriteLog(7, "", "二修可觸發,不做事情!");
            }
        }

        #endregion
        #endregion

        #region 外部FunctionCall.
        private bool CheckCanMove(ref string errorMessage)
        {
            if (!localData.MoveControlData.Ready)
            {
                errorMessage = "MoveControl not Ready, 拒絕移動命令";
                SendAlarmCode(EnumMoveCommandControlErrorCode.拒絕移動命令_MoveControlNotReady);

                return false;
            }
            else if (localData.MoveControlData.ErrorBit)
            {
                errorMessage = "MoveControl ErrorBit on, 拒絕移動命令";
                SendAlarmCode(EnumMoveCommandControlErrorCode.拒絕移動命令_MoveControlErrorBitOn);

                return false;
            }
            //else if (MoveControlVehicleData.SensorData.Charging)
            //{
            //    errorMessage = "Charging中,因此無視AGVM Move命令~!";
            //        SendAlarmCode(EnumMoveCommandControlErrorCode.拒絕移動命令_充電中);

            //    return false;
            //}
            //else if (!MoveControlVehicleData.SensorData.ForkHome)
            //{
            //    errorMessage = "Fork不在Home點,因此無視AGVM Move命令~!";
            //    if (sendAlarmCode)
            //        SendAlarmCode(EnumMoveCommandControlErrorCode.拒絕移動命令_Fork不在Home點);

            //    return false;
            //}
            //else if (!MoveControlVehicleData.SimulateMode && (LocateControl.Status != EnumDriverStatus.Ready || MotionCommand.Status != EnumDriverStatus.Ready))
            //{
            //    errorMessage = "有Driver連線失敗,因此無AGVM Move命令~!";
            //    if (sendAlarmCode)
            //        SendAlarmCode(EnumMoveCommandControlErrorCode.拒絕移動命令_MoveControl有Driver狀態不是Ready);

            //}
            else if (localData.Real == null)
            {
                errorMessage = "AGV迷航中(不知道目前在哪),因此無法接受AGVM Move命令!";
                SendAlarmCode(EnumMoveCommandControlErrorCode.拒絕移動命令_迷航中);

                return false;
            }
            else
            {
                //double distance = computeFunction.GetTwoPositionDistance(MoveControlVehicleData.Fake.Position, start.Position);

                //WriteLog(7, "", String.Concat("起點和目前位置安全判斷, 起點 : ( ", start.Position.X.ToString("0"), ", ",
                //                                         start.Position.Y.ToString("0"), " ), 目前位置(Fake) : ( ", MoveControlVehicleData.Fake.Position.X.ToString("0"), ", ",
                //                                         MoveControlVehicleData.Fake.Position.Y.ToString("0"), " ), 距離為 : ", distance.ToString("0")));
                //if (distance > (CreateMoveCommandList.Config.SafteyDistance[EnumCommandType.Move] / 2))
                //{
                //    errorMessage = "起點和目前位置差距過大,因此無視AGVM Move命令~!";
                //    if (sendAlarmCode)
                //        SendAlarmCode(EnumMoveCommandControlErrorCode.拒絕移動命令_和起點座標偏差過大);

                //    return false;
                //}

                //if (Math.Abs(computeFunction.GetCurrectAngle(MoveControlVehicleData.Real.AGVAngle - start.VehicleHeadAngle)) < CreateMoveCommandList.Config.AllowAGVAngleRange)
                //    MoveControlVehicleData.Real.AGVAngle = start.VehicleHeadAngle;
                //else
                //{
                //    errorMessage = "AGV目前角度和起點的角度要求差距過大!";
                //    if (sendAlarmCode)
                //        SendAlarmCode(EnumMoveCommandControlErrorCode.拒絕移動命令_和起點車姿角度偏差過大);

                //    return false;
                //}
            }

            return true;
        }

        public bool VehicleMove(MoveCmdInfo moveCmdInfo, ref string errorMessage)
        {
            if (!SetVehicleMove(moveCmdInfo, ref errorMessage))
                return false;

            StartCommand();

            return true;
        }

        public bool SetVehicleMove(MoveCmdInfo moveCmdInfo, ref string errorMessage)
        {
            try
            {
                lock (localData.MoveControlData.CreateCommandingLockObjcet)
                {
                    if (!CheckCanMove(ref errorMessage))
                        return false;

                    localData.MoveControlData.CreateCommanding = true;
                }

                MoveCommandData command = CreateMoveCommandList.CreateMoveCommand(moveCmdInfo, ref errorMessage);

                if (command != null)
                {
                    preCommand = command;

                    Stopwatch timer = new Stopwatch();
                    timer.Restart();

                    while (localData.MoveControlData.CreateCommanding)
                    {
                        if (timer.ElapsedMilliseconds > 1000)
                        {
                            WriteLog(3, "", String.Concat("等待PreCommand -> Command逾時.."));
                            break;
                        }

                        Thread.Sleep(10);
                    }

                    return true;
                }
                else
                {
                    localData.MoveControlData.CreateCommanding = false;
                    return false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                errorMessage = String.Concat("Exception : ", ex.ToString());
                localData.MoveControlData.CreateCommanding = false;
                return false;
            }
        }

        public bool VehicleMove_DebugForm(MoveCmdInfo moveCmdInfo, ref string errorMessage)
        {
            if (!SetVehicleMove(moveCmdInfo, ref errorMessage))
                return false;

            return true;
        }

        public void VehiclePause()
        {
            MoveCommandData temp = localData.MoveControlData.MoveCommand;

            if (temp == null)
            {
                WriteLog(7, "", String.Concat("收到VehiclePause命令, 但不是命令執行中"));
            }
            else
            {
                WriteLog(7, "", String.Concat("收到VehiclePause命令"));
                temp.AGVPause = EnumVehicleSafetyAction.SlowStop;
            }
        }

        public void VehicleContinue()
        {
            MoveCommandData temp = localData.MoveControlData.MoveCommand;

            if (temp == null)
            {
                WriteLog(7, "", String.Concat("收到VehicleConitnue命令, 但不是命令執行中"));
            }
            else
            {
                WriteLog(7, "", String.Concat("收到VehicleConitnue命令"));
                temp.AGVPause = EnumVehicleSafetyAction.Normal;
            }
        }

        public void VehicleCancel()
        {
            MoveCommandData temp = localData.MoveControlData.MoveCommand;

            if (temp == null)
            {
                WriteLog(7, "", String.Concat("收到VehicleCancel命令, 但不是命令執行中"));
            }
            else
            {
                WriteLog(7, "", String.Concat("收到VehicleCancel命令"));
                temp.VehicleStopFlag = true;
            }
        }

        public void VehicleStop()
        {
            MoveCommandData temp = localData.MoveControlData.MoveCommand;

            if (temp == null)
            {
                WriteLog(7, "", String.Concat("收到VehicleStop命令, 但不是命令執行中"));
            }
            else
            {
                WriteLog(7, "", String.Concat("收到VehicleStop命令"));
                temp.VehicleStopFlag = true;
            }
        }

        public void AddReserve(string sectionID)
        {
            MoveCommandData temp = localData.MoveControlData.MoveCommand;

            if (temp != null)
            {
                if (temp.IndexOfReserveList >= temp.ReserveList.Count)
                    WriteLog(5, "", String.Concat("Reserve已經全部取得,但收到Reserve, Section ID : ", sectionID));
                else if (temp.ReserveList[temp.IndexOfReserveList].SectionID != sectionID)
                    WriteLog(5, "", String.Concat("Reserve Section ID mismuch, Section ID : ", sectionID));
                else
                {
                    temp.ReserveList[temp.IndexOfReserveList].GetReserve = true;
                    temp.IndexOfReserveList++;
                    WriteLog(5, "", String.Concat("取得Reserve, Section ID : ", sectionID));
                }
            }
            else
                WriteLog(3, "", String.Concat("在沒有命令的情況收到Reserve, Section ID : ", sectionID));
        }

        public void AddReservedIndexForDebugModeTest(int index)
        {
            MoveCommandData temp = localData.MoveControlData.MoveCommand;

            if (temp != null && index < temp.ReserveList.Count && !temp.ReserveList[index].GetReserve)
                AddReserve(temp.ReserveList[index].SectionID);
        }
        #endregion

        private void SetMovingDirectionByEndAGVPosition(MapAGVPosition end)
        {
            VehicleLocation now = localData.Location;

            if (localData.TheMapInfo.AllSection.ContainsKey(now.NowSection))
            {
                MapAGVPosition nowPosition = computeFunction.GetAGVPositionByVehicleLocation(now);

                double nowToEndAngle = computeFunction.ComputeAngle(nowPosition, end);

                double movingAngle = computeFunction.GetCurrectAngle(nowToEndAngle - nowPosition.Angle);

                if (movingAngle == 0)
                    ChangeMovingDirection(EnumMovingDirection.Front);
                else if (movingAngle == 180)
                    ChangeMovingDirection(EnumMovingDirection.Back);
                else if (movingAngle == 90)
                    ChangeMovingDirection(EnumMovingDirection.Right);
                else if (movingAngle == -90)
                    ChangeMovingDirection(EnumMovingDirection.Left);
                else if (movingAngle > 0 && movingAngle < 90)
                    ChangeMovingDirection(EnumMovingDirection.FrontRight);
                else if (movingAngle < 0 && movingAngle > -90)
                    ChangeMovingDirection(EnumMovingDirection.FrontLeft);
                else if (movingAngle > 90 && movingAngle < 180)
                    ChangeMovingDirection(EnumMovingDirection.BackRight);
                else if (movingAngle < -90 && movingAngle > -180)
                    ChangeMovingDirection(EnumMovingDirection.BackLeft);
                else
                {
                    WriteLog(5, "", String.Concat("MovingAngle else.. change to none"));
                    ChangeMovingDirection(EnumMovingDirection.None);
                }
            }
            else
            {
                WriteLog(5, "", String.Concat("Now Section Not find, change to none"));
                ChangeMovingDirection(EnumMovingDirection.None);
            }
        }

        private void ChangeMovingDirection(EnumMovingDirection newDirection)
        {
            WriteLog(7, "", String.Concat("Change MovingDirection : ", newDirection.ToString()));
            localData.MIPCData.MoveControlDirection = newDirection;
        }

        private void SetBuzzerTypeByEndAGVPosition(MapAGVPosition end)
        {
            VehicleLocation now = localData.Location;

            if (localData.TheMapInfo.AllSection.ContainsKey(now.NowSection))
            {
                MapAGVPosition nowPosition = computeFunction.GetAGVPositionByVehicleLocation(now);

                double nowToEndAngle = computeFunction.ComputeAngle(nowPosition, end);

                double movingAngle = computeFunction.GetCurrectAngle(nowToEndAngle - nowPosition.Angle);

                if (movingAngle == 0 || movingAngle == 180)
                    ChangeBuzzerType(EnumBuzzerType.Moving);
                else
                    ChangeBuzzerType(EnumBuzzerType.MoveShift);
            }
            else
            {
                WriteLog(5, "", String.Concat("Now Section Not find, change to Moving"));
                ChangeBuzzerType(EnumBuzzerType.Moving);
            }
        }

        private void ChangeBuzzerType(EnumBuzzerType buzzerType)
        {
            WriteLog(7, "", String.Concat("Change BuzzerType : ", buzzerType.ToString()));
            localData.MIPCData.MoveControlBuzzerType = buzzerType;
        }

        private void SetDirectionLightByEndAGVPosition(MapAGVPosition end)
        {
            VehicleLocation now = localData.Location;

            if (localData.TheMapInfo.AllSection.ContainsKey(now.NowSection))
            {
                MapAGVPosition nowPosition = computeFunction.GetAGVPositionByVehicleLocation(now);

                double nowToEndAngle = computeFunction.ComputeAngle(nowPosition, end);

                double movingAngle = computeFunction.GetCurrectAngle(nowToEndAngle - nowPosition.Angle);

                if (movingAngle == 0)
                    ChangeDirectionLight(EnumDirectionLight.Front);
                else if (movingAngle == 180)
                    ChangeDirectionLight(EnumDirectionLight.Back);
                else if (movingAngle == 90)
                    ChangeDirectionLight(EnumDirectionLight.Right);
                else if (movingAngle == -90)
                    ChangeDirectionLight(EnumDirectionLight.Left);
                else if (movingAngle > 0 && movingAngle < 90)
                    ChangeDirectionLight(EnumDirectionLight.FrontRight);
                else if (movingAngle < 0 && movingAngle > -90)
                    ChangeDirectionLight(EnumDirectionLight.FrontLeft);
                else if (movingAngle > 90 && movingAngle < 180)
                    ChangeDirectionLight(EnumDirectionLight.BackRight);
                else if (movingAngle < -90 && movingAngle > -180)
                    ChangeDirectionLight(EnumDirectionLight.BackLeft);
                else
                {
                    WriteLog(5, "", String.Concat("MovingAngle else.. change to none"));
                    ChangeDirectionLight(EnumDirectionLight.None);
                }
            }
            else
            {
                WriteLog(5, "", String.Concat("Now Section Not find, change to none"));
                ChangeDirectionLight(EnumDirectionLight.None);
            }
        }

        private void ChangeDirectionLight(EnumDirectionLight newDirection)
        {
            WriteLog(7, "", String.Concat("Change DirectionLight : ", newDirection.ToString()));
            localData.MIPCData.MoveControlDirectionLight = newDirection;
        }

        #region Auto CycleRun.
        public bool StopAutoCycleRun { get; set; } = false;
        List<MapAddress> EndList = new List<MapAddress>();

        private MapAddress GetMapAddressByRandon()
        {
            if (localData.Real == null || EndList.Count == 0)
                return null;

            Random randon = new Random();
            int index;

            while (true)
            {
                index = randon.Next(0, EndList.Count);

                if (!computeFunction.IsSamePosition(localData.Real.Position, EndList[index].AGVPosition.Position))
                {
                    if (computeFunction.GetTwoPositionDistance(localData.Real, EndList[index].AGVPosition) > 30)
                        return EndList[index];
                }
            }
        }

        private void ProcessEndAddresList()
        {
            if (EndList.Count == 0)
            {
                foreach (MapAddress address in localData.TheMapInfo.AllAddress.Values)
                {
                    EndList.Add(address);
                }
            }
        }

        public void MoveControlLocalAutoCycleRun()
        {
            StopAutoCycleRun = false;
            Random randon = new Random();
            MapAddress nextEnd;
            int num;

            string errorMessage = "";
            MoveCmdInfo moveCmdInfo;

            try
            {
                ProcessEndAddresList();

                while (!StopAutoCycleRun)
                {
                    if (localData.MoveControlData.ErrorBit)
                    {
                        WriteLog(5, "", " 結束AutoCycleRun流程 : ErrorBit On");
                        break;
                    }
                    else if (localData.MoveControlData.Ready)
                    {
                        Thread.Sleep(1000);
                        num = randon.Next(0, 3);

                        nextEnd = GetMapAddressByRandon();

                        if (nextEnd == null)
                        {
                            WriteLog(3, "", "結束AutoCycleRun流程 : GetNextMove End Address return null");
                            break;
                        }

                        moveCmdInfo = new MoveCmdInfo();
                        moveCmdInfo.CommandID = String.Concat("LocalCycleRun ", DateTime.Now.ToString("HH:mm:ss"));
                        if (!CreateMoveCommandList.Step0_CheckMovingAddress(new List<string> { nextEnd.Id }, ref moveCmdInfo, ref errorMessage))
                        {
                            WriteLog(3, "", String.Concat("結束AutoCycleRun流程 : Step0_CheckMovingAddress return falst, EndAddress ID : ", nextEnd.Id));
                            break;
                        }

                        if (VehicleMove(moveCmdInfo, ref errorMessage))
                        {
                            //num = randon.Next(0, 3);

                            //if (num == 0)
                            //    AddReservedIndexForDebugModeTest(randon.Next(0, localData.MoveControlData.MoveCommand.ReserveList.Count));
                            //else // 正常命令.
                            for (int i = 0; i < localData.MoveControlData.MoveCommand.ReserveList.Count; i++)
                                AddReservedIndexForDebugModeTest(i);
                        }
                        else
                        {
                            WriteLog(3, "", "結束AutoCycleRun流程 : VehicleMove return false!");
                            break;
                        }
                    }
                }

                WriteLog(3, "", "結束AutoCycleRun流程");
            }
            catch (Exception ex)
            {
                WriteLog(5, "", String.Concat("Exceptrion : ", ex.StackTrace));
                WriteLog(5, "", String.Concat("Exceptrion : ", ex.ToString()));
            }

        }
        #endregion

        #region Write-CSV.
        private void WriteLogCSV()
        {
            string csvLog;
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch sleepTimer = new System.Diagnostics.Stopwatch();

            //AxisFeedbackData feedBackData;

            //List<string> order = MoveControlVehicleData.MotionData.AxisList;
            MapAGVPosition logAGVPosition;
            MapPosition logMapPosition;
            LocateAGVPosition locateAGVPosition;
            ThetaSectionDeviation logThetaDeviation;
            DateTime now;
            //EnumMoveState logState;
            double time;
            MoveCommandData logCommand;
            AxisFeedbackData tempFeedbackData = null;
            VehicleLocation tempVehicleLocation = null;

            bool isLine;

            try
            {
                while (Status != EnumControlStatus.WaitThreadStop)
                {
                    timer.Restart();
                    isLine = false;

                    now = DateTime.Now;
                    logCommand = localData.MoveControlData.MoveCommand;

                    if (logCommand != null && logCommand.MoveStatus == EnumMoveStatus.Moving)
                        isLine = true;

                    // Time.
                    csvLog = now.ToString("yyyy/MM/dd HH:mm:ss.fff");

                    // CommandStatus.
                    if (logCommand != null)
                        csvLog = String.Concat(csvLog, ",", logCommand.CommandStatus.ToString());
                    else
                        csvLog = String.Concat(csvLog, ",");

                    // MoveStatus.
                    if (logCommand != null)
                        csvLog = String.Concat(csvLog, ",", logCommand.MoveStatus.ToString());
                    else
                        csvLog = String.Concat(csvLog, ",");

                    //  RealEncoder
                    if (logCommand != null)
                        csvLog = String.Concat(csvLog, ",", logCommand.CommandEncoder.ToString("0.0"));
                    else
                        csvLog = String.Concat(csvLog, ",");

                    //  SimulateVelocity
                    csvLog = String.Concat(csvLog, ",", localData.MoveControlData.MotionControlData.SimulateLineVelocity.ToString("0.0"));

                    //  RealMovingVelocity
                    csvLog = String.Concat(csvLog, ",", localData.MoveControlData.MotionControlData.LineVelocity.ToString("0.0"));

                    //  RealMovingVelocityAngle
                    csvLog = String.Concat(csvLog, ",", localData.MoveControlData.MotionControlData.LineVelocityAngle.ToString("0.0"));

                    //  ThetaVelocity
                    csvLog = String.Concat(csvLog, ",", localData.MoveControlData.MotionControlData.ThetaVelocity.ToString("0.0"));

                    //  Move/Stop.
                    csvLog = String.Concat(csvLog, ",", (localData.MoveControlData.MotionControlData.MoveStatus != EnumAxisMoveStatus.Stop ? "Move" : "Stop"));

                    // location
                    tempVehicleLocation = localData.Location;

                    if (tempVehicleLocation != null)
                        csvLog = String.Concat(csvLog, ",", tempVehicleLocation.LastAddress, ",", tempVehicleLocation.InAddress.ToString(), ",", tempVehicleLocation.NowSection, ",", tempVehicleLocation.DistanceFormSectionHead.ToString("0"));
                    else
                        csvLog = String.Concat(csvLog, ",,,,");

                    // real x y theta.
                    logAGVPosition = localData.Real;

                    if (logAGVPosition != null)
                        csvLog = String.Concat(csvLog, ",", logAGVPosition.Position.X.ToString("0.0"),
                                                       ",", logAGVPosition.Position.Y.ToString("0.0"),
                                                       ",", logAGVPosition.Angle.ToString("0.0"));
                    else
                        csvLog = String.Concat(csvLog, ",,,");

                    // mipc x y theta.
                    locateAGVPosition = localData.MoveControlData.MotionControlData.EncoderAGVPosition;

                    if (locateAGVPosition != null && locateAGVPosition.AGVPosition != null)
                    {
                        csvLog = String.Concat(csvLog, ",", locateAGVPosition.AGVPosition.Position.X.ToString("0.0"),
                                                       ",", locateAGVPosition.AGVPosition.Position.Y.ToString("0.0"),
                                                       ",", locateAGVPosition.AGVPosition.Angle.ToString("0.0"));
                    }
                    else
                        csvLog = String.Concat(csvLog, ",,,");

                    //  LocateDriver
                    locateAGVPosition = localData.MoveControlData.LocateControlData.LocateAGVPosition;

                    if (locateAGVPosition != null && locateAGVPosition.AGVPosition != null)
                    {
                        csvLog = String.Concat(csvLog, ",", locateAGVPosition.AGVPosition.Position.X.ToString("0.0"),
                                                       ",", locateAGVPosition.AGVPosition.Position.Y.ToString("0.0"),
                                                       ",", locateAGVPosition.AGVPosition.Angle.ToString("0.0"));
                    }
                    else
                        csvLog = String.Concat(csvLog, ",,,");

                    // ThetaSectionDeviation
                    logThetaDeviation = localData.MoveControlData.ThetaSectionDeviation;

                    if (logThetaDeviation != null)
                    {
                        csvLog = String.Concat(csvLog, ",", logThetaDeviation.Theta.ToString("0.0"),
                                                       ",", logThetaDeviation.SectionDeviation.ToString("0.0"));
                    }
                    else
                        csvLog = String.Concat(csvLog, ",,");

                    #region MotionDriver.
                    for (int i = 0; i < 4; i++)
                    {
                        if (localData.MoveControlData.MotionControlData.AllAxisFeedbackData != null)
                        {
                            switch (i)
                            {
                                case 0:
                                    if (localData.MoveControlData.MotionControlData.AllAxisFeedbackData.ContainsKey(EnumDefaultAxisName.XFL))
                                        tempFeedbackData = localData.MoveControlData.MotionControlData.AllAxisFeedbackData[EnumDefaultAxisName.XFL];
                                    else
                                        tempFeedbackData = null;
                                    break;
                                case 1:
                                    if (localData.MoveControlData.MotionControlData.AllAxisFeedbackData.ContainsKey(EnumDefaultAxisName.XFR))
                                        tempFeedbackData = localData.MoveControlData.MotionControlData.AllAxisFeedbackData[EnumDefaultAxisName.XFR];
                                    else
                                        tempFeedbackData = null;
                                    break;
                                case 2:
                                    if (localData.MoveControlData.MotionControlData.AllAxisFeedbackData.ContainsKey(EnumDefaultAxisName.XRL))
                                        tempFeedbackData = localData.MoveControlData.MotionControlData.AllAxisFeedbackData[EnumDefaultAxisName.XRL];
                                    else
                                        tempFeedbackData = null;
                                    break;
                                case 3:
                                    if (localData.MoveControlData.MotionControlData.AllAxisFeedbackData.ContainsKey(EnumDefaultAxisName.XRR))
                                        tempFeedbackData = localData.MoveControlData.MotionControlData.AllAxisFeedbackData[EnumDefaultAxisName.XRR];
                                    else
                                        tempFeedbackData = null;
                                    break;
                            }
                        }
                        else
                            tempFeedbackData = null;

                        if (tempFeedbackData != null)
                        {
                            csvLog = String.Concat(csvLog, ",", tempFeedbackData.Position.ToString("0.0"));
                            csvLog = String.Concat(csvLog, ",", tempFeedbackData.Velocity.ToString("0.0"));
                            csvLog = String.Concat(csvLog, ",", tempFeedbackData.AxisServoOnOff.ToString());
                            csvLog = String.Concat(csvLog, ",", tempFeedbackData.AxisStatus.ToString());
                            csvLog = String.Concat(csvLog, ",", tempFeedbackData.V.ToString("0.0"));
                            csvLog = String.Concat(csvLog, ",", tempFeedbackData.DA.ToString("0.0"));
                            csvLog = String.Concat(csvLog, ",", tempFeedbackData.QA.ToString("0.0"));
                        }
                        else
                            csvLog = String.Concat(csvLog, ",,,,,,,");
                    }
                    #endregion

                    for (int i = 0; i < localData.MIPCData.MIPCTestArray.Length; i++)
                        csvLog = String.Concat(csvLog, ",", localData.MIPCData.MIPCTestArray[i].ToString("0.000"));

                    logger.LogString(csvLog);

                    if (isLine)
                        lineLogger.LogString(csvLog);

                    sleepTimer.Restart();

                    while (timer.ElapsedMilliseconds < config.TimeValueConfig.IntervalTimeList[EnumIntervalTimeType.CSVLogInterval])
                        Thread.Sleep(1);

                    sleepTimer.Stop();

                    if (localData.SimulateMode && localData.MoveControlData.SimulateBypassLog)
                    {
                        if (sleepTimer.ElapsedMilliseconds > config.TimeValueConfig.IntervalTimeList[EnumIntervalTimeType.CSVLogInterval] * 2)
                            WriteLog(3, "", String.Concat("CSVThreadSleep time : ", sleepTimer.ElapsedMilliseconds.ToString("0")));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(1, "", ex.ToString());
                WriteLog(1, "", "WriteLogCSV Excption!");
            }
        }
        #endregion
    }
}