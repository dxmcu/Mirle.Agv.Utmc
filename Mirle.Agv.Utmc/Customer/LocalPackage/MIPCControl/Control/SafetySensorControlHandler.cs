using Mirle.Agv.INX.Controller;
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

namespace Mirle.Agv.INX.Control
{
    public class SafetySensorControlHandler
    {
        private ComputeFunction computeFunction = ComputeFunction.Instance;
        private LoggerAgent loggerAgent = LoggerAgent.Instance;
        private LocalData localData = LocalData.Instance;
        private AlarmHandler alarmHandler;
        private MIPCControlHandler mipcControl;
        private string device = MethodInfo.GetCurrentMethod().ReflectedType.Name;
        private string normalLogName = "SafetySensor";

        private SafetySensorConfig config;

        private int safetyLevelCount = Enum.GetNames(typeof(EnumSafetyLevel)).Count();

        public List<SafetySensor> AllSafetySensor = new List<SafetySensor>();

        public uint AllStatus { get; set; } = 0;

        private EnumMovingDirection lastDirection = EnumMovingDirection.None;

        public SafetySensorControlHandler(MIPCControlHandler mipcControl, AlarmHandler alarmHandler, string path)
        {
            this.mipcControl = mipcControl;
            this.alarmHandler = alarmHandler;
            ReadXML(path);
            ProcessConfig();
            InitialSafetySensor();
        }

        private void InitialSafetySensor()
        {
            SafetySensor temp;

            for (int i = 0; i < config.SafetySensorList.Count; i++)
            {
                temp = null;

                switch (config.SafetySensorList[i].DeviceType)
                {
                    case EnumDeviceType.Tim781:
                        temp = new SafetySensor_Tim781();
                        break;

                    case EnumDeviceType.Bumper:
                        temp = new SafetySensor_Bumper();
                        break;
                    case EnumDeviceType.EMO:
                        temp = new SafetySensor_EMO();
                        break;
                    case EnumDeviceType.None:
                    default:
                        WriteLog(3, "", String.Concat("Device : ", config.SafetySensorList[i].Device, ", DeviceType : ", config.SafetySensorList[i].DeviceType.ToString(), " Initial未實作"));
                        break;
                }

                if (temp != null)
                {
                    temp.Initial(mipcControl, alarmHandler, config.SafetySensorList[i]);
                    AllSafetySensor.Add(temp);
                }
            }
        }

        public void UpdateAllSafetySensor()
        {
            #region SafetySensor區域設定.
            EnumMovingDirection tempDirection = localData.MIPCData.MoveControlDirection;

            switch (tempDirection)
            {
                case EnumMovingDirection.Front:
                    if (localData.MIPCData.BypassFront)
                        tempDirection = EnumMovingDirection.None;
                    break;
                case EnumMovingDirection.Back:
                    if (localData.MIPCData.BypassBack)
                        tempDirection = EnumMovingDirection.None;
                    break;
                case EnumMovingDirection.Left:
                    if (localData.MIPCData.BypassLeft)
                        tempDirection = EnumMovingDirection.None;
                    break;
                case EnumMovingDirection.Right:
                    if (localData.MIPCData.BypassRight)
                        tempDirection = EnumMovingDirection.None;
                    break;
                case EnumMovingDirection.FrontLeft:
                    if (localData.MIPCData.BypassFront && localData.MIPCData.BypassLeft)
                        tempDirection = EnumMovingDirection.None;
                    else if (localData.MIPCData.BypassFront)
                        tempDirection = EnumMovingDirection.Left;
                    else if (localData.MIPCData.BypassLeft)
                        tempDirection = EnumMovingDirection.Front;
                    break;
                case EnumMovingDirection.FrontRight:
                    if (localData.MIPCData.BypassFront && localData.MIPCData.BypassRight)
                        tempDirection = EnumMovingDirection.None;
                    else if (localData.MIPCData.BypassFront)
                        tempDirection = EnumMovingDirection.Right;
                    else if (localData.MIPCData.BypassRight)
                        tempDirection = EnumMovingDirection.Front;
                    break;
                case EnumMovingDirection.BackLeft:
                    if (localData.MIPCData.BypassBack && localData.MIPCData.BypassLeft)
                        tempDirection = EnumMovingDirection.None;
                    else if (localData.MIPCData.BypassBack)
                        tempDirection = EnumMovingDirection.Left;
                    else if (localData.MIPCData.BypassLeft)
                        tempDirection = EnumMovingDirection.Back;
                    break;
                case EnumMovingDirection.BackRight:
                    if (localData.MIPCData.BypassBack && localData.MIPCData.BypassRight)
                        tempDirection = EnumMovingDirection.None;
                    else if (localData.MIPCData.BypassBack)
                        tempDirection = EnumMovingDirection.Right;
                    else if (localData.MIPCData.BypassRight)
                        tempDirection = EnumMovingDirection.Back;
                    break;
                default:
                    break;
            }

            if (tempDirection != lastDirection)
            {
                WriteLog(7, "", String.Concat("MovingDirection : ", lastDirection.ToString(), " change to ", tempDirection.ToString()));

                for (int i = 0; i < AllSafetySensor.Count; i++)
                    AllSafetySensor[i].ChangeMovingDirection(lastDirection);

                lastDirection = tempDirection;
            }
            #endregion

            uint tempStatus = 0;

            for (int i = 0; i < AllSafetySensor.Count; i++)
            {
                AllSafetySensor[i].UpdateStatus();
                tempStatus = tempStatus | AllSafetySensor[i].Status;
            }

            AllStatus = tempStatus;

            EnumSafetyLevel newLevel = EnumSafetyLevel.Normal;

            for (int i = safetyLevelCount - 1; i >= 0; i--)
            {
                if ((tempStatus & (1 << i)) != 0 && ((EnumSafetyLevel)i) != EnumSafetyLevel.Warn)
                {
                    newLevel = ((EnumSafetyLevel)i);
                    break;
                }
            }

            if (newLevel != localData.MIPCData.safetySensorStatus)
            {
                if (newLevel == EnumSafetyLevel.IPCEMO)
                {
                    WriteLog(7, "", String.Concat("動力電斷電"));
                    mipcControl.SendMIPCDataByIPCTagName(new List<EnumMecanumIPCdefaultTag>() { EnumMecanumIPCdefaultTag.MIPCReady }, new List<float>() { 0 });
                }

                WriteLog(7, "", String.Concat("safetySensorStatus 從 ", localData.MIPCData.safetySensorStatus.ToString(), " 切換至 ", newLevel.ToString()));
                localData.MIPCData.SafetySensorStatus = newLevel;
            }
        }

        public void ByPassBySafetyLevel(EnumSafetyLevel byPassLevel)
        {
            switch (byPassLevel)
            {
                case EnumSafetyLevel.Alarm:
                    if (!localData.MIPCData.AllByPassLevel.ContainsKey(byPassLevel))
                    {
                        localData.MIPCData.AllByPassLevel.Add(byPassLevel, 0);
                        SendAlarmCode(EnumMIPCControlErrorCode.SensorSafety_AlarmByPass);
                    }

                    break;
                case EnumSafetyLevel.IPCEMO:
                    if (!localData.MIPCData.AllByPassLevel.ContainsKey(byPassLevel))
                    {
                        localData.MIPCData.AllByPassLevel.Add(byPassLevel, 0);
                        SendAlarmCode(EnumMIPCControlErrorCode.SensorSafety_IPCEMOByPass);
                    }

                    break;
                case EnumSafetyLevel.EMS:
                    if (!localData.MIPCData.AllByPassLevel.ContainsKey(byPassLevel))
                    {
                        localData.MIPCData.AllByPassLevel.Add(byPassLevel, 0);
                        SendAlarmCode(EnumMIPCControlErrorCode.SensorSafety_EMSByPass);
                    }

                    break;
                case EnumSafetyLevel.SlowStop:
                    if (!localData.MIPCData.AllByPassLevel.ContainsKey(byPassLevel))
                    {
                        localData.MIPCData.AllByPassLevel.Add(byPassLevel, 0);
                        SendAlarmCode(EnumMIPCControlErrorCode.SensorSafety_SlowStopByPass);
                    }

                    break;
                default:

                    WriteLog(5, "", String.Concat("無Bypass ", byPassLevel.ToString(), " 的功能"));
                    break;
            }
        }

        public void ResetByPass()
        {
            localData.MIPCData.AllByPassLevel = new Dictionary<EnumSafetyLevel, int>();
        }

        public void ResetAlarm()
        {
            if (localData.MIPCData.AllByPassLevel.ContainsKey(EnumSafetyLevel.Alarm))
                SendAlarmCode(EnumMIPCControlErrorCode.SensorSafety_AlarmByPass);

            if (localData.MIPCData.AllByPassLevel.ContainsKey(EnumSafetyLevel.IPCEMO))
                SendAlarmCode(EnumMIPCControlErrorCode.SensorSafety_IPCEMOByPass);

            if (localData.MIPCData.AllByPassLevel.ContainsKey(EnumSafetyLevel.EMS))
                SendAlarmCode(EnumMIPCControlErrorCode.SensorSafety_EMSByPass);

            if (localData.MIPCData.AllByPassLevel.ContainsKey(EnumSafetyLevel.SlowStop))
                SendAlarmCode(EnumMIPCControlErrorCode.SensorSafety_SlowStopByPass);

            for (int i = 0; i < AllSafetySensor.Count; i++)
                AllSafetySensor[i].ResetAlarm();
        }

        #region WriteLog/SendAlarmCode.
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

        private void SendAlarmCode(EnumMIPCControlErrorCode alarmCode)
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
        #endregion

        #region XML.
        private List<EnumSafetyLevel> ReadSafetyLevelList(XmlElement element)
        {
            List<EnumSafetyLevel> returnList = new List<EnumSafetyLevel>();

            return returnList;
        }

        private List<string> ReadTagList(XmlElement element)
        {
            List<string> returnStringList = new List<string>();

            foreach (XmlNode item in element.ChildNodes)
            {
                if (item.Name == "Tag")
                {
                    if (localData.MIPCData.AllDataByMIPCTagName.ContainsKey(item.InnerText))
                    {
                        returnStringList.Add(item.InnerText);
                    }
                    else
                    {
                        WriteLog(1, "", String.Concat("Tag : ", item.InnerText, " not find in AllDataByMIPCTagName"));
                        return new List<string>();
                    }
                }
                else
                    WriteLog(1, "", String.Concat("TagList Config must be <Tag>"));
            }

            return returnStringList;
        }

        private void ReadInputDataXML(ref SafetySensorData temp, XmlElement element)
        {
            bool a = true;
            bool readLevel = false;
            EnumSafetyLevel level = EnumSafetyLevel.Normal;

            bool readTag = false;
            string tag = "";

            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "MIPCTagName":
                        if (localData.MIPCData.AllDataByMIPCTagName.ContainsKey(item.InnerText))
                        {
                            tag = item.InnerText;
                            readTag = true;
                        }
                        else
                            WriteLog(1, "", String.Concat("Tag : ", item.InnerText, " not find in AllDataByMIPCTagName"));

                        break;
                    case "SafetyLevel":
                        if (Enum.TryParse(item.InnerText, out level))
                            readLevel = true;
                        else
                            WriteLog(3, "", String.Concat("無此SafetyLevel : ", item.InnerText));

                        break;
                    case "AB":
                        if (item.InnerText == "B")
                            a = false;

                        break;
                    default:
                        break;
                }
            }

            if (readTag && readLevel)
            {
                temp.InputSafetyLevelList.Add(level);
                temp.MIPCTagNmaeInput.Add(tag);
                temp.ABList.Add(a);
            }
        }

        private void ReadInputAndSafetyLevel(ref SafetySensorData temp, XmlElement element)
        {
            foreach (XmlNode item in element.ChildNodes)
            {
                ReadInputDataXML(ref temp, (XmlElement)item);
            }
        }

        private Dictionary<EnumMovingDirection, string> ReadMovingDirectionXML(XmlElement element, int inputCount)
        {
            Dictionary<EnumMovingDirection, string> returnData = new Dictionary<EnumMovingDirection, string>();
            EnumMovingDirection type;

            foreach (XmlNode item in element.ChildNodes)
            {
                if (Enum.TryParse(item.Name, out type))
                {
                    if (item.InnerText.Length == inputCount)
                    {
                        if (returnData.ContainsKey(type))
                        {
                            WriteLog(1, "", String.Concat("EnumMovingDirection type : ", item.Name, " 重複"));
                            return new Dictionary<EnumMovingDirection, string>();
                        }
                        else
                            returnData.Add(type, item.InnerText);
                    }
                    else
                    {
                        WriteLog(1, "", String.Concat("EnumMovingDirection type : ", item.Name, ", uint : ", item.InnerText, " 和input數量不相符"));
                        return new Dictionary<EnumMovingDirection, string>();
                    }
                }
                else
                {
                    WriteLog(1, "", String.Concat("EnumMovingDirection 中無此種type : ", item.Name));
                    return new Dictionary<EnumMovingDirection, string>();
                }
            }

            return returnData;
        }

        private void ReadSafetySensorXML(XmlElement element)
        {
            SafetySensorData temp = new SafetySensorData();

            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "Device":
                        temp.Device = item.InnerText;
                        break;

                    case "Type":
                        EnumSafetySensorType type;

                        if (Enum.TryParse(item.InnerText, out type))
                            temp.Type = type;
                        else
                            WriteLog(1, "", String.Concat("Device : ", temp.Device, ", Type TypeParse Fail : ", item.InnerText));
                        break;
                    case "DeviceType":
                        EnumDeviceType deviceType;

                        if (Enum.TryParse(item.InnerText, out deviceType))
                            temp.DeviceType = deviceType;
                        else
                            WriteLog(1, "", String.Concat("Device : ", temp.Device, ", DeviceType TypeParse Fail : ", item.InnerText));

                        break;
                    case "BeamSensorDircetion":
                        if (temp.Type == EnumSafetySensorType.BeamSensor || temp.Type == EnumSafetySensorType.Bumper)
                        {
                            EnumMovingDirection beamSensorDircetion;

                            if (Enum.TryParse(item.InnerText, out beamSensorDircetion))
                                temp.BeamSensorDircetion = beamSensorDircetion;
                            else
                                WriteLog(1, "", String.Concat("Device : ", temp.Device, " BeamSensorDircetion Read Fail"));
                        }
                        else
                            WriteLog(1, "", String.Concat("Device : ", temp.Device, " type != BeamSensor 不應該有BeamSensorDircetion"));
                        break;

                    case "MIPCTagNameOutput":
                        if (temp.Type == EnumSafetySensorType.AreaSensor)
                            temp.MIPCTagNameOutput = ReadTagList((XmlElement)item);
                        else
                            WriteLog(1, "", String.Concat("Device : ", temp.Device, " type != AreaSensor 不應該有MIPCTagNmaeInput"));
                        break;

                    case "MIPCTagNmaeInput":
                        ReadInputAndSafetyLevel(ref temp, (XmlElement)item);
                        break;

                    case "AreaSensorChangeDircetion":
                        if (temp.Type == EnumSafetySensorType.AreaSensor)
                            temp.AreaSensorChangeDircetion = ReadMovingDirectionXML((XmlElement)item, temp.MIPCTagNmaeInput.Count);
                        else
                            WriteLog(1, "", String.Concat("Device : ", temp.Device, " type != AreaSensor 不應該有AreaSensorChangeDircetion"));
                        break;

                    default:
                        break;
                }
            }

            config.SafetySensorList.Add(temp);
        }

        private void ReadXML(string path)
        {
            try
            {
                config = new SafetySensorConfig();

                if (path == null || path == "")
                {
                    WriteLog(3, "", "SafetySensorConfig 路徑錯誤為null或空值");
                    return;
                }
                else if (!File.Exists(path))
                {
                    WriteLog(1, "", "找不到SafetySensorConfig.xml.");
                    return;
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlElement rootNode = doc.DocumentElement;

                string locatePath = new DirectoryInfo(path).Parent.FullName;

                foreach (XmlNode item in rootNode.ChildNodes)
                {
                    switch (item.Name)
                    {
                        case "SafetySensor":
                            ReadSafetySensorXML((XmlElement)item);
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
        #endregion

        public void ProcessConfig()
        {
            List<float> ioList;

            for (int i = 0; i < config.SafetySensorList.Count; i++)
            {
                // output.
                switch (config.SafetySensorList[i].Type)
                {
                    case EnumSafetySensorType.AreaSensor:
                        foreach (var temp in config.SafetySensorList[i].AreaSensorChangeDircetion)
                        {
                            ioList = new List<float>();

                            for (int j = 0; j < temp.Value.Length; j++)
                                ioList.Add(temp.Value[j] == '0' ? 0 : 1);

                            config.SafetySensorList[i].AreaSensorChangeDircetionInputIOList.Add(temp.Key, ioList);
                        }

                        break;
                    case EnumSafetySensorType.BeamSensor:
                        WriteLog(1, "", "未實作");
                        break;
                    default:
                        break;
                }

                // input.
                for (int j = 0; j < config.SafetySensorList[i].MIPCTagNmaeInput.Count; j++)
                {
                    if (!config.SafetySensorList[i].MIPCInputTagNameToBit.ContainsKey(config.SafetySensorList[i].MIPCTagNmaeInput[j]))
                        config.SafetySensorList[i].MIPCInputTagNameToBit.Add(config.SafetySensorList[i].MIPCTagNmaeInput[j], (int)config.SafetySensorList[i].InputSafetyLevelList[j]);
                    else
                        WriteLog(1, "", String.Concat("Device : ", config.SafetySensorList[i].Device, " , mipcTagNmae 重複(bit), TageName : ", config.SafetySensorList[i].MIPCTagNmaeInput[j]));

                    if (!config.SafetySensorList[i].MIPCInputTagNameToAB.ContainsKey(config.SafetySensorList[i].MIPCTagNmaeInput[j]))
                        config.SafetySensorList[i].MIPCInputTagNameToAB.Add(config.SafetySensorList[i].MIPCTagNmaeInput[j], config.SafetySensorList[i].ABList[j]);
                    else
                        WriteLog(1, "", String.Concat("Device : ", config.SafetySensorList[i].Device, " , mipcTagNmae 重複(AB), TageName : ", config.SafetySensorList[i].MIPCTagNmaeInput[j]));
                }
            }
        }
    }
}
