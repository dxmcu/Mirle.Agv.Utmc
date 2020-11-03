using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Mirle.Agv.INX.Controller
{
    public class MotionControlHandler
    {
        private LoggerAgent loggerAgent = LoggerAgent.Instance;
        private ComputeFunction computeFunction = ComputeFunction.Instance;
        private LocalData localData = LocalData.Instance;

        private MIPCControlHandler mipcControl;
        private AlarmHandler alarmHandler;
        public SimulateControl SimulateControl { get; set; }

        private string device = MethodInfo.GetCurrentMethod().ReflectedType.Name;
        private string normalLogName;

        private EnumMIPCSetPostionStep SetPositionStep = EnumMIPCSetPostionStep.Step0_WaitLocateReady;
        
        public bool AllServoOn
        {
            get
            {
                if (localData.SimulateMode)
                    return true;
                else
                    return localData.MoveControlData.MotionControlData.AllServoStatus == localData.MoveControlData.MotionControlData.AllServoOn;
            }
        }

        public bool AllServoOff
        {
            get
            {
                if (localData.SimulateMode)
                    return true;
                else
                    return localData.MoveControlData.MotionControlData.AllServoStatus == localData.MoveControlData.MotionControlData.AllServoOff;
            }
        }

        private AxisData move = null;
        private AxisData turn = null;
        private AxisData ems = null;
        private AxisData eq = null;
        private AxisData turn_Moving = null;

        private bool resetAlarm = false;
        private EnumControlStatus status = EnumControlStatus.Ready;
        public EnumControlStatus Status
        {
            get
            {
                if (resetAlarm)
                    return EnumControlStatus.ResetAlarm;
                else
                    if (localData.SimulateMode)
                    return EnumControlStatus.Ready;
                else
                {
                    if (SetPositionStep == EnumMIPCSetPostionStep.Ready)
                        return status;
                    else
                        return EnumControlStatus.NotReady;
                }
            }
        }

        private double spinTurnStopTheta = 0;

        public MotionControlHandler(MIPCControlHandler mipcControl, AlarmHandler alarmHandler, string normalLogName)
        {
            this.mipcControl = mipcControl;
            this.alarmHandler = alarmHandler;
            move = localData.MoveControlData.CreateMoveCommandConfig.Move;
            turn_Moving = localData.MoveControlData.CreateMoveCommandConfig.Turn_Moving;
            ems = localData.MoveControlData.CreateMoveCommandConfig.EMS;
            turn = localData.MoveControlData.CreateMoveCommandConfig.Turn;
            eq = localData.MoveControlData.CreateMoveCommandConfig.EQ;
            SimulateControl = new SimulateControl("SimulateControl");

            this.normalLogName = normalLogName;
            setPositionTimer.Restart();
            ReadTurnParameterXML();

            spinTurnStopTheta = computeFunction.GetAccDecDistance(turn.Velocity, 0, turn.Deceleration, turn.Jerk);
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

        #region Read-XML.
        protected AGVTurnParameter ReadTurnXML(XmlElement element)
        {
            AGVTurnParameter temp = new AGVTurnParameter();

            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "R":
                        temp.R = double.Parse(item.InnerText);
                        break;
                    case "Velocity":
                        temp.Velocity = double.Parse(item.InnerText);
                        break;
                    case "VChangeSafetyDistance":
                        temp.VChangeSafetyDistance = double.Parse(item.InnerText);
                        break;
                    case "SectionAngleChangeMin":
                        temp.SectionAngleChangeMin = double.Parse(item.InnerText);
                        break;
                    case "SectionAngleChangeMax":
                        temp.SectionAngleChangeMax = double.Parse(item.InnerText);
                        break;
                    case "SafetyVelocityRange":
                        temp.SafetyVelocityRange = double.Parse(item.InnerText);
                        break;
                    case "RTurnDistanceRange":
                        temp.RTurnDistanceRange = double.Parse(item.InnerText);
                        break;
                    default:
                        break;
                }
            }

            return temp;
        }

        protected void ReadTurnParameterByType(XmlElement element, EnumTurnType type)
        {
            AGVTurnParameter temp;

            foreach (XmlNode item in element.ChildNodes)
            {
                temp = ReadTurnXML((XmlElement)item);
                temp.Type = type;
                temp.TurnName = item.Name;
                localData.MoveControlData.TurnParameter.Add(item.Name, temp);
            }
        }

        private void ReadTurnParameterXML()
        {
            string path = @"D:\MecanumConfigs\MoveControl\MotionControl\TurnParameter.xml";

            try
            {
                XmlDocument doc = new XmlDocument();

                if (!File.Exists(path))
                {
                    WriteLog(3, "", "找不到Config!");
                    return;
                }

                doc.Load(path);
                var rootNode = doc.DocumentElement;

                string locatePath = Path.Combine(new DirectoryInfo(path).Parent.FullName, "device");

                foreach (XmlNode item in rootNode.ChildNodes)
                {
                    switch (item.Name)
                    {
                        case "STurn":
                            ReadTurnParameterByType((XmlElement)item, EnumTurnType.STurn);
                            break;
                        case "RTurn":
                            ReadTurnParameterByType((XmlElement)item, EnumTurnType.RTurn);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Excption : ", ex.ToString()));
            }
        }
        #endregion

        public void ResetAlarm()
        {

        }

        private void UpdateMotionControlData_SimulateMode()
        {
            if (localData.MoveControlData.LocateControlData.LocateAGVPosition != null)
                localData.Real = localData.MoveControlData.LocateControlData.LocateAGVPosition.AGVPosition;

            // simulate realEnocder ++;
        }

        private Stopwatch setPositionTimer = new Stopwatch();

        private LocateAGVPosition stepSetPositionLocateAGVPosition = null;

        private void UpdateMotionControlData_NotSimulateMode()
        {
            switch (SetPositionStep)
            {
                case EnumMIPCSetPostionStep.Step0_WaitLocateReady:
                    stepSetPositionLocateAGVPosition = localData.MoveControlData.LocateControlData.LocateAGVPosition;
                    if (stepSetPositionLocateAGVPosition != null && stepSetPositionLocateAGVPosition.AGVPosition != null && stepSetPositionLocateAGVPosition.Type == EnumAGVPositionType.Normal)
                    {
                        SetPosition(stepSetPositionLocateAGVPosition);
                        SetPositionStep = EnumMIPCSetPostionStep.Step1_WaitMIPCDataOK;
                    }

                    break;
                case EnumMIPCSetPostionStep.Step1_WaitMIPCDataOK:
                    LocateAGVPosition encoder = localData.MoveControlData.MotionControlData.EncoderAGVPosition;

                    if (encoder != null && encoder.AGVPosition != null)
                    {
                        if (Math.Abs(encoder.AGVPosition.Position.X - stepSetPositionLocateAGVPosition.AGVPosition.Position.X) <= localData.MoveControlData.MoveControlConfig.InPositionRange &&
                            Math.Abs(encoder.AGVPosition.Position.Y - stepSetPositionLocateAGVPosition.AGVPosition.Position.Y) <= localData.MoveControlData.MoveControlConfig.InPositionRange &&
                            Math.Abs(computeFunction.GetCurrectAngle(encoder.AGVPosition.Angle - stepSetPositionLocateAGVPosition.AGVPosition.Angle)) <= localData.MoveControlData.MoveControlConfig.SectionAllowDeltaTheta)
                            SetPositionStep = EnumMIPCSetPostionStep.Ready;
                    }

                    LocateAGVPosition temp = localData.MoveControlData.LocateControlData.LocateAGVPosition;
                    if (temp != null && temp.AGVPosition != null && temp.Type == EnumAGVPositionType.Normal)
                    {
                        stepSetPositionLocateAGVPosition = temp;
                        SetPosition(temp);
                    }

                    break;
                case EnumMIPCSetPostionStep.Ready:
                    if (/*SetPositionFlag && */setPositionTimer.ElapsedMilliseconds > localData.MoveControlData.MoveControlConfig.TimeValueConfig.IntervalTimeList[EnumIntervalTimeType.SetPositionInterval])
                    {
                        if (localData.MoveControlData.LocateControlData.LocateAGVPosition != null)
                        {
                            setPositionTimer.Restart();
                            SetPosition(localData.MoveControlData.LocateControlData.LocateAGVPosition);
                        }
                    }

                    LocateAGVPosition motionLocateData = localData.MoveControlData.MotionControlData.EncoderAGVPosition;

                    if (motionLocateData != null)
                    {
                        double deltaTime = (DateTime.Now - motionLocateData.GetDataTime).TotalMilliseconds + motionLocateData.ScanTime +
                                           (localData.MoveControlData.MoveControlConfig.TimeValueConfig.IntervalTimeList[EnumIntervalTimeType.ThreadSleepTime]) / 2;

                        double deltaLength = deltaTime / 1000 * localData.MoveControlData.MotionControlData.LineVelocity;

                        double x = motionLocateData.AGVPosition.Position.X + Math.Cos(localData.MoveControlData.MotionControlData.LineVelocityAngle / 180 * Math.PI) * deltaLength;
                        double y = motionLocateData.AGVPosition.Position.Y + Math.Sin(localData.MoveControlData.MotionControlData.LineVelocityAngle / 180 * Math.PI) * deltaLength;
                        double angle = computeFunction.GetCurrectAngle(motionLocateData.AGVPosition.Angle + deltaTime / 1000 * localData.MoveControlData.MotionControlData.ThetaVelocity);

                        MapAGVPosition newAGVPosition = new MapAGVPosition(new MapPosition(x, y), angle);

                        localData.Real = newAGVPosition;
                    }
                    break;
                default:
                    break;
            }
        }

        public void UpdateMotionControlData()
        {
            if (localData.SimulateMode)
                UpdateMotionControlData_SimulateMode();
            else
                UpdateMotionControlData_NotSimulateMode();
        }

        #region 根源.
        virtual public bool Move(MapAGVPosition end, double lineVelocity, double lineAcc, double lineDec, double lineJerk, double thetaVelocity, double thetaAcc, double thetaDec, double thetaJerk)
        {
            if (mipcControl.AGV_Move(end, lineVelocity, lineAcc, lineDec, lineJerk, thetaVelocity, thetaAcc, thetaDec, thetaJerk))
            {
                MapAGVPosition now = localData.Real;

                double distance = 10000;

                if (now != null)
                    distance = computeFunction.GetDistanceFormTwoAGVPosition(now, end) * 2;

                if (lineVelocity != eq.Velocity)
                    SimulateControl.SetMoveCommandSimulateData(distance, lineVelocity, lineAcc, lineDec, lineJerk);
                else
                    SimulateControl.SetMoveCommandSimulateData(distance, lineVelocity, lineAcc, lineDec, lineJerk, false);

                localData.MoveControlData.MotionControlData.PreMoveStatus = EnumAxisMoveStatus.PreMove;
                return true;
            }
            else
                return false;
        }
        #endregion

        #region Auto.
        public bool ServoOff_All()
        {
            return mipcControl.AGV_ServoOff();
        }

        public bool ServoOn_All()
        {
            return mipcControl.AGV_ServoOn();
        }

        private MapAGVPosition GetMapAGVPositionWithThetaOffset(MapAGVPosition inputData)
        {
            MapAGVPosition outputData = new MapAGVPosition(inputData);
            outputData.Angle += cycleOffset * 360;

            if (!localData.SimulateMode)
            {
                while (Math.Abs(outputData.Angle - lastSetPositionValue_Offset.AGVPosition.Angle) > 180)
                {
                    if ((outputData.Angle - lastSetPositionValue_Offset.AGVPosition.Angle) > 180)
                        outputData.Angle -= 360;
                    else
                        outputData.Angle += 360;
                }
            }

            return outputData;
        }

        public bool Move_Line(MapAGVPosition end, double lineVelocity)
        {
            MapAGVPosition temp = GetMapAGVPositionWithThetaOffset(end);

            return Move(temp, lineVelocity, move.Acceleration, move.Deceleration, move.Jerk, turn_Moving.Velocity, turn_Moving.Acceleration, turn_Moving.Deceleration, turn_Moving.Jerk);
        }

        public bool Move_EQ(MapAGVPosition end)
        {
            MapAGVPosition temp = GetMapAGVPositionWithThetaOffset(end);

            return Move(temp, eq.Velocity, eq.Acceleration, eq.Deceleration, eq.Jerk, turn_Moving.Velocity, turn_Moving.Acceleration, turn_Moving.Deceleration, turn_Moving.Jerk);
        }

        public bool Move_ChangeEnd(MapAGVPosition end)
        {
            MapAGVPosition temp = GetMapAGVPositionWithThetaOffset(end);

            return mipcControl.AGV_ChangeEnd(temp);
        }

        public bool Move_VelocityChange(double newVelocity)
        {
            if (mipcControl.AGV_ChangeVelocity(newVelocity))
            {
                SimulateControl.SetVChangeSimulateData(newVelocity, move.Acceleration, move.Deceleration, move.Jerk);
                return true;
            }
            else
                return false;
        }

        public bool Turn_SpinTurn(MapAGVPosition end)
        {
            MapAGVPosition temp = GetMapAGVPositionWithThetaOffset(end);

            if (Move(temp, eq.Velocity, eq.Acceleration, eq.Deceleration, eq.Jerk, turn.Velocity, turn.Acceleration, turn.Deceleration, turn.Jerk))
            {
                localData.MoveControlData.MotionControlData.PreMoveStatus = EnumAxisMoveStatus.PreMove;
                return true;
            }
            else
                return false;
        }

        public bool Turn_SpinTurnNeedStop(MapAGVPosition end)
        {
            return Math.Abs(computeFunction.GetCurrectAngle(end.Angle - localData.Real.Angle)) <= spinTurnStopTheta;
        }

        public bool Turn_STurn(MapAGVPosition start, double moveVelocity, double moveAngle, double R, double RTheta)
        {
            //MapAGVPosition temp = new MapAGVPosition(end);

            //while (Math.Abs(temp.Angle + cycleOffset * 360) > 180)
            //{
            //    if ((temp.Angle + cycleOffset * 360) > 180)
            //        cycleOffset++;
            //    else
            //        cycleOffset--;
            //}

            return false;
        }

        public bool Turn_RTurn(MapAGVPosition start, double moveVelocity, double moveAngle, double R, double RTheta)
        {
            //MapAGVPosition temp = new MapAGVPosition(end);

            //while (Math.Abs(temp.Angle + cycleOffset * 360) > 180)
            //{
            //    if ((temp.Angle + cycleOffset * 360) > 180)
            //        cycleOffset++;
            //    else
            //        cycleOffset--;
            //}

            return false;
        }

        public bool Stop_Normal()
        {
            if (mipcControl.AGV_Stop(move.Deceleration, move.Jerk, turn_Moving.Deceleration, turn_Moving.Jerk))
            {
                SimulateControl.SetVChangeSimulateData(0, move.Acceleration, move.Deceleration, move.Jerk);
                return true;
            }
            else
                return false;
        }

        public bool Stop_SpinTurn()
        {
            if (mipcControl.AGV_Stop(move.Deceleration, move.Jerk, turn.Deceleration, turn.Jerk))
            {
                SimulateControl.SetVChangeSimulateData(0, move.Acceleration, move.Deceleration, move.Jerk);
                return true;
            }
            else
                return false;
        }

        public bool Stop_EMS()
        {
            if (mipcControl.AGV_Stop(ems.Deceleration, ems.Jerk, turn_Moving.Deceleration, turn_Moving.Jerk))
            {
                SimulateControl.SetVChangeSimulateData(0, move.Acceleration, ems.Deceleration, ems.Jerk);
                return true;
            }
            else
                return false;
        }

        public bool EMO()
        {
            return false;
        }

        private LocateAGVPosition lastSetPositionValue;
        private LocateAGVPosition lastSetPositionValue_Offset;

        private int cycleOffset = 0;

        private LocateAGVPosition CheckThetaOffset(LocateAGVPosition locateAGVPosition)
        {
            LocateAGVPosition temp = new LocateAGVPosition(locateAGVPosition);

            if (lastSetPositionValue != null)
            {
                while (Math.Abs((temp.AGVPosition.Angle + cycleOffset * 360) - lastSetPositionValue_Offset.AGVPosition.Angle) > 180)
                {
                    if (((temp.AGVPosition.Angle + cycleOffset * 360) - lastSetPositionValue_Offset.AGVPosition.Angle) > 180)
                        cycleOffset--;
                    else
                        cycleOffset++;
                }

                temp.AGVPosition.Angle += cycleOffset * 360;
            }

            return temp;
        }

        public bool SetPosition(LocateAGVPosition locateAGVPosition)
        {
            if (lastSetPositionValue == null || lastSetPositionValue != locateAGVPosition)
            {
                LocateAGVPosition setPosition = CheckThetaOffset(locateAGVPosition);
                mipcControl.SetPosition(setPosition, false);
                lastSetPositionValue = locateAGVPosition;
                lastSetPositionValue_Offset = setPosition;
            }

            return true;
        }

        public bool SetPositionFlag { get; set; } = true;

        public bool SetPosition_NoOffset(LocateAGVPosition locateAGVPosition)
        {
            if (!mipcControl.SetPosition(locateAGVPosition, true))
                return false;
            else
            {
                cycleOffset = 0;
                lastSetPositionValue = locateAGVPosition;
                return true;
            }
        }

        #endregion
        
    }
}
