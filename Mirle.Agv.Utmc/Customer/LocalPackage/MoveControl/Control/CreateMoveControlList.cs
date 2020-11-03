using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Mirle.Agv.INX.Controller
{
    public class CreateMoveControlList
    {
        private AlarmHandler alarmHandler;
        private LoggerAgent loggerAgent = LoggerAgent.Instance;
        private ComputeFunction computeFunction = ComputeFunction.Instance;
        private LocalData localData = LocalData.Instance;
        private string device = MethodInfo.GetCurrentMethod().ReflectedType.Name;
        private string normalLogName = "";

        private CreateMoveCommandListConfig config = new CreateMoveCommandListConfig();
        private string path = @"D:\MecanumConfigs\MoveControl\CreateMoveCommandListConfig.xml";

        public SimulateControl SimulateControl { private get; set; } = null;

        private double createCommandSearchVelcoityDelta = 1;

        public CreateMoveControlList(AlarmHandler alarmHandler, string normalLogName)
        {
            this.alarmHandler = alarmHandler;
            this.normalLogName = normalLogName;

            string errorMessage = "";
            if (!ReadCreateCommandListConfig(ref errorMessage))
                WriteLog(1, "", String.Concat(errorMessage));

            localData.MoveControlData.CreateMoveCommandConfig = config;
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

        #region Read XML.
        private AxisData ReadAxisData(XmlElement element)
        {
            AxisData returnAxisData = new AxisData();

            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "Acceleration":
                        returnAxisData.Acceleration = double.Parse(item.InnerText);
                        break;
                    case "Deceleration":
                        returnAxisData.Deceleration = double.Parse(item.InnerText);
                        break;
                    case "Jerk":
                        returnAxisData.Jerk = double.Parse(item.InnerText);
                        break;
                    case "Velocity":
                        returnAxisData.Velocity = double.Parse(item.InnerText);
                        break;
                    case "Position":
                        returnAxisData.Position = double.Parse(item.InnerText);
                        break;
                    default:
                        break;
                }
            }

            return returnAxisData;
        }

        private bool ReadCreateCommandListConfig(ref string errorMessage)
        {
            try
            {
                XmlDocument doc = new XmlDocument();

                if (!File.Exists(path))
                {
                    errorMessage = String.Concat("Path : ", path, " 找不到Config!");
                    return false;
                }

                doc.Load(path);
                XmlElement rootNode = doc.DocumentElement;

                foreach (XmlNode item in rootNode.ChildNodes)
                {
                    switch (item.Name)
                    {
                        case "Move":
                            config.Move = ReadAxisData((XmlElement)item);
                            break;
                        case "Turn":
                            config.Turn = ReadAxisData((XmlElement)item);
                            break;
                        case "Turn_Moving":
                            config.Turn_Moving = ReadAxisData((XmlElement)item);
                            break;
                        case "EQ":
                            config.EQ = ReadAxisData((XmlElement)item);

                            /// 強制Acc/Dec相同,以便模擬計算.
                            if (config.EQ.Acceleration < config.EQ.Deceleration)
                                config.EQ.Deceleration = config.EQ.Acceleration;
                            else
                                config.EQ.Acceleration = config.EQ.Deceleration;
                            break;
                        case "EMS":
                            config.EMS = ReadAxisData((XmlElement)item);
                            break;
                        case "LowVelocity_High":
                            config.LowVelocity_High = double.Parse(item.InnerText);
                            break;
                        case "LowVelocity_Low":
                            config.LowVelocity_Low = double.Parse(item.InnerText);
                            break;
                        case "SecondCorrectionX":
                            config.SecondCorrectionX = double.Parse(item.InnerText);
                            break;
                        case "NormalStopDistance":
                            config.NormalStopDistance = double.Parse(item.InnerText);
                            break;
                        case "ReserveSafetyDistance":
                            config.ReserveSafetyDistance = double.Parse(item.InnerText);
                            break;
                        case "RetryMoveDistance":
                            config.RetryMoveDistance = double.Parse(item.InnerText);
                            break;
                        case "AllowAGVAngleRange":
                            config.AllowAGVAngleRange = double.Parse(item.InnerText);
                            break;
                        case "VelocityRange":
                            config.VelocityRange = double.Parse(item.InnerText);
                            break;
                        case "VChangeBufferTime":
                            config.VChangeBufferTime = double.Parse(item.InnerText);
                            break;
                        case "SafteyDistance":
                            foreach (XmlNode item2 in ((XmlElement)item).ChildNodes)
                            {
                                config.SafteyDistance[(EnumCommandType)Enum.Parse(typeof(EnumCommandType), item2.Name)] = double.Parse(item2.InnerText);
                            }

                            break;
                        default:
                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = String.Concat("Exception : ", ex.ToString());
                return false;
            }
        }
        #endregion

        #region AccDecDistance.
        public double GetAccDecDistanceForMoveAxisData(double startVelocity, double endVelocity)
        {
            if (startVelocity == endVelocity)
                return 0;
            else if (startVelocity > endVelocity)
                return computeFunction.GetAccDecDistance(startVelocity, endVelocity, config.Move.Deceleration, config.Move.Jerk);
            else
                return computeFunction.GetAccDecDistance(startVelocity, endVelocity, config.Move.Acceleration, config.Move.Jerk);
        }
        #endregion

        #region WriteLog-Step.
        private string GetStep1String(MoveCmdInfo moveCmdInfo)
        {
            try
            {
                if (moveCmdInfo == null)
                    return "MoveCmdInfo == null";

                string logMessage = String.Concat("\r\nCommandID : ", moveCmdInfo.CommandID, "\r\nMovingAddress : ");

                if (moveCmdInfo.MovingAddress != null)
                {
                    for (int i = 0; i < moveCmdInfo.MovingAddress.Count; i++)
                    {
                        if (i == 0)
                            logMessage = String.Concat(logMessage, moveCmdInfo.MovingAddress[i].Id);
                        else
                            logMessage = String.Concat(logMessage, ", ", moveCmdInfo.MovingAddress[i].Id);
                    }
                }

                logMessage = String.Concat(logMessage, "\r\nMovingSections : ");

                if (moveCmdInfo.MovingSections != null)
                {
                    for (int i = 0; i < moveCmdInfo.MovingSections.Count; i++)
                    {
                        if (i == 0)
                            logMessage = String.Concat(logMessage, moveCmdInfo.MovingSections[i].Id);
                        else
                            logMessage = String.Concat(logMessage, ", ", moveCmdInfo.MovingSections[i].Id);
                    }
                }

                logMessage = String.Concat(logMessage, "\r\nEndAddress : ", (moveCmdInfo.EndAddress == null ? "" : moveCmdInfo.EndAddress.Id));

                logMessage = String.Concat(logMessage, "\r\nStageDirection : ", moveCmdInfo.StageDirection.ToString());
                logMessage = String.Concat(logMessage, "\r\nIsMoveEndDoLoadUnload : ", moveCmdInfo.IsMoveEndDoLoadUnload.ToString());

                return logMessage;
            }
            catch (Exception ex)
            {
                return String.Concat("Exception : ", ex.ToString());
            }
        }

        private void WriteLog_Step1(MoveCmdInfo moveCmdInfo)
        {
            WriteLog(7, "", GetStep1String(moveCmdInfo));
        }

        private void WriteLog_Step2(List<string> actionList)
        {
            try
            {
                string logMessage = String.Concat("AddressActionList : ", actionList[0]);

                for (int i = 1; i < actionList.Count; i++)
                    logMessage = String.Concat(logMessage, " -> ", actionList[i]);

                WriteLog(7, "", logMessage);
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        private void WriteLog_Step3(List<ReserveData> reseveList)
        {
            try
            {
                string logMessage = "ReserveList :";

                for (int i = 0; i < reseveList.Count; i++)
                    logMessage = String.Concat(logMessage, "\r\nreserve node ", i.ToString(), ", Section : ", reseveList[i].SectionID,
                                                           " ", computeFunction.GetMapAGVPositionStringWithAngle(reseveList[i].SectionEnd, "0"));

                WriteLog(7, "", logMessage);
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        public List<string> GetReserveListAndSectionID(MoveCommandData command)
        {
            List<string> returnList = new List<string>();

            for (int i = 0; i < command.ReserveList.Count; i++)
                returnList.Add(String.Concat(i.ToString(), ", Section : ", command.ReserveList[i].SectionID));

            return returnList;
        }

        private void WriteLog_Step4(List<OneceMoveCommand> oneceMoveCommandList, List<SectionLine> sectionLineList)
        {
            try
            {
                string logMessage = "OneceMoveCommandList :";

                for (int j = 0; j < oneceMoveCommandList.Count; j++)
                {
                    for (int i = 1; i < oneceMoveCommandList[j].PositionList.Count; i++)
                    {
                        logMessage = String.Concat(logMessage, "\r\n第 ", (j + 1).ToString(), " 次動令,第 " + i.ToString(),
                                         " 條路線 Action : ", oneceMoveCommandList[j].AddressActions[i - 1].ToString(), " -> ",
                                         oneceMoveCommandList[j].AddressActions[i].ToString(), ", from (",
                                         computeFunction.GetMapPositionString(oneceMoveCommandList[j].PositionList[i - 1], "0"), ") ",
                                         oneceMoveCommandList[j].AGVAngleList[i - 1].ToString("0"), " to (",
                                         computeFunction.GetMapPositionString(oneceMoveCommandList[j].PositionList[i], "0"), ") ",
                                         oneceMoveCommandList[j].AGVAngleList[i - 1].ToString("0"), ", velocity : ",
                                         oneceMoveCommandList[j].SpeedList[i - 1].ToString("0"));
                    }
                }

                WriteLog(7, "", logMessage);

                logMessage = "SectionLineList :";

                for (int i = 0; i < sectionLineList.Count; i++)
                {
                    logMessage = String.Concat(logMessage, "\r\nsectionLineList 第 ", (i + 1).ToString(), " 條, section ", sectionLineList[i].Section.Id,
                                                           " ,form ", sectionLineList[i].Start.Id, computeFunction.GetMapAGVPositionStringWithAngle(sectionLineList[i].Start.AGVPosition, "0"),
                                                           " to ", sectionLineList[i].End.Id, computeFunction.GetMapAGVPositionStringWithAngle(sectionLineList[i].End.AGVPosition, "0"),
                                                           ", Distance : ", sectionLineList[i].Distance.ToString("0"),
                                                           ", EncoderAddSectionDistanceStart : ", sectionLineList[i].EncoderAddSectionDistanceStart.ToString("0"),
                                                           ", SectionDirFlag : ", sectionLineList[i].SectionDirFlag.ToString(),
                                                           ", EncoderStart : ", sectionLineList[i].EncoderStart.ToString("0"),
                                                           ", EncoderEnd : ", sectionLineList[i].EncoderEnd.ToString("0"),
                                                           ", Angle : ", sectionLineList[i].Angle.ToString("0"));
                }

                WriteLog(7, "", logMessage);
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        private void TriggerLog(Command cmd, ref string logMessage)
        {
            logMessage = String.Concat(logMessage, "command type : ", cmd.CmdType.ToString());

            if (cmd.TriggerAGVPosition != null)
            {
                logMessage = String.Concat(logMessage, ", 觸發Encoder為 ", cmd.TriggerEncoder.ToString("0"), " ~ ",
                                          (cmd.TriggerEncoder + cmd.SafetyDistance).ToString("0"),
                                          ", ", computeFunction.GetMapAGVPositionStringWithAngle(cmd.TriggerAGVPosition, "0"));
            }
            else
                logMessage = String.Concat(logMessage, ", 為立即觸發");
        }

        private void WriteMoveCommandListLogTypeMove(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = String.Concat(logMessage, ", 終點位置 ", computeFunction.GetMapAGVPositionStringWithAngle(cmd.EndAGVPosition), ", 速度 ", cmd.Velocity.ToString("0"));

            if (cmd.ReserveNumber != -1)
                logMessage = String.Concat(logMessage, ", Reserve index : ", cmd.ReserveNumber.ToString());
        }

        private void WriteMoveCommandListLogTypeSTurn(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = String.Concat(logMessage, ", 為", cmd.TurnType, ", R = ", localData.MoveControlData.TurnParameter[cmd.TurnType].R.ToString("0"),
                                                   ", 速度 : ", localData.MoveControlData.TurnParameter[cmd.TurnType].Velocity.ToString("0"),
                                                   ", 終點改為 ", computeFunction.GetMapAGVPositionStringWithAngle(cmd.EndAGVPosition, "0"),
                                                   ", oldMovingAngle ", cmd.MovingAngle.ToString("0"),
                                                   ", newMovingAngle ", cmd.NewMovingAngle.ToString("0"));
        }

        private void WriteMoveCommandListLogTypeRTurn(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = String.Concat(logMessage, ", 為", cmd.TurnType, ", R = ", localData.MoveControlData.TurnParameter[cmd.TurnType].R.ToString("0"),
                                                   ", 速度 : ", localData.MoveControlData.TurnParameter[cmd.TurnType].Velocity.ToString("0"),
                                                   ", 終點改為 ", computeFunction.GetMapAGVPositionStringWithAngle(cmd.EndAGVPosition, "0"));
        }

        private void WriteMoveCommandListLogTypeSpinTurn(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = String.Concat(logMessage, ", 終點位置 ", computeFunction.GetMapAGVPositionStringWithAngle(cmd.EndAGVPosition));
        }

        private void WriteMoveCommandListLogTypeVchange(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = String.Concat(logMessage, ", 速度變更為 : ", cmd.Velocity.ToString("0"), ", vChangeType : ", ((EnumVChangeType)cmd.Type).ToString());
        }

        private void WriteMoveCommandListLogTypeChangeSectionLine(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = String.Concat(logMessage, ", 過站");
        }

        private void WriteMoveCommandListLogTypeSlowStop(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = String.Concat(logMessage, ", 方向 : ", ", endEncoder : ", cmd.EndEncoder.ToString("0"));
        }

        private void WriteMoveCommandListLogTypeStop(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = String.Concat(logMessage, ", 取得Reserve index = ", cmd.ReserveNumber.ToString(), "時取消此Command");
        }

        private void WriteMoveCommandListLogTypeEnd(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = String.Concat(logMessage, ", 終點Encoder : ", cmd.EndEncoder.ToString("0"), ", 座標 ", computeFunction.GetMapAGVPositionStringWithAngle(cmd.EndAGVPosition, "0"));
        }

        public List<string> GetCommandList(MoveCommandData command)
        {
            return GetMoveCommandListInfo(command.CommandList);
        }

        private List<string> GetMoveCommandListInfo(List<Command> moveCmdList)
        {
            List<string> logMessage = new List<string>();

            string tempLogMessage;

            for (int i = 0; i < moveCmdList.Count; i++)
            {
                tempLogMessage = "";
                switch (moveCmdList[i].CmdType)
                {
                    case EnumCommandType.Move:
                        WriteMoveCommandListLogTypeMove(moveCmdList[i], ref tempLogMessage);
                        break;
                    case EnumCommandType.STurn:
                        WriteMoveCommandListLogTypeSTurn(moveCmdList[i], ref tempLogMessage);
                        break;
                    case EnumCommandType.RTurn:
                        WriteMoveCommandListLogTypeRTurn(moveCmdList[i], ref tempLogMessage);
                        break;
                    case EnumCommandType.SpinTurn:
                        WriteMoveCommandListLogTypeSpinTurn(moveCmdList[i], ref tempLogMessage);
                        break;
                    case EnumCommandType.ChangeSection:
                        WriteMoveCommandListLogTypeChangeSectionLine(moveCmdList[i], ref tempLogMessage);
                        break;
                    case EnumCommandType.Vchange:
                        WriteMoveCommandListLogTypeVchange(moveCmdList[i], ref tempLogMessage);
                        break;
                    case EnumCommandType.SlowStop:
                        WriteMoveCommandListLogTypeSlowStop(moveCmdList[i], ref tempLogMessage);
                        break;
                    case EnumCommandType.Stop:
                        WriteMoveCommandListLogTypeStop(moveCmdList[i], ref tempLogMessage);
                        break;
                    case EnumCommandType.End:
                        WriteMoveCommandListLogTypeEnd(moveCmdList[i], ref tempLogMessage);
                        break;
                    default:
                        tempLogMessage = "command type : default ??";
                        break;
                }

                logMessage.Add(tempLogMessage);
            }

            return logMessage;
        }

        private void WriteLog_Step5(MoveCommandData moveCommand)
        {
            try
            {
                string totalLogMessage = "MoveCommandList :";
                List<string> logMessage = new List<string>();
                logMessage = GetMoveCommandListInfo(moveCommand.CommandList);

                for (int i = 0; i < logMessage.Count; i++)
                    totalLogMessage = String.Concat(totalLogMessage, "\r\n", logMessage[i]);

                WriteLog(7, "", totalLogMessage);
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }
        #endregion

        #region Step0-產生移動Address(from MainForm).
        public bool Step0_CheckMovingAddress(List<string> mainFormAddressList, ref MoveCmdInfo moveCmdInfo, ref string errorMessage)
        {
            try
            {
                VehicleLocation nowLocation = localData.Location;

                MapAGVPosition now = localData.Real;

                if (!localData.TheMapInfo.AllAddress.ContainsKey(nowLocation.LastAddress))
                {
                    errorMessage = "迷航中";
                    return false;
                }
                else if (mainFormAddressList.Count == 0)
                {
                    errorMessage = "mainFormAddressList.Count == 0";
                    return false;
                }

                MapAddress start;

                if (nowLocation.InAddress)
                    start = localData.TheMapInfo.AllAddress[nowLocation.LastAddress];
                else
                    start = computeFunction.FindMapAddressByMapAGVPosition(now, nowLocation.NowSection);

                if (start == null)
                {
                    errorMessage = "搜尋不到相近的Address or Section";
                    return false;
                }

                if (!computeFunction.FindFromToAddressSectionList(start, localData.TheMapInfo.AllAddress[mainFormAddressList[0]], ref moveCmdInfo))
                {
                    errorMessage = "FindFromToAddressSectionList return false";
                    return false;
                }

                for (int i = 1; i < mainFormAddressList.Count; i++)
                {
                    if (!computeFunction.FindFromToAddressSectionList(localData.TheMapInfo.AllAddress[mainFormAddressList[i - 1]],
                                                                      localData.TheMapInfo.AllAddress[mainFormAddressList[i]], ref moveCmdInfo))
                    {
                        errorMessage = "FindFromToAddressSectionList return false";
                        return false;
                    }
                }

                moveCmdInfo.EndAddress = localData.TheMapInfo.AllAddress[mainFormAddressList[mainFormAddressList.Count - 1]];
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = String.Concat("Exception : ", ex.ToString());
                return false;
            }
        }
        #endregion

        #region Step1-檢查Middler資料.
        public bool Step1_CheckMoveCommandData(MoveCmdInfo moveCmdInfo, ref string errorMessage)
        {
            try
            {
                WriteLog_Step1(moveCmdInfo);

                if (moveCmdInfo == null)
                {
                    errorMessage = "moveCmdInfo == null";
                    return false;
                }
                else if (moveCmdInfo.EndAddress == null)
                {
                    errorMessage = "EndAddress == null";
                    return false;
                }
                else if (moveCmdInfo.MovingSections == null)
                {
                    errorMessage = "MovingSections == null";
                    return false;
                }
                else if (moveCmdInfo.MovingAddress == null)
                {
                    errorMessage = "MovingAddress == null";
                    return false;
                }
                else if (moveCmdInfo.MovingSections.Count + 1 != moveCmdInfo.MovingAddress.Count)
                {
                    errorMessage = "MovingSections.Count + 1 != MovingAddress.Count";
                    return false;
                }
                else
                {
                    for (int i = 0; i < moveCmdInfo.MovingSections.Count; i++)
                    {
                        if (moveCmdInfo.MovingAddress[i] == moveCmdInfo.MovingAddress[i + 1])
                        {
                            errorMessage = String.Concat("MovingAddress(index) : ", i.ToString(), " 和 ", (i + 1).ToString(), "相同");
                            return false;
                        }
                        else if (moveCmdInfo.MovingSections[i].FromAddress != moveCmdInfo.MovingAddress[i] &&
                                 moveCmdInfo.MovingSections[i].FromAddress != moveCmdInfo.MovingAddress[i + 1])
                        {
                            errorMessage = String.Concat("MovingSections(index) : ", i.ToString(), " 的 FromAddress 和 MovingAddress 對不上");
                            return false;
                        }
                        else if (moveCmdInfo.MovingSections[i].ToAddress != moveCmdInfo.MovingAddress[i] &&
                                 moveCmdInfo.MovingSections[i].ToAddress != moveCmdInfo.MovingAddress[i + 1])
                        {
                            errorMessage = String.Concat("MovingSections(index) : ", i.ToString(), " 的 ToAddress 和 MovingAddress 對不上");
                            return false;
                        }
                    }

                    MapAddress newFirstAddress = computeFunction.CheckNowAndFirstSectionAndAddress((moveCmdInfo.MovingSections.Count == 0 ? null : moveCmdInfo.MovingSections[0]), moveCmdInfo.MovingAddress[0], ref errorMessage);

                    if (newFirstAddress == null)
                        return false;

                    moveCmdInfo.StartAddress = newFirstAddress;

                    if (!moveCmdInfo.StartAddress.CanSpin && computeFunction.GetCurrectAngle(localData.Real.Angle - moveCmdInfo.StartAddress.AGVPosition.Angle) > localData.MoveControlData.MoveControlConfig.SectionAllowDeltaTheta)
                    {
                        errorMessage = "起始角度差異過大且起點無法SpinTurn";
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = String.Concat("Exception : ", ex.ToString());
                return false;
            }
        }
        #endregion

        #region Step2-產生AddressActionList.
        private bool Step2_CheckTurnInOutDistanceChangeAction(ref MoveCmdInfo moveCmdInfo)
        {
            int startIndex = 0;

            int lastSTurnIndex = -1;

            double needDistance;
            double distance;
            bool result = false;
            bool rTurning = false;

            for (int i = 1; i < moveCmdInfo.AddressActions.Count; i++)
            {
                if (moveCmdInfo.AddressActions[i] == EnumDefaultAction.ST.ToString())
                {
                    if (rTurning)
                    {
                        startIndex = i;
                        rTurning = false;
                    }
                }
                else if (moveCmdInfo.AddressActions[i] == EnumDefaultAction.BST.ToString() ||
                         moveCmdInfo.AddressActions[i] == EnumDefaultAction.SpinTurn.ToString() ||
                         moveCmdInfo.AddressActions[i] == EnumDefaultAction.StopTurn.ToString())
                {
                    if (rTurning)
                        rTurning = false;

                    if (lastSTurnIndex != -1)
                    {
                        needDistance = localData.MoveControlData.TurnParameter[moveCmdInfo.AddressActions[lastSTurnIndex]].R +
                                       GetAccDecDistanceForMoveAxisData(localData.MoveControlData.TurnParameter[moveCmdInfo.AddressActions[lastSTurnIndex]].Velocity, 0) +
                                       config.NormalStopDistance;

                        distance = computeFunction.GetTwoPositionDistance((startIndex == 0 ? moveCmdInfo.StartAddress : moveCmdInfo.MovingAddress[startIndex]).AGVPosition, moveCmdInfo.MovingAddress[i].AGVPosition);

                        if (needDistance > distance)
                            moveCmdInfo.AddressActions[lastSTurnIndex] = EnumDefaultAction.StopTurn.ToString();

                        lastSTurnIndex = -1;
                    }


                    startIndex = i;
                }
                else if (computeFunction.TurnAndBTurnTypeCheck(moveCmdInfo.AddressActions[i], EnumTurnType.RTurn))
                {
                    if (lastSTurnIndex != -1)
                    {
                        needDistance = localData.MoveControlData.TurnParameter[moveCmdInfo.AddressActions[lastSTurnIndex]].R +
                                       GetAccDecDistanceForMoveAxisData(localData.MoveControlData.TurnParameter[moveCmdInfo.AddressActions[lastSTurnIndex]].Velocity, 0) +
                                       config.NormalStopDistance;

                        distance = computeFunction.GetTwoPositionDistance((startIndex == 0 ? moveCmdInfo.StartAddress : moveCmdInfo.MovingAddress[startIndex]).AGVPosition, moveCmdInfo.MovingAddress[i].AGVPosition);

                        if (needDistance > distance)
                            moveCmdInfo.AddressActions[lastSTurnIndex] = EnumDefaultAction.StopTurn.ToString();

                        lastSTurnIndex = -1;
                    }

                    startIndex = i;
                    rTurning = true;
                }
                else if (computeFunction.TurnTypeCheck(moveCmdInfo.AddressActions[i], EnumTurnType.STurn))
                {
                    distance = computeFunction.GetTwoPositionDistance((startIndex == 0 ? moveCmdInfo.StartAddress : moveCmdInfo.MovingAddress[startIndex]).AGVPosition, moveCmdInfo.MovingAddress[i].AGVPosition);

                    if (lastSTurnIndex != -1)
                    {
                        needDistance = localData.MoveControlData.TurnParameter[moveCmdInfo.AddressActions[lastSTurnIndex]].R +
                                       GetAccDecDistanceForMoveAxisData(localData.MoveControlData.TurnParameter[moveCmdInfo.AddressActions[lastSTurnIndex]].Velocity, 0) +
                                       config.NormalStopDistance +
                                       localData.MoveControlData.TurnParameter[moveCmdInfo.AddressActions[i]].R +
                                       localData.MoveControlData.TurnParameter[moveCmdInfo.AddressActions[i]].VChangeSafetyDistance +
                                       GetAccDecDistanceForMoveAxisData(0, localData.MoveControlData.TurnParameter[moveCmdInfo.AddressActions[i]].Velocity);

                        if (needDistance > distance)
                            moveCmdInfo.AddressActions[lastSTurnIndex] = EnumDefaultAction.StopTurn.ToString();

                        lastSTurnIndex = -1;
                    }

                    needDistance = localData.MoveControlData.TurnParameter[moveCmdInfo.AddressActions[i]].R +
                                   localData.MoveControlData.TurnParameter[moveCmdInfo.AddressActions[i]].VChangeSafetyDistance +
                                   GetAccDecDistanceForMoveAxisData(0, localData.MoveControlData.TurnParameter[moveCmdInfo.AddressActions[i]].Velocity);

                    if (needDistance > distance)
                        moveCmdInfo.AddressActions[i] = EnumDefaultAction.StopTurn.ToString();
                    else
                        lastSTurnIndex = i;

                    startIndex = i;
                }
                else if (moveCmdInfo.AddressActions[i] == EnumDefaultAction.End.ToString())
                    ;
                else
                    WriteLog(3, "", String.Concat("出現奇怪Action : ", moveCmdInfo.AddressActions[i]));
            }

            return result;
        }

        private bool GetRTurnStringByTwoMapAddress(MapAddress start, MapAddress end, ref string turnName, ref string errorMessage)
        {
            double agvAngleDelta = Math.Abs(computeFunction.GetCurrectAngle(start.AGVPosition.Angle - end.AGVPosition.Angle));

            foreach (AGVTurnParameter turn in localData.MoveControlData.TurnParameter.Values)
            {
                if (turn.Type == EnumTurnType.RTurn)
                {
                    if (Math.Abs(agvAngleDelta - turn.SectionAngleChangeMin) <= turn.SectionAngleChangeMax &&
                        Math.Abs(computeFunction.GetTwoPositionDistance(start.AGVPosition, end.AGVPosition) - turn.R * Math.Pow(2, 0.5)) <= turn.RTurnDistanceRange)
                    {
                        turnName = turn.TurnName;
                        return true;
                    }
                }
            }

            errorMessage = String.Concat("找不到車頭角度變化為 : ", agvAngleDelta.ToString(), "的RTurn參數!");
            return false;
        }

        private bool GetSTurnStringBySectionAngleChange(double sectionAngleChange, string specialTurnTag, ref string sTurnName, ref string errorMessage)
        {
            if (specialTurnTag == EnumDefaultAction.StopTurn.ToString())
            {
                sTurnName = specialTurnTag;
                return true;
            }

            if (localData.MoveControlData.TurnParameter.ContainsKey(specialTurnTag))
            {
                if (localData.MoveControlData.TurnParameter[specialTurnTag].SectionAngleChangeMax <= sectionAngleChange &&
                    sectionAngleChange <= localData.MoveControlData.TurnParameter[specialTurnTag].SectionAngleChangeMax)
                {
                    sTurnName = specialTurnTag;
                    return true;
                }
            }

            foreach (AGVTurnParameter turn in localData.MoveControlData.TurnParameter.Values)
            {
                if (turn.Type == EnumTurnType.STurn)
                {
                    if (turn.SectionAngleChangeMin <= sectionAngleChange && sectionAngleChange <= turn.SectionAngleChangeMax)
                    {
                        sTurnName = turn.TurnName;
                        return true;
                    }
                }
            }

            WriteLog(5, "", String.Concat("沒有找到Section變化為 : ", sectionAngleChange.ToString("0"), "的轉彎方式, 因此轉為", EnumDefaultAction.StopTurn.ToString()));

            sTurnName = specialTurnTag;
            return true;
        }

        public bool Step2_CreateAddresActionList(ref MoveCmdInfo moveCmdInfo, ref string errorMessage)
        {
            try
            {
                double lastSectionMovingAngle = 0;
                double thisSectionMovingAngle = 0;
                double newSectionMovingAngle = 0;

                double sectionAngleDelta = 0;
                string turnName = "";

                moveCmdInfo.AddressActions = new List<string>();

                DecompositionCommandData data = new DecompositionCommandData();

                lastSectionMovingAngle = computeFunction.ComputeAngle(moveCmdInfo.MovingAddress[0].AGVPosition, moveCmdInfo.MovingAddress[1].AGVPosition);

                if (moveCmdInfo.MovingSections[0].FromVehicleAngle == moveCmdInfo.MovingSections[0].ToVehicleAngle)
                {
                    if ((moveCmdInfo.MovingSections[0].FromAddress == moveCmdInfo.StartAddress && Math.Abs(computeFunction.GetCurrectAngle(localData.Real.Angle - moveCmdInfo.MovingSections[0].FromVehicleAngle)) <= localData.MoveControlData.MoveControlConfig.SectionAllowDeltaTheta) ||
                        (moveCmdInfo.MovingSections[0].ToAddress == moveCmdInfo.StartAddress && Math.Abs(computeFunction.GetCurrectAngle(localData.Real.Angle - moveCmdInfo.MovingSections[0].ToVehicleAngle)) <= localData.MoveControlData.MoveControlConfig.SectionAllowDeltaTheta) ||
                        (moveCmdInfo.MovingSections[0].FromAddress != moveCmdInfo.StartAddress && moveCmdInfo.MovingSections[0].ToAddress != moveCmdInfo.StartAddress &&
                         Math.Abs(computeFunction.GetCurrectAngle(localData.Real.Angle - moveCmdInfo.StartAddress.AGVPosition.Angle)) <= localData.MoveControlData.MoveControlConfig.SectionAllowDeltaTheta))
                        moveCmdInfo.AddressActions.Add(EnumDefaultAction.ST.ToString());
                    else if (moveCmdInfo.StartAddress.CanSpin)
                        moveCmdInfo.AddressActions.Add(EnumDefaultAction.SpinTurn.ToString());
                    else
                    {
                        errorMessage = String.Concat("AGV角度 : ", localData.Real.Angle.ToString("0.0"), ", 命令起始角度 : ", moveCmdInfo.StartAddress.AGVPosition.Angle.ToString("0.0"), " 差異過大且起點不能SpinTurn");
                        return false;
                    }
                }
                else
                {   //RTurn.
                    errorMessage = "by pass RTurn";
                    return false;

                    if (!GetRTurnStringByTwoMapAddress(moveCmdInfo.MovingAddress[0], moveCmdInfo.MovingAddress[1], ref turnName, ref errorMessage))
                        return false;

                    moveCmdInfo.AddressActions.Add(turnName);
                }

                if (computeFunction.TurnTypeCheck(moveCmdInfo.AddressActions[0], EnumTurnType.RTurn))
                {
                    if (!computeFunction.GetMovingAngleAfterRTurn(localData.MoveControlData.TurnParameter[moveCmdInfo.AddressActions[0]],
                                                                  moveCmdInfo.MovingAddress[0], moveCmdInfo.MovingAddress[1], ref newSectionMovingAngle, ref errorMessage))
                        return false;

                    lastSectionMovingAngle = newSectionMovingAngle;
                }

                if (moveCmdInfo.MovingSections[0].FromAddress == moveCmdInfo.MovingAddress[1])
                    data.AGVAngleInMap = moveCmdInfo.MovingSections[0].FromVehicleAngle;
                else
                    data.AGVAngleInMap = moveCmdInfo.MovingSections[0].ToVehicleAngle;

                for (int i = 1; i < moveCmdInfo.MovingSections.Count; i++)
                {
                    thisSectionMovingAngle = computeFunction.ComputeAngle(moveCmdInfo.MovingAddress[i].AGVPosition, moveCmdInfo.MovingAddress[i + 1].AGVPosition);

                    if (moveCmdInfo.MovingSections[i].FromVehicleAngle != moveCmdInfo.MovingSections[i].ToVehicleAngle)
                    {   //RTurn.
                        errorMessage = "by pass RTurn";
                        return false;

                        if (!GetRTurnStringByTwoMapAddress(moveCmdInfo.MovingAddress[i], moveCmdInfo.MovingAddress[i + 1], ref turnName, ref errorMessage))
                            return false;

                        if (Math.Abs(Math.Abs(computeFunction.GetCurrectAngle(thisSectionMovingAngle - lastSectionMovingAngle)) -
                                    localData.MoveControlData.TurnParameter[turnName].SectionAngleChangeMin / 2) < localData.MoveControlData.TurnParameter[turnName].SectionAngleChangeMax)
                        {
                            moveCmdInfo.AddressActions.Add(turnName);

                            if (!computeFunction.GetMovingAngleAfterRTurn(localData.MoveControlData.TurnParameter[turnName],
                                                                       moveCmdInfo.MovingAddress[i], moveCmdInfo.MovingAddress[i + 1], ref newSectionMovingAngle, ref errorMessage))
                                return false;



                            thisSectionMovingAngle = newSectionMovingAngle;
                        }
                        else if (Math.Abs(Math.Abs(computeFunction.GetCurrectAngle(thisSectionMovingAngle - (lastSectionMovingAngle - 180))) -
                                          localData.MoveControlData.TurnParameter[turnName].SectionAngleChangeMin / 2) < localData.MoveControlData.TurnParameter[turnName].SectionAngleChangeMax)
                        {
                            moveCmdInfo.AddressActions.Add(String.Concat("B", turnName));

                            if (!computeFunction.GetMovingAngleAfterRTurn(localData.MoveControlData.TurnParameter[turnName],
                                                                       moveCmdInfo.MovingAddress[i], moveCmdInfo.MovingAddress[i + 1], ref newSectionMovingAngle, ref errorMessage))
                                return false;

                            thisSectionMovingAngle = newSectionMovingAngle;
                        }
                        else
                        {
                            errorMessage = String.Concat("找不到角度相差為 ", Math.Abs(computeFunction.GetCurrectAngle(thisSectionMovingAngle - lastSectionMovingAngle)).ToString(),
                                                          " 的 RTurn 過彎參數!");
                            return false;
                        }

                        if (moveCmdInfo.MovingSections[i].FromAddress == moveCmdInfo.MovingAddress[i + 1])
                            data.AGVAngleInMap = moveCmdInfo.MovingSections[i].FromVehicleAngle;
                        else
                            data.AGVAngleInMap = moveCmdInfo.MovingSections[i].ToVehicleAngle;
                    }
                    else
                    {
                        if (data.AGVAngleInMap != moveCmdInfo.MovingSections[i].FromVehicleAngle)
                        {
                            moveCmdInfo.AddressActions.Add(EnumDefaultAction.SpinTurn.ToString());
                            data.AGVAngleInMap = moveCmdInfo.MovingSections[i].FromVehicleAngle;
                        }
                        else
                        {
                            sectionAngleDelta = computeFunction.GetCurrectAngle(thisSectionMovingAngle - lastSectionMovingAngle);

                            if (Math.Abs(computeFunction.GetCurrectAngle(sectionAngleDelta - 0)) < localData.MoveControlData.MoveControlConfig.SectionAllowDeltaTheta)
                                moveCmdInfo.AddressActions.Add(EnumDefaultAction.ST.ToString());
                            else if (Math.Abs(computeFunction.GetCurrectAngle(sectionAngleDelta - 180)) < localData.MoveControlData.MoveControlConfig.SectionAllowDeltaTheta)
                                moveCmdInfo.AddressActions.Add(EnumDefaultAction.BST.ToString());
                            else
                            {
                                if (GetSTurnStringBySectionAngleChange(Math.Abs(sectionAngleDelta), moveCmdInfo.MovingAddress[i].SpecialTurn, ref turnName, ref errorMessage))
                                    moveCmdInfo.AddressActions.Add(turnName);
                                else
                                    return false;
                            }
                        }
                    }

                    lastSectionMovingAngle = thisSectionMovingAngle;
                }

                moveCmdInfo.AddressActions.Add(EnumDefaultAction.End.ToString());

                WriteLog_Step2(moveCmdInfo.AddressActions);

                if (Step2_CheckTurnInOutDistanceChangeAction(ref moveCmdInfo))
                {
                    WriteLog(7, "", String.Concat("因為距離問題, Action進行修改"));
                    WriteLog_Step2(moveCmdInfo.AddressActions);
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = String.Concat("Exception : ", ex.ToString());
                return false;
            }
        }
        #endregion

        #region Step3-設定Reserve停點.
        public bool Step3_SetReserveStopAddress(MoveCmdInfo moveCmdInfo, ref MoveCommandData commandData, ref string errorMessage)
        {
            try
            {
                commandData.ReserveList = new List<ReserveData>();

                for (int i = 0; i < moveCmdInfo.MovingSections.Count; i++)
                    commandData.ReserveList.Add(new ReserveData(moveCmdInfo.MovingSections[i].Id, moveCmdInfo.MovingAddress[i + 1].AGVPosition));

                WriteLog_Step3(commandData.ReserveList);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = String.Concat("Exception : ", ex.ToString());
                return false;
            }
        }
        #endregion

        #region Step4-分解成多次移動命令.
        private EnumActionType GetActionType(string action)
        {
            if (action == EnumDefaultAction.ST.ToString() || action == EnumDefaultAction.SpinTurn.ToString() ||
                action == EnumDefaultAction.StopTurn.ToString() || localData.MoveControlData.TurnParameter.ContainsKey(action))
                return EnumActionType.FrontOrTurn;
            else if (action == EnumDefaultAction.End.ToString())
                return EnumActionType.End;
            else
            {
                if (action[0] != 'B')
                    return EnumActionType.None;
                else
                {
                    action = action.Substring(1, action.Length - 1);

                    if (action == EnumDefaultAction.ST.ToString() || action == EnumDefaultAction.SpinTurn.ToString() || localData.MoveControlData.TurnParameter.ContainsKey(action))
                        return EnumActionType.BackOrBackTurn;
                    else
                        return EnumActionType.None;
                }
            }
        }

        private void AddOneNodeCommandToOneceMoveCommand(MoveCmdInfo moveCmd, ref OneceMoveCommand tempOnceMoveCmd, int index, double agvAngleInMap, bool isEnd = false)
        {
            if (index == 0)
                tempOnceMoveCmd.PositionList.Add(moveCmd.StartAddress.AGVPosition.Position);
            else
                tempOnceMoveCmd.PositionList.Add(moveCmd.MovingAddress[index].AGVPosition.Position);

            tempOnceMoveCmd.AddressActions.Add(moveCmd.AddressActions[index]);
            tempOnceMoveCmd.AGVAngleList.Add(agvAngleInMap);

            if (isEnd)
                tempOnceMoveCmd.SpeedList.Add(0);
            else
            {
                if (moveCmd.SpecifySpeed.Count != 0 && moveCmd.SpecifySpeed[index] < moveCmd.MovingSections[index].Speed)
                    tempOnceMoveCmd.SpeedList.Add(moveCmd.SpecifySpeed[index]);
                else
                    tempOnceMoveCmd.SpeedList.Add(moveCmd.MovingSections[index].Speed);

                if (tempOnceMoveCmd.SpeedList[tempOnceMoveCmd.SpeedList.Count - 1] > config.Move.Velocity)
                    tempOnceMoveCmd.SpeedList[tempOnceMoveCmd.SpeedList.Count - 1] = config.Move.Velocity;
            }
        }

        private void AddOneNodeCommandToOneceMoveCommandWithAction(MoveCmdInfo moveCmd, ref OneceMoveCommand tempOnceMoveCmd, int index, double agvAngleInMap, string action)
        {
            tempOnceMoveCmd.PositionList.Add(moveCmd.MovingAddress[index].AGVPosition.Position);
            tempOnceMoveCmd.AddressActions.Add(action);
            tempOnceMoveCmd.AGVAngleList.Add(agvAngleInMap);

            if (moveCmd.SpecifySpeed.Count != 0 && moveCmd.SpecifySpeed[index] < moveCmd.MovingSections[index].Speed)
                tempOnceMoveCmd.SpeedList.Add(moveCmd.SpecifySpeed[index]);
            else
                tempOnceMoveCmd.SpeedList.Add(moveCmd.MovingSections[index].Speed);

            if (tempOnceMoveCmd.SpeedList[tempOnceMoveCmd.SpeedList.Count - 1] > config.Move.Velocity)
                tempOnceMoveCmd.SpeedList[tempOnceMoveCmd.SpeedList.Count - 1] = config.Move.Velocity;
        }

        private void AddOneNodeDataToOnceMoveCommand(ref OneceMoveCommand tempOnceMoveCmd, MapPosition position, string action, double speed, double agvAngleInMap)
        {
            tempOnceMoveCmd.PositionList.Add(position);
            tempOnceMoveCmd.AddressActions.Add(action);
            tempOnceMoveCmd.AGVAngleList.Add(agvAngleInMap);

            tempOnceMoveCmd.SpeedList.Add(speed);
        }

        #region 轉彎類型處理.
        private void AddSectionLine(ref MoveCommandData commandData, ref DecompositionCommandData data,
                                           MapAddress start, MapAddress end, MapSection section)
        {
            double startEndAngle = computeFunction.ComputeAngle(start.AGVPosition, end.AGVPosition);

            bool dirFlag = true;

            if (computeFunction.GetCurrectAngle(startEndAngle - section.SectionAngle) == 0)
                dirFlag = true;
            else if (computeFunction.GetCurrectAngle(startEndAngle - section.SectionAngle) == 180)
                dirFlag = false;
            else
                throw new Exception("startEndAngle - section.SectionAngle != 0 && != 180");

            double startEndDistance = computeFunction.GetDistanceFormTwoAGVPosition(start.AGVPosition, end.AGVPosition);

            double startToSectionStartDistance;

            if (dirFlag)
                startToSectionStartDistance = computeFunction.GetDistanceFormTwoAGVPosition(start.AGVPosition, section.FromAddress.AGVPosition);
            else
                startToSectionStartDistance = computeFunction.GetDistanceFormTwoAGVPosition(start.AGVPosition, section.ToAddress.AGVPosition);

            double encoderAddSectionDistanceStart;

            if (dirFlag)
                encoderAddSectionDistanceStart = data.StartEncoder - startToSectionStartDistance;
            else
                encoderAddSectionDistanceStart = data.StartEncoder - (startToSectionStartDistance - section.Distance);
            //encoderAddSectionDistanceStart = data.StartEncoder - (startToSectionStartDistance - startEndDistance);

            SectionLine tempSectionLine = new SectionLine(section, start, end, startEndAngle, startEndDistance, encoderAddSectionDistanceStart, dirFlag, data.StartEncoder);

            data.StartEncoder = data.StartEncoder + startEndDistance;
            data.LastAddress = end;
            commandData.SectionLineList.Add(tempSectionLine);
        }

        private bool Step4_STurn(MoveCmdInfo moveCmd, ref MoveCommandData commandData, ref DecompositionCommandData data,
               ref List<OneceMoveCommand> oneceMoveCommandList, ref OneceMoveCommand tempOnceMoveCmd, ref string errorMessage)
        {
            AddSectionLine(ref commandData, ref data, data.LastAddress, data.NowAddress, data.NowSection);
            AddOneNodeCommandToOneceMoveCommand(moveCmd, ref tempOnceMoveCmd, data.Index, data.AGVAngleInMap);
            data.LineDistance = 0;
            return true;
        }

        private bool Step4_RTurn(MoveCmdInfo moveCmd, ref MoveCommandData commandData, ref DecompositionCommandData data,
               ref List<OneceMoveCommand> oneceMoveCommandList, ref OneceMoveCommand tempOnceMoveCmd, ref string errorMessage)
        {
            return true;
        }

        private bool Step4_SpinTurn(MoveCmdInfo moveCmd, ref MoveCommandData commandData, ref DecompositionCommandData data,
               ref List<OneceMoveCommand> oneceMoveCommandList, ref OneceMoveCommand tempOnceMoveCmd, ref string errorMessage)
        {
            if (data.NowSection != null)
            {
                AddSectionLine(ref commandData, ref data, data.LastAddress, data.NowAddress, data.NowSection);

                AddOneNodeDataToOnceMoveCommand(ref tempOnceMoveCmd, moveCmd.MovingAddress[data.Index].AGVPosition.Position, EnumDefaultAction.SlowStop.ToString(), 0, data.AGVAngleInMap);
                oneceMoveCommandList.Add(tempOnceMoveCmd);

                if (moveCmd.MovingSections[data.Index].FromAddress == moveCmd.MovingAddress[data.Index])
                    data.AGVAngleInMap = moveCmd.MovingSections[data.Index].FromVehicleAngle;
                else
                    data.AGVAngleInMap = moveCmd.MovingSections[data.Index].ToVehicleAngle;

                tempOnceMoveCmd = new OneceMoveCommand();
                AddOneNodeCommandToOneceMoveCommandWithAction(moveCmd, ref tempOnceMoveCmd, data.Index, data.AGVAngleInMap, EnumDefaultAction.SpinTurn.ToString());
                oneceMoveCommandList.Add(tempOnceMoveCmd);
                tempOnceMoveCmd = new OneceMoveCommand();
                AddOneNodeCommandToOneceMoveCommandWithAction(moveCmd, ref tempOnceMoveCmd, data.Index, data.AGVAngleInMap, EnumDefaultAction.ST.ToString());

            }
            else
            {
                AddOneNodeCommandToOneceMoveCommandWithAction(moveCmd, ref tempOnceMoveCmd, data.Index, data.AGVAngleInMap, EnumDefaultAction.SpinTurn.ToString());
                oneceMoveCommandList.Add(tempOnceMoveCmd);
                tempOnceMoveCmd = new OneceMoveCommand();
                AddOneNodeCommandToOneceMoveCommandWithAction(moveCmd, ref tempOnceMoveCmd, data.Index, data.AGVAngleInMap, EnumDefaultAction.ST.ToString());
            }

            return true;
        }

        private bool Step4_StopTurn(MoveCmdInfo moveCmd, ref MoveCommandData commandData, ref DecompositionCommandData data,
               ref List<OneceMoveCommand> oneceMoveCommandList, ref OneceMoveCommand tempOnceMoveCmd, ref string errorMessage)
        {
            AddSectionLine(ref commandData, ref data, data.LastAddress, data.NowAddress, data.NowSection);

            AddOneNodeDataToOnceMoveCommand(ref tempOnceMoveCmd, moveCmd.MovingAddress[data.Index].AGVPosition.Position, EnumDefaultAction.SlowStop.ToString(), 0, data.AGVAngleInMap);
            oneceMoveCommandList.Add(tempOnceMoveCmd);

            tempOnceMoveCmd = new OneceMoveCommand();
            AddOneNodeCommandToOneceMoveCommandWithAction(moveCmd, ref tempOnceMoveCmd, data.Index, data.AGVAngleInMap, EnumDefaultAction.ST.ToString());

            return true;
        }
        #endregion

        #region Back action.
        private bool Step4_Back_ST(MoveCmdInfo moveCmd, ref MoveCommandData commandData, ref DecompositionCommandData data,
               ref List<OneceMoveCommand> oneceMoveCommandList, ref OneceMoveCommand tempOnceMoveCmd, ref string errorMessage)
        {
            double needDistance = 0;

            if (data.TurnType != "")
                needDistance = GetAccDecDistanceFormMove(localData.MoveControlData.TurnParameter[data.TurnType].Velocity, 0) + config.NormalStopDistance;

            if (data.LineDistance < needDistance)
            {
                errorMessage = "R轉後返折, 未寫";
                return false;
            }
            else
            {
                AddSectionLine(ref commandData, ref data, data.LastAddress, data.NowAddress, data.NowSection);

                AddOneNodeCommandToOneceMoveCommandWithAction(moveCmd, ref tempOnceMoveCmd, data.Index, data.AGVAngleInMap, EnumDefaultAction.SlowStop.ToString());
                oneceMoveCommandList.Add(tempOnceMoveCmd);

                tempOnceMoveCmd = new OneceMoveCommand();
                AddOneNodeCommandToOneceMoveCommandWithAction(moveCmd, ref tempOnceMoveCmd, data.Index, data.AGVAngleInMap, EnumDefaultAction.ST.ToString());
            }

            return true;
        }
        #endregion

        private bool Step4_End(MoveCmdInfo moveCmd, ref MoveCommandData commandData, ref DecompositionCommandData data,
               ref List<OneceMoveCommand> oneceMoveCommandList, ref OneceMoveCommand tempOnceMoveCmd, ref string errorMessage)
        {
            AddSectionLine(ref commandData, ref data, data.LastAddress, data.NowAddress, data.NowSection);
            AddOneNodeCommandToOneceMoveCommand(moveCmd, ref tempOnceMoveCmd, data.Index, data.AGVAngleInMap, true);
            oneceMoveCommandList.Add(tempOnceMoveCmd);
            return true;
        }

        public bool Step4_ChangeToOneceMoveCommandList(MoveCmdInfo moveCmd, ref MoveCommandData commandData, ref List<OneceMoveCommand> oneceMoveCommandList, ref string errorMessage)
        {
            try
            {
                commandData.SectionLineList = new List<SectionLine>();
                DecompositionCommandData data = new DecompositionCommandData();
                data.StartAddress = moveCmd.StartAddress;
                data.LastAddress = moveCmd.StartAddress;

                if (moveCmd.MovingSections[0].FromVehicleAngle == moveCmd.MovingSections[0].ToVehicleAngle)
                    data.AGVAngleInMap = moveCmd.MovingSections[0].FromVehicleAngle;
                else
                {
                    errorMessage = "先不處理RTurn";
                    return false;
                }

                OneceMoveCommand tempOnceMoveCmd = new OneceMoveCommand();

                while (data.Index < moveCmd.AddressActions.Count)
                {
                    if (data.Index == 0)
                        data.NowAddress = moveCmd.StartAddress;
                    else
                        data.NowAddress = moveCmd.MovingAddress[data.Index];

                    if (data.Index > 0)
                        data.NowSection = moveCmd.MovingSections[data.Index - 1];

                    switch (GetActionType(moveCmd.AddressActions[data.Index]))
                    {
                        case EnumActionType.FrontOrTurn:
                            if (moveCmd.AddressActions[data.Index] == EnumDefaultAction.ST.ToString())
                            {
                                if (data.Index != 0)
                                    AddSectionLine(ref commandData, ref data, data.LastAddress, data.NowAddress, data.NowSection);

                                AddOneNodeCommandToOneceMoveCommand(moveCmd, ref tempOnceMoveCmd, data.Index, data.AGVAngleInMap);
                            }
                            else if (computeFunction.TurnTypeCheck(moveCmd.AddressActions[data.Index], EnumTurnType.STurn))
                            {
                                data.NextAddress = moveCmd.MovingAddress[data.Index + 1];

                                if (!Step4_STurn(moveCmd, ref commandData, ref data, ref oneceMoveCommandList, ref tempOnceMoveCmd, ref errorMessage))
                                    return false;
                            }
                            //else if (computeFunction.TurnTypeCheck(moveCmd.AddressActions[data.Index], EnumTurnType.RTurn))
                            //{
                            //    if (moveCmd.MovingSections[data.Index].Speed != localData.MoveData.TurnParameter[moveCmd.AddressActions[data.Index]].Velocity)
                            //    {
                            //        errorMessage = String.Concat("Turn : ", moveCmd.AddressActions[data.Index], " 速度必須要是 ", localData.MoveData.TurnParameter[moveCmd.AddressActions[data.Index]].Velocity.ToString(), " !(movingSection)");
                            //        return false;
                            //    }
                            //    else if (moveCmd.SpecifySpeed.Count != 0 && moveCmd.SpecifySpeed[data.Index] != localData.MoveData.TurnParameter[moveCmd.AddressActions[data.Index]].Velocity)
                            //    {
                            //        errorMessage = String.Concat("Turn : ", moveCmd.AddressActions[data.Index], " 速度必須要是 ", localData.MoveData.TurnParameter[moveCmd.AddressActions[data.Index]].Velocity.ToString(), " !(SpecifySpeed)");
                            //        return false;
                            //    }

                            //    data.NextAddress = moveCmd.MovingAddress[data.Index + 1];

                            //    if (!Step4_RTurn(moveCmd, ref commandData, ref data, ref oneceMoveCommandList, ref tempOnceMoveCmd, ref errorMessage))
                            //        return false;
                            //}
                            else if (computeFunction.TurnTypeCheck(moveCmd.AddressActions[data.Index], EnumTurnType.StopTurn))
                            {
                                if (!Step4_StopTurn(moveCmd, ref commandData, ref data, ref oneceMoveCommandList, ref tempOnceMoveCmd, ref errorMessage))
                                    return false;
                            }
                            else if (computeFunction.TurnTypeCheck(moveCmd.AddressActions[data.Index], EnumTurnType.SpinTurn))
                            {
                                if (!Step4_SpinTurn(moveCmd, ref commandData, ref data, ref oneceMoveCommandList, ref tempOnceMoveCmd, ref errorMessage))
                                    return false;
                            }
                            else
                            {
                                errorMessage = "In switch case EnumActionType == FrontOrTurn, Action != ST & Action != STurn、RTurn、SpinTurn、StopTurn.";
                                return false;
                            }

                            break;
                        case EnumActionType.BackOrBackTurn:
                            moveCmd.AddressActions[data.Index] = moveCmd.AddressActions[data.Index].Substring(1, moveCmd.AddressActions[data.Index].Length - 1);

                            if (moveCmd.AddressActions[data.Index] == EnumDefaultAction.ST.ToString())
                            {
                                if (!Step4_Back_ST(moveCmd, ref commandData, ref data, ref oneceMoveCommandList, ref tempOnceMoveCmd, ref errorMessage))
                                    return false;
                            }
                            else if (computeFunction.TurnTypeCheck(moveCmd.AddressActions[data.Index], EnumTurnType.RTurn))
                            {
                                /*if (!Step4_Back_ST(moveCmd, ref commandData, ref data, ref oneceMoveCommandList, ref tempOnceMoveCmd))*/
                                return false;
                            }
                            else
                            {
                                errorMessage = "麥克阿姆輪的反折種類只會有B-ST、B-Rturn";
                                return false;
                            }
                            break;

                        case EnumActionType.End:
                            if (!Step4_End(moveCmd, ref commandData, ref data, ref oneceMoveCommandList, ref tempOnceMoveCmd, ref errorMessage))
                                return false;
                            break;

                        default:
                        case EnumActionType.None:
                            errorMessage = String.Concat("action : ", moveCmd.AddressActions[data.Index], " is EnumActionType default or None!");
                            return false;
                    }

                    data.Index++;
                }

                WriteLog_Step4(oneceMoveCommandList, commandData.SectionLineList);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = String.Concat("Exception : ", ex.ToString());
                return false;
            }
        }
        #endregion

        #region Step5-分解成移動命令.

        #region NewCommand function
        public Command NewMoveCommand(MapAGVPosition triggetAGVPosition, MapAGVPosition endAGVPosition, double realEncoder, double commandVelocity, EnumMoveStartType moveType, int reserveNumber = -1)
        {
            Command returnCommand = new Command();

            returnCommand.TriggerAGVPosition = triggetAGVPosition;
            returnCommand.EndAGVPosition = endAGVPosition;
            returnCommand.SafetyDistance = config.SafteyDistance[EnumCommandType.Move];
            returnCommand.TriggerEncoder = realEncoder - returnCommand.SafetyDistance / 2;
            returnCommand.CmdType = EnumCommandType.Move;
            returnCommand.Velocity = commandVelocity;
            returnCommand.ReserveNumber = reserveNumber;
            returnCommand.Type = moveType;
            return returnCommand;
        }

        public Command NewSpinTurnCommand(MapAGVPosition endAGVPosition, double endEncoder)
        {
            Command returnCommand = new Command();
            returnCommand.TriggerAGVPosition = null;
            returnCommand.EndEncoder = endEncoder;
            returnCommand.EndAGVPosition = endAGVPosition;
            returnCommand.SafetyDistance = config.SafteyDistance[EnumCommandType.SpinTurn];
            returnCommand.CmdType = EnumCommandType.SpinTurn;
            return returnCommand;
        }

        public Command NewVChangeCommand(MapAGVPosition triggetAGVPosition, double realEncoder, double commandVelocity, EnumVChangeType vChangeType = EnumVChangeType.Normal)
        {
            Command returnCommand = new Command();
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.TriggerAGVPosition = triggetAGVPosition;
            returnCommand.SafetyDistance = config.SafteyDistance[EnumCommandType.Vchange];
            returnCommand.CmdType = EnumCommandType.Vchange;
            returnCommand.Velocity = commandVelocity;
            returnCommand.Type = vChangeType;
            return returnCommand;
        }

        public Command NewStopCommand(MapAGVPosition triggetAGVPosition, double realEncoder, int nextReserveNumber)
        {
            Command returnCommand = new Command();
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.TriggerAGVPosition = triggetAGVPosition;
            returnCommand.SafetyDistance = config.SafteyDistance[EnumCommandType.Stop];
            returnCommand.CmdType = EnumCommandType.Stop;
            returnCommand.ReserveNumber = nextReserveNumber;
            return returnCommand;
        }

        public Command NewSlowStopCommand(MapAGVPosition triggerAGVPosition, double realEncoder, double endEncoder, EnumSlowStopType type)
        {
            Command returnCommand = new Command();
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.TriggerAGVPosition = triggerAGVPosition;

            returnCommand.SafetyDistance = config.SafteyDistance[EnumCommandType.SlowStop];
            returnCommand.CmdType = EnumCommandType.SlowStop;
            returnCommand.EndEncoder = endEncoder;
            returnCommand.Type = type;

            return returnCommand;
        }

        public Command NewChangeSectionLineCommand(MapAGVPosition triggerAGVPosition, double realEncoder)
        {
            Command returnCommand = new Command();
            returnCommand.CmdType = EnumCommandType.ChangeSection;
            returnCommand.SafetyDistance = config.SafteyDistance[EnumCommandType.ChangeSection];
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.TriggerAGVPosition = triggerAGVPosition;
            return returnCommand;
        }

        private Command NewEndCommand(double endEncoder, MapAGVPosition endAGVPosition)
        {
            Command returnCommand = new Command();
            returnCommand.TriggerAGVPosition = null;
            returnCommand.SafetyDistance = config.SafteyDistance[EnumCommandType.End];
            returnCommand.CmdType = EnumCommandType.End;
            returnCommand.EndEncoder = endEncoder;
            returnCommand.EndAGVPosition = endAGVPosition;

            return returnCommand;
        }


        private Command NewRTurnCommand(MapPosition position, double realEncoder, double wheelAngle, string type)
        {
            return null;
        }

        private Command NewSTurnCommand(MapAGVPosition triggerAGVPosition, double realEncoder, string type, double oldMovingAngle, double newMovingAngle)
        {
            Command returnCommand = new Command();
            returnCommand.TriggerAGVPosition = triggerAGVPosition;
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.SafetyDistance = config.SafteyDistance[EnumCommandType.STurn];
            returnCommand.CmdType = EnumCommandType.STurn;
            returnCommand.TurnType = type;
            returnCommand.MovingAngle = oldMovingAngle;
            returnCommand.NewMovingAngle = newMovingAngle;

            return returnCommand;
        }

        #endregion

        private void AddCommandToCommandList(ref MoveCommandData commandData, Command command, DecompositionCommandData data, ref List<VChangeData> vChangeList)
        {
            if (command.CmdType == EnumCommandType.Vchange || command.CmdType == EnumCommandType.Move)
                AddVChangeCommandToVChangeList(ref commandData, command, data, ref vChangeList);

            if (command.TriggerAGVPosition == null)
                commandData.CommandList.Add(command);
            else
            {
                int inserIndex = data.StartMoveIndex;

                for (; inserIndex < commandData.CommandList.Count; inserIndex++)
                {
                    if (commandData.CommandList[inserIndex].TriggerAGVPosition != null)
                    {
                        if (command.TriggerEncoder < commandData.CommandList[inserIndex].TriggerEncoder)
                            break;
                    }
                }

                commandData.CommandList.Insert(inserIndex, command);
            }
        }

        #region 確認ReserveIndex & 不再轉彎內.
        private int GetReserveIndex(List<ReserveData> reserveDataList, MapPosition position)
        {
            for (int i = 0; i < reserveDataList.Count; i++)
            {
                if (computeFunction.IsSamePosition(position, reserveDataList[i].SectionEnd.Position))
                {
                    if (reserveDataList[i].GetReserve)
                        return -1;
                    else
                    {
                        reserveDataList[i].GetReserve = true;

                        //if (i > 0 && computeFunction.TurnAndBTurnTypeCheck(reserveDataList[i - 1].Action, EnumTurnType.RTurn))
                        //    return -1;
                        //else
                        return i;
                    }
                }
            }

            return -1;
        }

        private bool CheckNotInTurn(OneceMoveCommand oneceMoveCommand, List<ReserveData> reserveDataList, int index, int reserveIndex, double safetyDistnace, double lineDistance, double nextVelocity)
        {
            if (reserveIndex == -1)
                return false;

            if (index > 0 && computeFunction.TurnTypeCheck(oneceMoveCommand.AddressActions[index - 1], EnumTurnType.RTurn))
                return false;

            double totalDistance;
            double accDistance;

            if (index == 0)
                return false;

            for (int i = index - 1; i >= 0; i--)
            {
                if (computeFunction.TurnTypeCheck(oneceMoveCommand.AddressActions[i], EnumTurnType.STurn))
                {
                    double distance = computeFunction.GetTwoPositionDistance(oneceMoveCommand.PositionList[i], reserveDataList[reserveIndex].SectionEnd.Position);

                    accDistance = GetAccDecDistanceFormMove(localData.MoveControlData.TurnParameter[oneceMoveCommand.AddressActions[i]].Velocity, 0) + GetAccDecDistanceFormMove(0, nextVelocity);

                    totalDistance = localData.MoveControlData.TurnParameter[oneceMoveCommand.AddressActions[i]].R + accDistance + safetyDistnace;

                    if (distance < totalDistance)
                        return false;

                    break;
                }
            }

            for (int i = index; i < oneceMoveCommand.PositionList.Count; i++)
            {
                if (computeFunction.TurnTypeCheck(oneceMoveCommand.AddressActions[i], EnumTurnType.STurn))
                {
                    double distance = lineDistance + computeFunction.GetTwoPositionDistance(oneceMoveCommand.PositionList[i], reserveDataList[reserveIndex].SectionEnd.Position);

                    if (distance < safetyDistnace)
                        return false;

                    break;
                }
            }

            return true;
        }
        #endregion

        #region 速度相關.
        public double GetAccDecDistanceFormMove(double oldVelociy, double newVelocity)
        {
            if (oldVelociy == newVelocity)
                return 0;
            else if (oldVelociy < newVelocity)
                return computeFunction.GetAccDecDistance(oldVelociy, newVelocity, config.Move.Acceleration, config.Move.Jerk);
            else
                return computeFunction.GetAccDecDistance(oldVelociy, newVelocity, config.Move.Deceleration, config.Move.Jerk);
        }

        public double GetSLowStopDistance()
        {
            return GetAccDecDistanceFormMove(config.EQ.Velocity, 0);
        }

        private int FindLastVChangeCommand(MoveCommandData commandData)
        {
            for (int i = commandData.CommandList.Count - 1; i >= 0; i--)
            {
                if (commandData.CommandList[i].CmdType == EnumCommandType.Vchange ||
                    commandData.CommandList[i].CmdType == EnumCommandType.Move)
                    return i;
            }

            return -1;
        }

        private void RemoveLastVChangeCommand(ref MoveCommandData commandData)
        {
            int index = FindLastVChangeCommand(commandData);

            if (index != -1)
                commandData.CommandList.RemoveAt(index);
        }

        private void OverRrideLastVChangeCommand(ref MoveCommandData commandData, DecompositionCommandData data, ref List<VChangeData> vChangeList, double velocity, EnumVChangeType type = EnumVChangeType.Normal)
        {
            int index = FindLastVChangeCommand(commandData);

            if (index != -1)
            {
                commandData.CommandList[index].Velocity = velocity;

                if (commandData.CommandList[index].CmdType != EnumCommandType.Move)
                    commandData.CommandList[index].Type = type;

                vChangeList[vChangeList.Count - 1].VelocityCommand = velocity;
                double distance = GetAccDecDistanceFormMove(vChangeList[vChangeList.Count - 1].StartVelocity, vChangeList[vChangeList.Count - 1].VelocityCommand);
                vChangeList[vChangeList.Count - 1].EndEncoder = vChangeList[vChangeList.Count - 1].StartEncoder + distance;
                data.NowVelocityCommand = velocity;
            }
        }

        private double GetLastVChangeVelocityCommandByDistance(VChangeData lastData, double distance)
        {
            double delta = (lastData.VelocityCommand - lastData.StartVelocity) > 0 ? 5 : -5;

            double newVelocity = lastData.StartVelocity;

            double vChangeDistance = GetAccDecDistanceFormMove(lastData.StartVelocity, newVelocity) + newVelocity * lastData.BufferTime / 1000;

            while (vChangeDistance <= distance)
            {
                newVelocity += delta;
                vChangeDistance = GetAccDecDistanceFormMove(lastData.StartVelocity, newVelocity) + newVelocity * lastData.BufferTime / 1000;
            }

            newVelocity -= delta;

            return newVelocity;
        }

        private bool GetLastVChangeVelocityCommandByTwoVChangeData(VChangeData lastData, VChangeData nowData, ref double newVelocity)
        {
            newVelocity = GetLastVChangeVelocityCommandByDistance(lastData, Math.Abs(lastData.StartEncoder - nowData.StartEncoder));

            if (lastData.StartVelocity == 0 && newVelocity == 0)
            {
                newVelocity = nowData.VelocityCommand;
                return false;
            }
            else if (lastData.StartVelocity <= newVelocity && newVelocity <= nowData.VelocityCommand)
            {
                newVelocity = nowData.VelocityCommand;
                return false;
            }

            return true;
        }

        private void AddVChangeCommandToVChangeList(ref MoveCommandData commandData, Command command, DecompositionCommandData data, ref List<VChangeData> vChangeList)
        {
            VChangeData vChangeData = new VChangeData();
            vChangeData.VelocityCommand = command.Velocity;
            vChangeData.Type = (EnumVChangeType)command.Type;

            switch (vChangeData.Type)
            {
                case EnumVChangeType.Normal:
                case EnumVChangeType.SensorSlow:
                case EnumVChangeType.TurnOut:
                case EnumVChangeType.MoveStart:
                    vChangeData.BufferTime = config.VChangeBufferTime;
                    break;
                case EnumVChangeType.STurn:
                case EnumVChangeType.RTurn:
                case EnumVChangeType.EQ:
                case EnumVChangeType.SlowStop:
                    vChangeData.BufferTime = 0;
                    break;

                default:
                    vChangeData.BufferTime = config.VChangeBufferTime;
                    break;
            }

            // 設定這次VChange的起始Encoder為多少.
            if (command.TriggerAGVPosition != null)
                vChangeData.StartEncoder = command.TriggerEncoder;
            else
            {
                for (int i = commandData.CommandList.Count - 1; i >= 0; i--)
                {
                    if (commandData.CommandList[i].TriggerAGVPosition != null)
                    {
                        double deltaEncoder = 0;

                        if (commandData.CommandList[i].CmdType == EnumCommandType.STurn)
                            deltaEncoder = localData.MoveControlData.TurnParameter[commandData.CommandList[i].TurnType].R * 2;
                        else if (commandData.CommandList[i].CmdType == EnumCommandType.RTurn)
                            deltaEncoder = localData.MoveControlData.TurnParameter[commandData.CommandList[i].TurnType].R * Math.Sqrt(2);
                        else if (commandData.CommandList[i].CmdType == EnumCommandType.Move)
                            deltaEncoder = commandData.CommandList[i].SafetyDistance / 2;

                        vChangeData.StartEncoder = commandData.CommandList[i].TriggerEncoder + deltaEncoder;
                        break;
                    }
                }
            }

            bool result = true;

            if (vChangeList.Count == 0)
                vChangeData.StartVelocity = 0;
            else
            {
                double bufferDistance = vChangeList[vChangeList.Count - 1].VelocityCommand * vChangeList[vChangeList.Count - 1].BufferTime / 1000;
                //double bufferDistance = vChangeList[vChangeList.Count - 1].VelocityCommand * vChangeData.BufferTime / 1000;

                if (vChangeList[vChangeList.Count - 1].EndEncoder + bufferDistance > vChangeData.StartEncoder)
                {
                    double newVelocityCommand = 0;
                    result = GetLastVChangeVelocityCommandByTwoVChangeData(vChangeList[vChangeList.Count - 1], vChangeData, ref newVelocityCommand);

                    if (result)
                        OverRrideLastVChangeCommand(ref commandData, data, ref vChangeList, newVelocityCommand);
                    else
                        OverRrideLastVChangeCommand(ref commandData, data, ref vChangeList, newVelocityCommand, (EnumVChangeType)command.Type);
                }

                vChangeData.StartVelocity = vChangeList[vChangeList.Count - 1].VelocityCommand;
            }

            vChangeData.EndEncoder = vChangeData.StartEncoder + GetAccDecDistanceFormMove(vChangeData.StartVelocity, vChangeData.VelocityCommand);

            if (result)
                vChangeList.Add(vChangeData);

            data.NowVelocityCommand = vChangeData.VelocityCommand;
        }

        public double GetVChangeDistanceInAcc(double startVel, double nextVelocity, double targetVel, double distance)
        {
            double jerk = config.Move.Jerk;
            double acc = config.Move.Acceleration;
            double dec = config.Move.Deceleration;
            double velocity = startVel;

            double accDistance = 0;
            double decDistance = 0;
            double stopStartAccDistance = GetAccDecDistanceFormMove(0, nextVelocity);

            accDistance = GetAccDecDistanceFormMove(startVel, targetVel);
            decDistance = GetAccDecDistanceFormMove(targetVel, 0);

            if (accDistance + decDistance + stopStartAccDistance < distance)
                return decDistance + stopStartAccDistance;

            while (GetAccDecDistanceFormMove(startVel, velocity + createCommandSearchVelcoityDelta) + GetAccDecDistanceFormMove(velocity + createCommandSearchVelcoityDelta, 0) + stopStartAccDistance < distance)
                velocity = velocity + createCommandSearchVelcoityDelta;

            accDistance = GetAccDecDistanceFormMove(startVel, velocity);

            double time = acc / jerk;
            double deltaVelocity = (double)1 / 2 * jerk * Math.Pow(time, 2);

            if (Math.Abs(startVel - velocity) < deltaVelocity * 2)
                time = accDistance / (startVel + velocity);

            accDistance = accDistance - (velocity * time - 1 / 6 * jerk * Math.Pow(time, 3));

            return distance - accDistance;
        }

        public double GetNewVChangeVelocityInNextVChangeEnd(VChangeData lastData, double nextVelociy, double distance)
        {
            double delta = (lastData.VelocityCommand - lastData.StartVelocity) > 0 ? 50 : -50; // 5 > 50 2020/7/2 add.

            double newVelocity = lastData.StartVelocity;

            double vChangeDistance = GetAccDecDistanceFormMove(lastData.StartVelocity, newVelocity) + newVelocity * lastData.BufferTime / 1000 + GetAccDecDistanceFormMove(newVelocity, nextVelociy);

            // 2020/7/2 add.
            if (vChangeDistance > distance)
                return lastData.StartVelocity;

            while (vChangeDistance <= distance)
            {
                newVelocity += delta;
                vChangeDistance = GetAccDecDistanceFormMove(lastData.StartVelocity, newVelocity) + newVelocity * lastData.BufferTime / 1000 + GetAccDecDistanceFormMove(newVelocity, nextVelociy);
            }

            newVelocity -= delta;

            return newVelocity;
        }

        private void ProcessVelocityChangeFinishInDistance(OneceMoveCommand oneceMoveCommand, ref MoveCommandData commandData, ref Command command,
                                           DecompositionCommandData data, MapAGVPosition agvPosition, ref List<VChangeData> vChangeList)
        {
            MapAGVPosition triggerPosition;
            if (vChangeList.Count == 0)
                return;

            VChangeData lastVChange = vChangeList[vChangeList.Count - 1];
            double distanceToPosition = command.TriggerEncoder;
            double distance;
            double triggerEncoder;

            if (lastVChange.VelocityCommand == command.Velocity)
            {   // 速度依樣
                triggerPosition = computeFunction.GetAGVPositionFormEndDistance(oneceMoveCommand.PositionList[data.Index - 1], agvPosition, distanceToPosition);
                command.TriggerEncoder = data.StartEncoder + data.TotalDistance - distanceToPosition;
                command.TriggerAGVPosition = triggerPosition;
                AddCommandToCommandList(ref commandData, command, data, ref vChangeList);
            }
            else
            {
                //data.TurnOutDistance
                distance = GetAccDecDistanceFormMove(lastVChange.VelocityCommand, command.Velocity) + distanceToPosition;
                triggerEncoder = data.StartEncoder + data.TotalDistance - distance;

                //double bufferTime = ((EnumVChangeType)command.Type == EnumVChangeType.EQ || (EnumVChangeType)command.Type == EnumVChangeType.SlowStop) ? 0 : lastVChange.BufferTime;
                double bufferTime = lastVChange.BufferTime;

                if (triggerEncoder > lastVChange.EndEncoder + lastVChange.VelocityCommand * bufferTime / 1000)

                { // 正常情況.
                    triggerPosition = computeFunction.GetAGVPositionFormEndDistance(oneceMoveCommand.PositionList[data.Index - 1], agvPosition, distance);
                    command.TriggerEncoder = triggerEncoder;
                    command.TriggerAGVPosition = triggerPosition;
                    AddCommandToCommandList(ref commandData, command, data, ref vChangeList);
                }
                else
                { // 上次速度命令無法執行完.
                    distance = GetAccDecDistanceFormMove(lastVChange.StartVelocity, command.Velocity) + distanceToPosition;
                    triggerEncoder = data.StartEncoder + data.TotalDistance - distance;

                    if (triggerEncoder > lastVChange.StartEncoder)
                    { // 上次變速命令可以執行,但是需要修改速度.
                        // 上次vChange開始到這次需要降到指定velocity的總距離 ( 這個長度要做到 升速->等速段->降速).
                        double realDistance = Math.Abs((data.StartEncoder + data.TotalDistance) - lastVChange.StartEncoder) - distanceToPosition;

                        double newVelocity = GetNewVChangeVelocityInNextVChangeEnd(lastVChange, command.Velocity, realDistance);

                        if (lastVChange.StartVelocity < newVelocity && newVelocity < command.Velocity)
                        {
                            OverRrideLastVChangeCommand(ref commandData, data, ref vChangeList, command.Velocity, (EnumVChangeType)command.Type);
                        }
                        else
                        {
                            distance = GetAccDecDistanceFormMove(newVelocity, command.Velocity) + distanceToPosition;
                            triggerEncoder = data.StartEncoder + data.TotalDistance - distance;
                            triggerPosition = computeFunction.GetAGVPositionFormEndDistance(oneceMoveCommand.PositionList[data.Index - 1], agvPosition, distance);
                            command.TriggerEncoder = triggerEncoder;
                            command.TriggerAGVPosition = triggerPosition;
                            OverRrideLastVChangeCommand(ref commandData, data, ref vChangeList, newVelocity);
                            data.NowVelocityCommand = newVelocity;
                            AddCommandToCommandList(ref commandData, command, data, ref vChangeList);
                        }
                    }
                    else
                    { // 上次速度命令無法執行.
                        if (vChangeList.Count == 1)
                            OverRrideLastVChangeCommand(ref commandData, data, ref vChangeList, command.Velocity, (EnumVChangeType)command.Type);
                        else
                        {
                            int index = FindLastVChangeCommand(commandData);
                            RemoveLastVChangeCommand(ref commandData);
                            vChangeList.RemoveAt(vChangeList.Count - 1);
                            ProcessVelocityChangeFinishInDistance(oneceMoveCommand, ref commandData, ref command, data, agvPosition, ref vChangeList);
                        }
                    }
                }
            }
        }

        private bool ProcessStopCommand(OneceMoveCommand oneceMoveCommand, ref Command command, DecompositionCommandData data, MapAGVPosition agvPosition, double nextVelocity, ref List<VChangeData> vChangeList)
        {
            MapAGVPosition triggerPosition;
            VChangeData lastVChange;
            double distanceToPosition = command.TriggerEncoder;
            double distance;
            double triggerEncoder;

            for (int i = vChangeList.Count - 1; i >= 0; i--)
            {
                lastVChange = vChangeList[i];

                distance = GetAccDecDistanceFormMove(lastVChange.VelocityCommand, 0) +
                           GetAccDecDistanceFormMove(0, nextVelocity) + distanceToPosition;
                triggerEncoder = data.StartEncoder + data.TotalDistance - distance;

                if (triggerEncoder > lastVChange.EndEncoder)
                { // 正常情況.
                    triggerPosition = computeFunction.GetAGVPositionFormEndDistance(oneceMoveCommand.PositionList[data.Index - 1], agvPosition, distance);
                    command.TriggerEncoder = triggerEncoder;
                    command.TriggerAGVPosition = triggerPosition;
                    return distance < data.LineDistance - data.TurnOutDistance;
                }
                else
                { // 上次速度命令無法執行完.
                    distance = GetAccDecDistanceFormMove(lastVChange.StartVelocity, 0) +
                               GetAccDecDistanceFormMove(0, nextVelocity) + distanceToPosition;

                    triggerEncoder = data.StartEncoder + data.TotalDistance - distance;

                    if (triggerEncoder > lastVChange.StartEncoder)
                    { // 上次變速命令可以執行,但是需要修改速度.
                        distance = GetVChangeDistanceInAcc(lastVChange.StartVelocity, nextVelocity, lastVChange.VelocityCommand,
                                                           Math.Abs((data.StartEncoder + data.TotalDistance) - lastVChange.StartEncoder) -
                                                           distanceToPosition) + distanceToPosition;

                        triggerEncoder = data.StartEncoder + (data.TotalDistance - distance);
                        triggerPosition = computeFunction.GetAGVPositionFormEndDistance(oneceMoveCommand.PositionList[data.Index - 1], agvPosition, distance);
                        command.TriggerEncoder = triggerEncoder;
                        command.TriggerAGVPosition = triggerPosition;
                        return distance < data.LineDistance - data.TurnOutDistance;
                    }
                }
            }

            return false;
        }
        #endregion

        #region Turn.
        public bool Step5_STurn(OneceMoveCommand oneceMoveCommand, ref DecompositionCommandData data, ref MoveCommandData commandData, ref List<VChangeData> vChangeList, ref string errorMessage)
        {
            string turnAction = oneceMoveCommand.AddressActions[data.Index];
            double velocityCommand = oneceMoveCommand.SpeedList[data.Index];
            AGVTurnParameter turn = localData.MoveControlData.TurnParameter[turnAction];

            MapAGVPosition triggerPosition;
            Command tempCommand;

            double distance = GetAccDecDistanceFormMove(data.NowVelocity, turn.Velocity);
            double totalDistance = distance + turn.VChangeSafetyDistance + turn.R + data.TurnOutDistance;

            double oldMovingAngle = computeFunction.ComputeAngle(oneceMoveCommand.PositionList[data.Index - 1], oneceMoveCommand.PositionList[data.Index]);
            double newMovingAngle = computeFunction.ComputeAngle(oneceMoveCommand.PositionList[data.Index], oneceMoveCommand.PositionList[data.Index + 1]);


            if (totalDistance > data.LineDistance)
            {
                OverRrideLastVChangeCommand(ref commandData, data, ref vChangeList, turn.Velocity, EnumVChangeType.STurn);
            }
            else
            {
                tempCommand = NewVChangeCommand(null, turn.R + turn.VChangeSafetyDistance, turn.Velocity, EnumVChangeType.STurn);
                ProcessVelocityChangeFinishInDistance(oneceMoveCommand, ref commandData, ref tempCommand, data, data.TempEndAGVPosition, ref vChangeList);
            }

            triggerPosition = computeFunction.GetAGVPositionFormEndDistance(oneceMoveCommand.PositionList[data.Index - 1], data.TempEndAGVPosition, turn.R);

            tempCommand = NewSTurnCommand(triggerPosition, data.StartEncoder + data.TotalDistance - turn.R, turn.TurnName, oldMovingAngle, newMovingAngle);

            AddCommandToCommandList(ref commandData, tempCommand, data, ref vChangeList);

            int reserveIndex = GetReserveIndex(commandData.ReserveList, data.TempEndAGVPosition.Position);

            double safetyDistance = turn.R + config.ReserveSafetyDistance;

            if (CheckNotInTurn(oneceMoveCommand, commandData.ReserveList, data.Index, reserveIndex, safetyDistance, data.LineDistance, turn.Velocity))
            {
                tempCommand = NewStopCommand(null, turn.R + config.ReserveSafetyDistance, reserveIndex + 1);

                if (ProcessStopCommand(oneceMoveCommand, ref tempCommand, data, data.TempEndAGVPosition, turn.Velocity, ref vChangeList))
                    AddCommandToCommandList(ref commandData, tempCommand, data, ref vChangeList);
            }

            tempCommand = NewVChangeCommand(null, 0, velocityCommand, EnumVChangeType.TurnOut);
            AddCommandToCommandList(ref commandData, tempCommand, data, ref vChangeList);

            data.TurnOutDistance = turn.R;
            data.NowVelocity = turn.Velocity;
            data.LineDistance = 0;
            data.LineEndAGVPositionList.Add(data.TempEndAGVPosition);
            return true;
        }
        #endregion

        #region ST/End/SlowStop.
        public bool Step5_ST(OneceMoveCommand oneceMoveCommand, ref DecompositionCommandData data, ref MoveCommandData commandData, ref List<VChangeData> vChangeList, ref string errorMessage)
        {
            Command tempCommand;
            double velocityCommand = oneceMoveCommand.SpeedList[data.Index];
            MapAGVPosition agvPosition = new MapAGVPosition(oneceMoveCommand.PositionList[data.Index], oneceMoveCommand.AGVAngleList[data.Index]);

            if (velocityCommand > data.NowVelocityCommand)
            {
                tempCommand = NewVChangeCommand(new MapAGVPosition(oneceMoveCommand.PositionList[data.Index], oneceMoveCommand.AGVAngleList[data.Index]),
                                                data.StartEncoder + data.TotalDistance, velocityCommand);

                AddCommandToCommandList(ref commandData, tempCommand, data, ref vChangeList);
            }
            else if (velocityCommand < data.NowVelocityCommand)
            {
                errorMessage = "還未處理..";
                tempCommand = NewVChangeCommand(null, 0, velocityCommand);
                //ProcessVelocityChangeFinishInDistance(oneceMoveCommand, ref commandData, ref tempCommand, data, position, ref vChangeList);
            }

            int reserveIndex = GetReserveIndex(commandData.ReserveList, agvPosition.Position);

            double safetyDistance = config.ReserveSafetyDistance;

            if (CheckNotInTurn(oneceMoveCommand, commandData.ReserveList, data.Index, reserveIndex, safetyDistance, data.LineDistance, 0))
            {
                tempCommand = NewStopCommand(null, config.ReserveSafetyDistance, reserveIndex + 1);

                if (ProcessStopCommand(oneceMoveCommand, ref tempCommand, data, agvPosition, 0, ref vChangeList))
                    AddCommandToCommandList(ref commandData, tempCommand, data, ref vChangeList);
            }

            tempCommand = NewChangeSectionLineCommand(agvPosition, data.StartEncoder + data.TotalDistance);
            AddCommandToCommandList(ref commandData, tempCommand, data, ref vChangeList);

            return true;
        }

        public bool Step5_EndSlowStop(OneceMoveCommand oneceMoveCommand, ref DecompositionCommandData data, ref MoveCommandData commandData, ref List<VChangeData> vChangeList, ref string errorMessage)
        {
            string action = oneceMoveCommand.AddressActions[data.Index];
            MapAGVPosition agvPosition = new MapAGVPosition(oneceMoveCommand.PositionList[data.Index], oneceMoveCommand.AGVAngleList[data.Index]);

            data.LineEndAGVPositionList.Add(agvPosition);

            double distance;
            MapAGVPosition triggerPosition;
            Command tempCommand;

            double lessDistance = GetAccDecDistanceFormMove(0, config.EQ.Velocity) + config.EQ.Velocity * config.VChangeBufferTime / 1000 + config.EQ.Position + GetAccDecDistanceFormMove(config.EQ.Velocity, 0);

            // 距離非常短,不做加速到站前減速,改為直接用80速度慢慢走,在停止(和正常情況相比會缺少VChange command).
            if (data.TotalDistance < lessDistance)
            {
                OverRrideLastVChangeCommand(ref commandData, data, ref vChangeList, config.EQ.Velocity);
                triggerPosition = computeFunction.GetAGVPositionFormEndDistance(oneceMoveCommand.PositionList[data.Index - 1], agvPosition, GetSLowStopDistance());
                tempCommand = NewSlowStopCommand(triggerPosition, data.StartEncoder + data.TotalDistance - GetSLowStopDistance(),
                                                 data.StartEncoder + data.TotalDistance,
                                                 (action == EnumDefaultAction.End.ToString() ? EnumSlowStopType.End : EnumSlowStopType.ChangeMovingAngle));
                AddCommandToCommandList(ref commandData, tempCommand, data, ref vChangeList);
            }
            else
            { // 距離不會太短之情況.
                if (data.NowVelocityCommand > config.EQ.Velocity)
                { // 需要降速.
                    distance = (action == EnumDefaultAction.End.ToString() ? config.EQ.Position : config.NormalStopDistance) + GetSLowStopDistance();
                    tempCommand = NewVChangeCommand(null, distance, config.EQ.Velocity, (action == EnumDefaultAction.End.ToString() ? EnumVChangeType.EQ : EnumVChangeType.SlowStop));
                    ProcessVelocityChangeFinishInDistance(oneceMoveCommand, ref commandData, ref tempCommand, data, agvPosition, ref vChangeList);

                    int index = FindLastVChangeCommand(commandData);

                    if (index != -1)
                        commandData.CommandList[index].NowVelocity = vChangeList[vChangeList.Count - 1].StartVelocity;
                }

                // 算出停止距離跟座標.
                distance = GetSLowStopDistance();

                triggerPosition = computeFunction.GetAGVPositionFormEndDistance(oneceMoveCommand.PositionList[data.Index - 1], agvPosition, -distance);

                // 插入停止指令.
                tempCommand = NewSlowStopCommand(triggerPosition, data.StartEncoder + data.TotalDistance - distance, data.StartEncoder + data.TotalDistance, (action == EnumDefaultAction.End.ToString() ? EnumSlowStopType.End : EnumSlowStopType.ChangeMovingAngle));
                AddCommandToCommandList(ref commandData, tempCommand, data, ref vChangeList);
            }

            // 是終點.
            if (action == EnumDefaultAction.End.ToString())
            {
                tempCommand = NewEndCommand(data.StartEncoder + data.TotalDistance, agvPosition);
                AddCommandToCommandList(ref commandData, tempCommand, data, ref vChangeList);
            }

            data.TurnOutDistance = 0;
            return true;
        }
        #endregion

        private bool Step5_CreatMoveCommand_OneMove(ref MoveCommandData commandData, OneceMoveCommand oneceMoveCommand, ref string errorMessage)
        {
            List<VChangeData> vChangeList = new List<VChangeData>();
            double tempDistance;

            DecompositionCommandData data = new DecompositionCommandData();
            data.TotalDistance = 0;
            data.NowVelocity = 0;
            data.NowVelocityCommand = 0;

            Command tempCommand;

            MapAGVPosition tempAGVPosition;

            if (commandData.CommandList.Count != 0)
            {
                data.StartEncoder = commandData.CommandList[commandData.CommandList.Count - 1].EndEncoder;
                data.StartMoveIndex = commandData.CommandList.Count;
            }

            if (oneceMoveCommand.AddressActions[0] == EnumDefaultAction.SpinTurn.ToString())
            {
                tempAGVPosition = new MapAGVPosition();
                tempCommand = NewSpinTurnCommand(new MapAGVPosition(oneceMoveCommand.PositionList[0], oneceMoveCommand.AGVAngleList[0]), data.StartEncoder);
                AddCommandToCommandList(ref commandData, tempCommand, data, ref vChangeList);
                return true;
            }
            else
            {
                if (oneceMoveCommand.AddressActions[0] != EnumDefaultAction.ST.ToString())
                {
                    errorMessage = "RTurn啟動未寫";
                    return false;
                }

                tempAGVPosition = new MapAGVPosition();
                tempCommand = NewMoveCommand(new MapAGVPosition(oneceMoveCommand.PositionList[0], oneceMoveCommand.AGVAngleList[0]), null, data.StartEncoder, oneceMoveCommand.SpeedList[0],
                                                             (data.StartMoveIndex == 0 ? EnumMoveStartType.FirstMove : EnumMoveStartType.ChangeDirFlagMove), (data.StartMoveIndex == 0 ? 0 : -1));

                AddCommandToCommandList(ref commandData, tempCommand, data, ref vChangeList);

                for (; data.Index < oneceMoveCommand.AddressActions.Count; data.Index++)
                {
                    if (data.Index != 0)
                    {
                        tempDistance = computeFunction.GetTwoPositionDistance(oneceMoveCommand.PositionList[data.Index - 1], oneceMoveCommand.PositionList[data.Index]);
                        data.TotalDistance += tempDistance;
                        data.LineDistance += tempDistance;

                        data.TempEndAGVPosition = new MapAGVPosition(oneceMoveCommand.PositionList[data.Index], oneceMoveCommand.AGVAngleList[data.Index]);

                        if (oneceMoveCommand.AddressActions[data.Index] == EnumDefaultAction.ST.ToString())
                        {
                            if (!Step5_ST(oneceMoveCommand, ref data, ref commandData, ref vChangeList, ref errorMessage))
                                return false;
                        }
                        else if (oneceMoveCommand.AddressActions[data.Index] == EnumDefaultAction.End.ToString() ||
                                 oneceMoveCommand.AddressActions[data.Index] == EnumDefaultAction.SlowStop.ToString())
                        {
                            if (!Step5_EndSlowStop(oneceMoveCommand, ref data, ref commandData, ref vChangeList, ref errorMessage))
                                return false;
                        }
                        else if (computeFunction.TurnTypeCheck(oneceMoveCommand.AddressActions[data.Index], EnumTurnType.STurn))
                        {
                            if (!Step5_STurn(oneceMoveCommand, ref data, ref commandData, ref vChangeList, ref errorMessage))
                                return false;
                        }
                        else if (computeFunction.TurnTypeCheck(oneceMoveCommand.AddressActions[data.Index], EnumTurnType.RTurn))
                        {
                            //if (!CreateCommandStep6_RTurn(onceMoveCommand, ref data, ref commandData, ref vChangeList, ref errorMessage))
                            return false;
                        }
                        else
                        {
                            errorMessage = "action not ST、End/SlowStop、STurn、RTurn ??";
                            return false;
                        }
                    }
                }

                //commandData.CommandList[data.StartMoveIndex].Velocity = vChangeList[0].VelocityCommand;

                int index = data.StartMoveIndex;

                for (int i = 0; i < data.LineEndAGVPositionList.Count; i++)
                {
                    while (commandData.CommandList[index].CmdType != EnumCommandType.Move &&
                           commandData.CommandList[index].CmdType != EnumCommandType.STurn &&
                           commandData.CommandList[index].CmdType != EnumCommandType.RTurn)
                        index++;

                    MapAGVPosition commandAGVPosition = new MapAGVPosition();

                    commandAGVPosition.Position.X = 2 * (data.LineEndAGVPositionList[i].Position.X - commandData.CommandList[index].TriggerAGVPosition.Position.X) + commandData.CommandList[index].TriggerAGVPosition.Position.X;
                    commandAGVPosition.Position.Y = 2 * (data.LineEndAGVPositionList[i].Position.Y - commandData.CommandList[index].TriggerAGVPosition.Position.Y) + commandData.CommandList[index].TriggerAGVPosition.Position.Y;
                    commandAGVPosition.Angle = data.LineEndAGVPositionList[i].Angle;

                    commandData.CommandList[index].EndAGVPosition = commandAGVPosition;
                    //commandData.CommandList[index].EndAGVPosition = data.LineEndAGVPositionList[i];
                    index++;
                }

                return true;
            }
        }

        private void Step5_CommandListChangeReserveIndexToCurrectIndex(ref MoveCommandData commandData)
        {
            int nowIndex = commandData.ReserveList.Count - 1;
            int temp = 0;
            for (int i = commandData.CommandList.Count - 1; i >= 0; i--)
            {
                if (commandData.CommandList[i].ReserveNumber != -1)
                {
                    temp = commandData.CommandList[i].ReserveNumber;
                    commandData.CommandList[i].ReserveNumber = nowIndex;
                    nowIndex = temp - 1;
                }
            }
        }

        public bool Step5_CreateMoveCommand(ref MoveCommandData commandData, List<OneceMoveCommand> oneceMoveCommandList, ref string errorMessage)
        {
            try
            {
                commandData.CommandList = new List<Command>();

                for (int i = 0; i < oneceMoveCommandList.Count; i++)
                {
                    if (!Step5_CreatMoveCommand_OneMove(ref commandData, oneceMoveCommandList[i], ref errorMessage))
                        return false;
                }

                for (int i = 0; i < commandData.ReserveList.Count; i++)
                    commandData.ReserveList[i].GetReserve = false;

                Step5_CommandListChangeReserveIndexToCurrectIndex(ref commandData);

                WriteLog_Step5(commandData);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = String.Concat("Exception : ", ex.ToString());
                return false;
            }
        }
        #endregion

        public MoveCommandData CreateMoveCommand(MoveCmdInfo moveCmd, ref string errorMessage)
        {
            System.Diagnostics.Stopwatch createListTimer = new System.Diagnostics.Stopwatch();
            createListTimer.Restart();

            MoveCommandData commandData = new MoveCommandData();

            commandData.StartAddress = moveCmd.StartAddress;
            commandData.EndAddress = moveCmd.EndAddress;

            bool scuess = false;
            List<OneceMoveCommand> oneceMoveCommandList = new List<OneceMoveCommand>();

            if (!Step1_CheckMoveCommandData(moveCmd, ref errorMessage))
                errorMessage = String.Concat("Step1 : ", errorMessage);
            else if (!Step2_CreateAddresActionList(ref moveCmd, ref errorMessage))
                errorMessage = String.Concat("Step2 : ", errorMessage);
            else if (!Step3_SetReserveStopAddress(moveCmd, ref commandData, ref errorMessage))
                errorMessage = String.Concat("Step3 : ", errorMessage);
            else if (!Step4_ChangeToOneceMoveCommandList(moveCmd, ref commandData, ref oneceMoveCommandList, ref errorMessage))
                errorMessage = String.Concat("Step4 : ", errorMessage);
            else if (!Step5_CreateMoveCommand(ref commandData, oneceMoveCommandList, ref errorMessage))
                errorMessage = String.Concat("Step5 : ", errorMessage);
            else
                scuess = true;

            createListTimer.Stop();

            if (scuess)
            {
                commandData.CommandID = moveCmd.CommandID;
                commandData.IsAutoCommand = moveCmd.IsAutoCommand;
                WriteLog(7, "", String.Concat("命令分解成功, 分解時間 : ", createListTimer.ElapsedMilliseconds.ToString(), " ms."));
                return commandData;
            }
            else
            {
                WriteLog(7, "", String.Concat("命令分解失敗, 分解時間 : ", createListTimer.ElapsedMilliseconds.ToString(), " ms."));
                WriteLog(3, "", String.Concat("分解失敗原因 : ", errorMessage));
                WriteLog(3, "", GetStep1String(moveCmd));
                return null;
            }
        }


        public double PrecessRetrunVelocity(double nowVelocityCommand, double nextVelocityCommnad, double returnVelocity, double velocityCommand)
        {                                           // 0                            300                         150
            if (returnVelocity < config.EQ.Velocity)
                returnVelocity = config.EQ.Velocity;

            if (nowVelocityCommand < returnVelocity && returnVelocity < nextVelocityCommnad)
            {
                WriteLog(7, "", String.Concat("command :: nowVelocityCommand = ", nowVelocityCommand.ToString("0"), ", nextVelocityCommnad = ", nextVelocityCommnad.ToString("0"),
                                                         ", velocityCommand = ", velocityCommand.ToString("0"), ", returnVelocity = ", returnVelocity.ToString("0")));

                if (nextVelocityCommnad < velocityCommand)
                {
                    if (nextVelocityCommnad > config.EQ.Velocity)
                        return nextVelocityCommnad;
                    else
                        return config.EQ.Velocity;
                }
                else
                {
                    if (velocityCommand > config.EQ.Velocity)
                        return velocityCommand;
                    else
                        return config.EQ.Velocity;
                }
            }
            else
                return returnVelocity;
        }

        private double GetVChangeCommandDistance(ref double nextVelocity, bool skipNext)
        {
            double distance = 0;

            for (int i = (skipNext ? localData.MoveControlData.MoveCommand.IndexOfCommandList + 1 : localData.MoveControlData.MoveCommand.IndexOfCommandList); i < localData.MoveControlData.MoveCommand.CommandList.Count; i++)
            {
                if (localData.MoveControlData.MoveCommand.CommandList[i].CmdType == EnumCommandType.Vchange)
                {
                    nextVelocity = localData.MoveControlData.MoveCommand.CommandList[i].Velocity;
                    return Math.Abs(localData.MoveControlData.MoveCommand.CommandEncoder - localData.MoveControlData.MoveCommand.CommandList[i].TriggerEncoder);
                }
                else if (localData.MoveControlData.MoveCommand.CommandList[i].CmdType == EnumCommandType.SlowStop)
                {
                    WriteLog(3, "", "return -1");
                    return -1;
                }
            }

            if (distance == 0)
                WriteLog(3, "", "vChangeDistance == 0 !???");

            return distance;
        }

        public double GetVChangeVelocity(double nowVelocityCommand, double VelocityCommand, bool spikNext = false)
        {
            double tempVelocityCommand = VelocityCommand;
            double nextVelocity = -1;

            double distance = GetVChangeCommandDistance(ref nextVelocity, spikNext);

            if (distance == -1)
                return VelocityCommand;

            double deltaDistance = 0;
            double velocity = 0;

            if (localData.SimulateMode)
                WriteLog(7, "", String.Concat("nowVelocity = ", localData.MoveControlData.MotionControlData.LineVelocity.ToString(), ", nowRealEncoder = ", localData.MoveControlData.MoveCommand.CommandEncoder.ToString("0.0"), ", distance = ", distance.ToString(), ", nextVelocity = ", nextVelocity));

            if (!spikNext || localData.MoveControlData.MotionControlData.SimulateIsIsokinetic)
            {
                velocity = nowVelocityCommand;

                if (Math.Abs(nowVelocityCommand - localData.MoveControlData.MotionControlData.LineVelocity) > 50)
                {
                    velocity = localData.MoveControlData.MotionControlData.LineVelocity;
                    WriteLog(3, "", String.Concat("IsIsokinetc, but nowVelocityCommand : ", nowVelocityCommand.ToString("0"), ", NowVelocity : ", localData.MoveControlData.MotionControlData.LineVelocity.ToString("0")));
                }
            }
            else
            {
                SimulateControl.GetAccOrDecZeroDistanceAndVelocity_SpeedUp(ref deltaDistance, ref velocity);

                if (localData.SimulateMode)
                    WriteLog(7, "", String.Concat("nowVelocityCoomand = ", localData.MoveControlData.MoveCommand.NowVelocity.ToString(), ", StartVelocity = ", velocity.ToString(), ", allDistance = ", (distance - deltaDistance).ToString()));

                if (velocity < 0)
                    velocity = 0;
            }

            distance = distance - deltaDistance;

            double tempDistance;

            tempDistance = GetAccDecDistanceFormMove(velocity, VelocityCommand) + VelocityCommand * config.VChangeBufferTime / 1000;
            // 完整變速的距離 ( 包含Buffer ).

            if (tempDistance <= distance)
                return VelocityCommand;
            else if (velocity * config.VChangeBufferTime / 1000 > distance)
                return PrecessRetrunVelocity(velocity, nextVelocity, nowVelocityCommand, tempVelocityCommand);
            else
            {
                double deltaVelcoity = (VelocityCommand > velocity ? 50 : -50);

                double addOneDeltaVelocity = nowVelocityCommand + deltaVelcoity;

                if ((GetAccDecDistanceFormMove(velocity, addOneDeltaVelocity) + addOneDeltaVelocity * config.VChangeBufferTime / 1000) > distance)
                {
                    WriteLog(7, "", "完全無法升速");

                    return PrecessRetrunVelocity(velocity, nextVelocity, nowVelocityCommand, tempVelocityCommand);
                }

                while (tempDistance > distance)
                {
                    VelocityCommand -= deltaVelcoity;
                    tempDistance = GetAccDecDistanceFormMove(velocity, VelocityCommand) + VelocityCommand * config.VChangeBufferTime / 1000;

                    if (nowVelocityCommand > VelocityCommand)
                    {
                        WriteLog(1, "", String.Concat("GetVChangeVelocity Fatal Error nowVelocityCommand > VelocityCommand : ", nowVelocityCommand.ToString("0")));

                        return PrecessRetrunVelocity(velocity, nextVelocity, nowVelocityCommand, tempVelocityCommand);
                    }
                    else if (VelocityCommand < 0)
                    {
                        WriteLog(1, "", "GetVChangeVelocity Fatal Error VelocityCommand < 0");

                        return PrecessRetrunVelocity(velocity, nextVelocity, nowVelocityCommand, tempVelocityCommand);
                    }
                }

                return PrecessRetrunVelocity(velocity, nextVelocity, VelocityCommand, tempVelocityCommand);
            }
        }
    }
}
