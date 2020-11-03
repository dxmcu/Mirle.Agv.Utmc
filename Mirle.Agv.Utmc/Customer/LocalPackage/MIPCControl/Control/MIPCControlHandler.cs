using Mirle.Agv.INX.Configs;
using Mirle.Agv.INX.Control;
using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Model;
using Mirle.Agv.INX.Model.Configs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Mirle.Agv.INX.Controller
{
    public class MIPCControlHandler
    {
        private ComputeFunction computeFunction = ComputeFunction.Instance;
        private LoggerAgent loggerAgent = LoggerAgent.Instance;
        private LocalData localData = LocalData.Instance;
        private string device = MethodInfo.GetCurrentMethod().ReflectedType.Name;
        private string normalLogName = "MIPC";
        private string socketLogName = "MIPCSocket";
        private string mipcDebug = "MIPCDebug";
        private Logger logger = LoggerAgent.Instance.GetLooger("BatteryCSV");
        private AlarmHandler alarmHandler;
        private MIPCConfig config;

        private Dictionary<string, MIPCData> allDataByMIPCTagName = new Dictionary<string, MIPCData>();
        private Dictionary<string, MIPCData> allDataByIPCTagName = new Dictionary<string, MIPCData>();
        public Dictionary<string, List<MIPCData>> AllDataByClassification { get; set; } = new Dictionary<string, List<MIPCData>>();
        private Dictionary<int, List<MIPCData>> pollingGroup = new Dictionary<int, List<MIPCData>>();

        private Dictionary<string, Thread> allSocketThread = new Dictionary<string, Thread>();
        private Dictionary<string, Socket> allSocket = new Dictionary<string, Socket>();

        private Thread pollingThread;

        private Dictionary<string, object> allReadObject = new Dictionary<string, object>();
        private Dictionary<string, object> allWriteObject = new Dictionary<string, object>();
        private Dictionary<string, Queue<SendAndReceive>> allReadQueue = new Dictionary<string, Queue<SendAndReceive>>();
        private Dictionary<string, Queue<SendAndReceive>> allWriteQueue = new Dictionary<string, Queue<SendAndReceive>>();

        private Dictionary<int, MIPCPollingData> allPollingData = new Dictionary<int, MIPCPollingData>();
        private List<MIPCPollingData> pollingDataList = new List<MIPCPollingData>();
        private List<int> pollingIntervalList = new List<int>();

        private object newSendLockObject = new object();
        private UInt16 count = 0;

        private UInt32 dataStartAddress = 0;
        private Byte[] allData { get; set; }
        private UInt32 ipcHeartbeatNumber = 0;

        public uint MoveControlHeartBeat { get; set; } = 0;
        private uint lastMoveControlHeartBeat = 0;

        private Thread batteryCSVThread = null;
        private SendAndReceive sendHeartbeat = null;

        private EnumControlStatus status = EnumControlStatus.NotInitial;
        private bool resetAlarm = false;
        private Thread resetAlarmThread = null;

        private TimeStampData timeStampData = null;

        private double overflowValue = 10000;

        public SafetySensorControlHandler SafetySensorControl { get; set; } = null;
        private MainFlowHandler main = null;

        private Dictionary<EnumMIPCControlErrorCode, bool> mipcAlarm = new Dictionary<EnumMIPCControlErrorCode, bool>();

        public EnumControlStatus Status
        {
            get
            {
                if (resetAlarm)
                    return EnumControlStatus.ResetAlarm;
                else
                    return status;
            }

            set
            {
                status = value;
            }
        }

        private List<EnumDefaultAxisName> axisList = new List<EnumDefaultAxisName>() { EnumDefaultAxisName.XFL, EnumDefaultAxisName.XFR, EnumDefaultAxisName.XRL, EnumDefaultAxisName.XRR };

        private UInt32[] moveCommandAddressArray = new UInt32[0];
        private List<string> moveCommandTagNameList = new List<string>()
        {
            EnumMecanumIPCdefaultTag.Command_MapX.ToString(),
            EnumMecanumIPCdefaultTag.Command_MapY.ToString(),
            EnumMecanumIPCdefaultTag.Command_MapTheta.ToString(),
            EnumMecanumIPCdefaultTag.Command_線速度.ToString(),
            EnumMecanumIPCdefaultTag.Command_線加速度.ToString(),
            EnumMecanumIPCdefaultTag.Command_線減速度.ToString(),
            EnumMecanumIPCdefaultTag.Command_線急跳度.ToString(),
            EnumMecanumIPCdefaultTag.Command_角速度.ToString(),
            EnumMecanumIPCdefaultTag.Command_角加速度.ToString(),
            EnumMecanumIPCdefaultTag.Command_角減速度.ToString(),
            EnumMecanumIPCdefaultTag.Command_角急跳度.ToString(),
            EnumMecanumIPCdefaultTag.Command_Start.ToString()
        };

        private UInt32[] jogjoystickDataArray = new UInt32[0];
        private List<string> jogjoystickDataList = new List<string>()
        {
            EnumMecanumIPCdefaultTag.Joystick_LineVelocity.ToString(),
            EnumMecanumIPCdefaultTag.Joystick_LineAcc.ToString(),
            EnumMecanumIPCdefaultTag.Joystick_LineDec.ToString(),
            EnumMecanumIPCdefaultTag.Joystick_ThetaVelocity.ToString(),
            EnumMecanumIPCdefaultTag.Joystick_ThetaAcc.ToString(),
            EnumMecanumIPCdefaultTag.Joystick_ThetaDec.ToString()
        };

        private UInt32[] changeVelociyAddressArray = new UInt32[0];
        private List<string> changeVelociyTagNameList = new List<string>()
        {
            EnumMecanumIPCdefaultTag.Command_線速度.ToString()
            //,
            //EnumMecanumIPCdefaultTag.Command_Start.ToString()
        };

        private UInt32[] changeEndAddressArray = new UInt32[0];
        private List<string> changeEndTagNameList = new List<string>()
        {
            EnumMecanumIPCdefaultTag.Command_MapX.ToString(),
            EnumMecanumIPCdefaultTag.Command_MapY.ToString(),
            EnumMecanumIPCdefaultTag.Command_MapTheta.ToString()
            //,
            //EnumMecanumIPCdefaultTag.Command_Start.ToString()
        };

        private UInt32[] setPositionAddressArray = new UInt32[0];
        private List<string> setPositionTagNameList = new List<string>()
        {
            EnumMecanumIPCdefaultTag.SetPosition_MapX.ToString(),
            EnumMecanumIPCdefaultTag.SetPosition_MapY.ToString(),
            EnumMecanumIPCdefaultTag.SetPosition_MapTheta.ToString(),
            EnumMecanumIPCdefaultTag.SetPosition_TimeStmap.ToString(),
            EnumMecanumIPCdefaultTag.SetPosition_Start.ToString()
        };

        private UInt32[] turnCommandAddressArray = new UInt32[0];

        private List<string> turnCommandTagNameList = new List<string>()
        {
            EnumMecanumIPCdefaultTag.Turn_MapX.ToString(),
            EnumMecanumIPCdefaultTag.Turn_MapY.ToString(),
            EnumMecanumIPCdefaultTag.Turn_MapTheta.ToString(),
            EnumMecanumIPCdefaultTag.Turn_R.ToString(),
            EnumMecanumIPCdefaultTag.Turn_Theta.ToString(),
            EnumMecanumIPCdefaultTag.Turn_Velocity.ToString(),
            EnumMecanumIPCdefaultTag.Turn_MovingAngle.ToString(),
            EnumMecanumIPCdefaultTag.Turn_DeltaTheta.ToString(),
            EnumMecanumIPCdefaultTag.Turn_Start.ToString()
        };

        private UInt32[] stopCommandAddressArray = new UInt32[0];

        private List<string> stopCommandTagNameList = new List<string>()
        {
            EnumMecanumIPCdefaultTag.Command_線減速度.ToString(),
            EnumMecanumIPCdefaultTag.Command_線急跳度.ToString(),
            EnumMecanumIPCdefaultTag.Command_角減速度.ToString(),
            EnumMecanumIPCdefaultTag.Command_角急跳度.ToString(),
            EnumMecanumIPCdefaultTag.Command_Stop.ToString()
        };

        private Dictionary<string, bool> allIPCTageName = new Dictionary<string, bool>();

        private void AddMIPCAlarmCode()
        {
            foreach (EnumMIPCControlErrorCode item in (EnumMIPCControlErrorCode[])Enum.GetValues(typeof(EnumMIPCControlErrorCode)))
            {
                switch (item)
                {
                    case EnumMIPCControlErrorCode.MIPC初始化失敗:
                        alarmHandler.TryAddNewAlarmCode((int)item, item.ToString(), EnumAlarmLevel.Alarm, item.ToString());
                        break;
                    case EnumMIPCControlErrorCode.MIPC連線失敗:
                        alarmHandler.TryAddNewAlarmCode((int)item, item.ToString(), EnumAlarmLevel.Alarm, item.ToString());
                        break;
                    case EnumMIPCControlErrorCode.MIPC斷線:
                        alarmHandler.TryAddNewAlarmCode((int)item, item.ToString(), EnumAlarmLevel.Alarm, item.ToString());
                        break;
                    case EnumMIPCControlErrorCode.MIPC通訊異常:
                        alarmHandler.TryAddNewAlarmCode((int)item, item.ToString(), EnumAlarmLevel.Warn, item.ToString());
                        break;
                    case EnumMIPCControlErrorCode.MIPC回傳資料異常:
                        alarmHandler.TryAddNewAlarmCode((int)item, item.ToString(), EnumAlarmLevel.Warn, item.ToString());
                        break;
                    default:
                        break;
                }
            }
        }

        public MIPCControlHandler(MainFlowHandler main, AlarmHandler alarmHandler)
        {
            this.main = main;
            this.alarmHandler = alarmHandler;
            AddMIPCAlarmCode();
            ReadXMLAndConfig();

            SafetySensorControl = new SafetySensorControlHandler(this, alarmHandler, config.SafetySensorConfigPath);

            if (localData.SimulateMode)
            {
                Status = EnumControlStatus.Ready;
            }
            else
            {
                for (int i = 0; i < config.PortList.Count; i++)
                    socketListConnect.Add(false);

                if (Status == EnumControlStatus.Initial)
                    InitialSocketAndThread();
                else
                    SendAlarmCode(EnumMIPCControlErrorCode.MIPC初始化失敗);
            }
        }

        public void CloseMipcControlHandler()
        {
            Status = EnumControlStatus.Closing;

            Stopwatch timer = new Stopwatch();
            timer.Restart();

            while (timer.ElapsedMilliseconds < 2000 && Status != EnumControlStatus.Closed)
            {
                Thread.Sleep(50);
            }

            foreach (Socket socket in allSocket.Values)
            {
                try
                {
                    socket.Dispose();
                }
                catch { }
            }
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

        public void WriteSocketLog(int logLevel, string carrierId, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            LogFormat logFormat = new LogFormat(socketLogName, logLevel.ToString(), memberName, device, carrierId, message);

            loggerAgent.Log(logFormat.Category, logFormat);
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

        #region ReadXML/Config.
        private bool ReadPortXML(XmlElement element)
        {
            try
            {
                MIPCPortData temp = new MIPCPortData();

                foreach (XmlNode item in element.ChildNodes)
                {
                    switch (item.Name)
                    {
                        case "PortNumber":
                            temp.PortNumber = Int32.Parse(item.InnerText);
                            break;
                        case "ConnectType":
                            EnumMIPCConnectType type;

                            if (Enum.TryParse(item.InnerText, out type))
                                temp.ConnectType = type;
                            else
                            {
                                WriteLog(3, "", String.Concat("ConnectType Error : ", item.InnerText));
                                return false;
                            }

                            break;
                        case "SocketName":
                            temp.SocketName = item.InnerText;
                            break;
                        default:
                            break;
                    }
                }

                if (temp.PortNumber == -1 || temp.ConnectType == EnumMIPCConnectType.None || temp.SocketName == "")
                {
                    WriteLog(3, "", "Port- PortNumber/ConnectType/TagName 皆需設定");
                    return false;
                }

                for (int i = 0; i < config.PortList.Count; i++)
                {
                    if (config.PortList[i].PortNumber == temp.PortNumber)
                    {
                        WriteLog(3, "", "Port- PortNumber重複");
                        return false;
                    }
                    else if (config.PortList[i].SocketName == temp.SocketName)
                    {
                        WriteLog(3, "", "Port- TagName重複");
                        return false;
                    }
                }

                config.PortList.Add(temp);
                config.AllPort.Add(temp.SocketName, temp);

                return true;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        private bool ReadXML()
        {
            try
            {
                string localPath = @"D:\MecanumConfigs\MIPCControl";

                string path = Path.Combine(localPath, "MIPCConfig.xml");
                config = new MIPCConfig();

                if (path == null || path == "")
                {
                    WriteLog(1, "", "MIPCConfig 路徑錯誤為null或空值,請檢查程式內部的string.");
                    return false;
                }
                else if (!File.Exists(path))
                {
                    WriteLog(1, "", "找不到MIPCConfig.xml.");
                    return false;
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlElement rootNode = doc.DocumentElement;

                string locatePath = new DirectoryInfo(path).Parent.FullName;

                foreach (XmlNode item in rootNode.ChildNodes)
                {
                    switch (item.Name)
                    {
                        case "IP":
                            config.IP = item.InnerText;
                            break;
                        case "PollingInterval":
                            config.PollingInterval = Int32.Parse(item.InnerText);
                            break;
                        case "SocketTimeoutValue":
                            config.SocketTimeoutValue = Int32.Parse(item.InnerText);
                            break;
                        case "CommandTimeoutValue":
                            config.CommandTimeoutValue = Int32.Parse(item.InnerText);
                            break;
                        case "SafetySensorUpdateInterval":
                            config.SafetySensorUpdateInterval = Int32.Parse(item.InnerText);
                            break;
                        case "PollingGroupInterval":
                            ReadPollingGroupIntervalXML((XmlElement)item);
                            break;
                        case "HeartbeatInterval":
                            config.HeartbeatInterval = Int32.Parse(item.InnerText);
                            break;
                        case "MIPCDataConfigPath":
                            config.MIPCDataConfigPath = Path.Combine(localPath, localData.MainFlowConfig.AGVType.ToString(), item.InnerText);
                            break;
                        case "SafetySensorConfigPath":
                            config.SafetySensorConfigPath = Path.Combine(localPath, localData.MainFlowConfig.AGVType.ToString(), item.InnerText);
                            break;
                        case "Port":
                            if (!ReadPortXML((XmlElement)item))
                                return false;
                            break;
                        case "LogMode":
                            config.LogMode = Boolean.Parse(item.InnerText);
                            break;
                        default:
                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        private void ReadPollingGroupIntervalXML(XmlElement element)
        {
            int pollingGroup;
            int interval;

            foreach (XmlNode item in element.ChildNodes)
            {
                if (Int32.TryParse(Regex.Replace(item.Name, "[^0-9]", ""), out pollingGroup) && Int32.TryParse(item.InnerText, out interval))
                {
                    if (config.PollingGroupInterval.ContainsKey(pollingGroup))
                        WriteLog(3, "", String.Concat("Group : ", item.Name, " 重複"));
                    else if (interval < 0)
                        WriteLog(3, "", String.Concat("Group : ", item.Name, " Interval < 0 : ", item.InnerText));
                    else
                        config.PollingGroupInterval.Add(pollingGroup, interval);
                }
                else
                    WriteLog(3, "", String.Concat("Group : ", item.Name, " Interval : ", item.InnerText, " 非數字"));
            }
        }

        private bool ReadConfig(string path)
        {
            if (path == null || path == "")
            {
                WriteLog(3, "", "MIPCDataConfig 路徑錯誤為null或空值.");
                return false;
            }
            else if (!File.Exists(path))
            {
                WriteLog(3, "", "找不到 MIPCDataConfig.csv.");
                return false;
            }

            try
            {
                string[] allRows = File.ReadAllLines(path);
                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                Dictionary<string, int> dicHeaderIndexes = new Dictionary<string, int>();

                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();

                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        if (dicHeaderIndexes.ContainsKey(keyword))
                            WriteLog(3, "", String.Concat("Title repeat : ", keyword));
                        else
                            dicHeaderIndexes.Add(keyword, i);
                    }
                }

                if (dicHeaderIndexes.ContainsKey("DataName") && dicHeaderIndexes.ContainsKey("Address") &&
                    dicHeaderIndexes.ContainsKey("ByteNumber") && dicHeaderIndexes.ContainsKey("BitNumber") &&
                    dicHeaderIndexes.ContainsKey("DataType") && dicHeaderIndexes.ContainsKey("IoStatus") &&
                    dicHeaderIndexes.ContainsKey("IPCName") && dicHeaderIndexes.ContainsKey("Classification"))
                    ;
                else
                {
                    WriteLog(3, "", String.Concat("Title must have : DataName,Address,ByteNumber,BitNumber,DataType,IoStatus,IPCName,Classification"));
                    return false;
                }


                MIPCData temp;
                EnumDataType dataType;
                EnumIOType ioStatus;

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');

                    if (getThisRow.Length != dicHeaderIndexes.Count)
                        WriteLog(3, "", String.Concat("line : ", (i + 2).ToString(), " 和Title數量不吻合"));
                    else
                    {
                        temp = new MIPCData();

                        temp.DataName = getThisRow[dicHeaderIndexes["DataName"]];
                        temp.Address = UInt32.Parse(getThisRow[dicHeaderIndexes["Address"]]);
                        temp.ByteNumber = UInt32.Parse(getThisRow[dicHeaderIndexes["ByteNumber"]]);
                        temp.BitNumber = UInt32.Parse(getThisRow[dicHeaderIndexes["BitNumber"]]);
                        temp.IPCName = getThisRow[dicHeaderIndexes["IPCName"]];
                        temp.Classification = getThisRow[dicHeaderIndexes["Classification"]];

                        int group;
                        if (Int32.TryParse(getThisRow[dicHeaderIndexes["PollingGroup"]], out group))
                            temp.PollingGroup = group;
                        else
                            temp.PollingGroup = -1;

                        if (dicHeaderIndexes.ContainsKey("Description"))
                            temp.Description = getThisRow[dicHeaderIndexes["Description"]];

                        if (allDataByMIPCTagName.ContainsKey(temp.DataName))
                            WriteLog(3, "", String.Concat("line : ", (i + 2).ToString(), ", DataName 重複 : ", temp.DataName));
                        else if (allDataByIPCTagName.ContainsKey(temp.IPCName))
                            WriteLog(3, "", String.Concat("line : ", (i + 2).ToString(), ", IPCName 重複 : ", temp.IPCName));
                        else if (temp.Address < 0 || temp.ByteNumber < 0 || temp.BitNumber < 0)
                            WriteLog(3, "", String.Concat("line : ", (i + 2).ToString(), ", Int屬性錯誤 < 0"));
                        else if (!Enum.TryParse(getThisRow[dicHeaderIndexes["DataType"]], out dataType))
                            WriteLog(3, "", String.Concat("line : ", (i + 2).ToString(), ", DataType 錯誤 : ", getThisRow[dicHeaderIndexes["DataType"]]));
                        else if (!Enum.TryParse(getThisRow[dicHeaderIndexes["IoStatus"]], out ioStatus))
                            WriteLog(3, "", String.Concat("line : ", (i + 2).ToString(), ", ioType 錯誤 : ", getThisRow[dicHeaderIndexes["IoStatus"]]));
                        else
                        {
                            temp.DataType = dataType;

                            switch (dataType)
                            {
                                case EnumDataType.Int32:
                                case EnumDataType.UInt32:
                                case EnumDataType.Double_1:
                                case EnumDataType.Float:
                                    temp.Length = 32;
                                    break;
                                case EnumDataType.Boolean:
                                    temp.Length = 1;
                                    break;
                                default:
                                    temp.Length = 32;
                                    break;
                            }

                            temp.IoStatus = ioStatus;

                            allDataByMIPCTagName.Add(temp.DataName, temp);

                            if (temp.IPCName != "")
                                allDataByIPCTagName.Add(temp.IPCName, temp);

                            if (!AllDataByClassification.ContainsKey(temp.Classification))
                                AllDataByClassification.Add(temp.Classification, new List<MIPCData>());

                            AllDataByClassification[temp.Classification].Add(temp);

                            if (!pollingGroup.ContainsKey(temp.PollingGroup))
                            {
                                pollingGroup.Add(temp.PollingGroup, new List<MIPCData>());
                                allPollingData.Add(temp.PollingGroup, new MIPCPollingData());
                            }

                            allPollingData[temp.PollingGroup].UpdateData(temp);

                            pollingGroup[temp.PollingGroup].Add(temp);
                        }
                    }
                }

                localData.MIPCData.AllDataByIPCTagName = allDataByIPCTagName;
                localData.MIPCData.AllDataByMIPCTagName = allDataByMIPCTagName;

                return true;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        private bool SetPollingData()
        {
            try
            {
                pollingDataList = new List<MIPCPollingData>();
                pollingIntervalList = new List<int>();

                bool first = true;

                foreach (MIPCPollingData pollingData in allPollingData.Values)
                {
                    pollingData.InitialPollingData();

                    if (pollingData.StartAddress > pollingData.EndAddress)
                        WriteLog(1, "", String.Concat("Fatal :: Group = ", pollingData.GroupNumber.ToString("0"), ", startAddress > endAddress"));

                    if (pollingData.GroupNumber > 0)
                    {
                        pollingDataList.Add(pollingData);

                        if (config.PollingGroupInterval.ContainsKey(pollingData.GroupNumber))
                            pollingIntervalList.Add(config.PollingGroupInterval[pollingData.GroupNumber]);
                        else
                            pollingIntervalList.Add(config.PollingInterval);
                    }
                }

                //UInt32 minStartAddress = 0;
                UInt32 maxEndAddress = 0;
                first = true;

                foreach (MIPCPollingData pollingData in allPollingData.Values)
                {
                    if (first)
                    {
                        first = false;
                        dataStartAddress = pollingData.StartAddress;
                        maxEndAddress = pollingData.EndAddress;
                    }
                    else
                    {
                        if (pollingData.StartAddress < dataStartAddress)
                            dataStartAddress = pollingData.StartAddress;

                        if (pollingData.EndAddress > maxEndAddress)
                            maxEndAddress = pollingData.EndAddress;
                    }
                }

                allData = new byte[(maxEndAddress - dataStartAddress + 1) * 4];

                for (int i = 0; i < pollingDataList.Count; i++)
                {
                    for (int j = i + 1; j < pollingDataList.Count; j++)
                    {
                        if (pollingDataList[i].StartAddress > pollingDataList[j].EndAddress ||
                            pollingDataList[i].EndAddress < pollingDataList[j].StartAddress)
                            ;
                        else
                        {
                            WriteLog(1, "", String.Concat("Address start end 干涉 : ", pollingDataList[i].GroupNumber.ToString("0"), " and ", pollingDataList[j].GroupNumber.ToString("0")));
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        private void SetCommandList()
        {
            foreach (EnumMecanumIPCdefaultTag item in (EnumMecanumIPCdefaultTag[])Enum.GetValues(typeof(EnumMecanumIPCdefaultTag)))
            {
                allIPCTageName.Add(item.ToString(), true);
            }

            #region Command-Move.
            bool allFind = true;

            for (int i = 0; i < moveCommandTagNameList.Count && allFind; i++)
            {
                if (!allDataByIPCTagName.ContainsKey(moveCommandTagNameList[i]))
                    allFind = false;
            }

            if (allFind)
            {
                moveCommandAddressArray = new UInt32[moveCommandTagNameList.Count];

                for (int i = 0; i < moveCommandTagNameList.Count; i++)
                    moveCommandAddressArray[i] = allDataByIPCTagName[moveCommandTagNameList[i]].Address;
            }
            #endregion

            #region Command-SetPosition.
            allFind = true;

            for (int i = 0; i < setPositionTagNameList.Count && allFind; i++)
            {
                if (!allDataByIPCTagName.ContainsKey(setPositionTagNameList[i]))
                    allFind = false;
            }

            if (allFind)
            {
                setPositionAddressArray = new UInt32[setPositionTagNameList.Count];

                for (int i = 0; i < setPositionTagNameList.Count; i++)
                    setPositionAddressArray[i] = allDataByIPCTagName[setPositionTagNameList[i]].Address;
            }
            #endregion

            #region changeEnd.
            allFind = true;

            for (int i = 0; i < changeEndTagNameList.Count && allFind; i++)
            {
                if (!allDataByIPCTagName.ContainsKey(changeEndTagNameList[i]))
                    allFind = false;
            }

            if (allFind)
            {
                changeEndAddressArray = new UInt32[changeEndTagNameList.Count];

                for (int i = 0; i < changeEndTagNameList.Count; i++)
                    changeEndAddressArray[i] = allDataByIPCTagName[changeEndTagNameList[i]].Address;
            }
            #endregion

            #region changeVelocity.
            allFind = true;

            for (int i = 0; i < changeVelociyTagNameList.Count && allFind; i++)
            {
                if (!allDataByIPCTagName.ContainsKey(changeVelociyTagNameList[i]))
                    allFind = false;
            }

            if (allFind)
            {
                changeVelociyAddressArray = new UInt32[changeVelociyTagNameList.Count];

                for (int i = 0; i < changeVelociyTagNameList.Count; i++)
                    changeVelociyAddressArray[i] = allDataByIPCTagName[changeVelociyTagNameList[i]].Address;
            }
            #endregion

            #region Command-Turn.
            allFind = true;

            for (int i = 0; i < turnCommandTagNameList.Count && allFind; i++)
            {
                if (!allDataByIPCTagName.ContainsKey(turnCommandTagNameList[i]))
                    allFind = false;
            }

            if (allFind)
            {
                turnCommandAddressArray = new UInt32[turnCommandTagNameList.Count];

                for (int i = 0; i < turnCommandTagNameList.Count; i++)
                    turnCommandAddressArray[i] = allDataByIPCTagName[turnCommandTagNameList[i]].Address;
            }
            #endregion

            #region Command-Stop.
            allFind = true;

            for (int i = 0; i < stopCommandTagNameList.Count && allFind; i++)
            {
                if (!allDataByIPCTagName.ContainsKey(stopCommandTagNameList[i]))
                    allFind = false;
            }

            if (allFind)
            {
                stopCommandAddressArray = new UInt32[stopCommandTagNameList.Count];

                for (int i = 0; i < stopCommandTagNameList.Count; i++)
                    stopCommandAddressArray[i] = allDataByIPCTagName[stopCommandTagNameList[i]].Address;
            }
            #endregion

            #region Command-Move.
            allFind = true;

            for (int i = 0; i < jogjoystickDataList.Count && allFind; i++)
            {
                if (!allDataByIPCTagName.ContainsKey(jogjoystickDataList[i]))
                    allFind = false;
            }

            if (allFind)
            {
                jogjoystickDataArray = new UInt32[jogjoystickDataList.Count];

                for (int i = 0; i < jogjoystickDataList.Count; i++)
                    jogjoystickDataArray[i] = allDataByIPCTagName[jogjoystickDataList[i]].Address;
            }
            #endregion
        }

        private void ReadXMLAndConfig()
        {
            if (ReadXML())
            {
                localData.MIPCData.Config = config;

                if (ReadConfig(config.MIPCDataConfigPath))
                {
                    SetCommandList();

                    if (SetPollingData())
                        Status = EnumControlStatus.Initial;
                }
            }
        }
        #endregion

        #region Initial-Socket,Thread.
        private List<bool> socketListConnect = new List<bool>();

        private void InitialSocketAndThread()
        {
            try
            {
                bool anyError = false;

                for (int i = 0; i < config.PortList.Count; i++)
                {
                    Socket socket;
                    Thread thread;

                    if (!socketListConnect[i])
                    {
                        try
                        {
                            if (allSocket.ContainsKey(config.PortList[i].SocketName))
                            {
                                allSocket[config.PortList[i].SocketName].Dispose();
                            }

                            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 500);
                            socket.Connect(IPAddress.Parse(config.IP), config.PortList[i].PortNumber);

                            if (allSocket.ContainsKey(config.PortList[i].SocketName))
                                allSocket[config.PortList[i].SocketName] = socket;
                            else
                                allSocket.Add(config.PortList[i].SocketName, socket);

                            thread = new Thread(new ParameterizedThreadStart(SocketThread));
                            thread.Start((object)i);

                            if (allSocketThread.ContainsKey(config.PortList[i].SocketName))
                                allSocketThread[config.PortList[i].SocketName] = thread;
                            else
                                allSocketThread.Add(config.PortList[i].SocketName, thread);

                            if (!allReadObject.ContainsKey(config.PortList[i].SocketName))
                                allReadObject.Add(config.PortList[i].SocketName, new object());

                            if (!allWriteObject.ContainsKey(config.PortList[i].SocketName))
                                allWriteObject.Add(config.PortList[i].SocketName, new object());

                            if (allReadQueue.ContainsKey(config.PortList[i].SocketName))
                                allReadQueue[config.PortList[i].SocketName] = new Queue<SendAndReceive>();
                            else
                                allReadQueue.Add(config.PortList[i].SocketName, new Queue<SendAndReceive>());

                            if (allWriteQueue.ContainsKey(config.PortList[i].SocketName))
                                allWriteQueue[config.PortList[i].SocketName] = new Queue<SendAndReceive>();
                            else
                                allWriteQueue.Add(config.PortList[i].SocketName, new Queue<SendAndReceive>());

                            socketListConnect[i] = true;
                        }
                        catch (Exception ex)
                        {
                            anyError = true;
                            WriteLog(3, "", String.Concat("socket, Name =  ", config.PortList[i].SocketName, ", port = ", config.PortList[i].PortNumber.ToString("0"), " Connect Exception : ", ex.ToString()));
                        }
                    }
                }

                if (!anyError)
                {
                    communicationErrorCount = 0;
                    pollingThread = new Thread(PollingThread);
                    pollingThread.Start();

                    batteryCSVThread = new Thread(BatteryCSVThread);
                    batteryCSVThread.Start();

                    Status = EnumControlStatus.Ready;


                    SendMIPCDataByIPCTagName(new List<EnumMecanumIPCdefaultTag>() { EnumMecanumIPCdefaultTag.AGV_Type }, new List<float>() { (float)localData.MainFlowConfig.AGVType });
                    SendMIPCDataByIPCTagName(new List<EnumMecanumIPCdefaultTag>() { EnumMecanumIPCdefaultTag.Light_Green }, new List<float>() { 1 });
                }
                else
                    SendAlarmCode(EnumMIPCControlErrorCode.MIPC連線失敗);
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }
        #endregion

        #region Enqueue/Dequeue.
        private SendAndReceive GetDataFromWriteQueue(string socketName)
        {
            try
            {
                lock (allWriteObject[socketName])
                {
                    if (allWriteQueue[socketName].Count > 0)
                    {

                        LogFormat logFormat = new LogFormat(mipcDebug, "7", "", device, "", String.Concat("Dec Write, Count : ", allWriteQueue[socketName].Count.ToString()));
                        loggerAgent.Log(logFormat.Category, logFormat);

                        SendAndReceive a = allWriteQueue[socketName].Dequeue();

                        if (a.IsHearBeat)
                        {
                            logFormat = new LogFormat(mipcDebug, "7", "", device, "", String.Concat("HeartBeat AddQueue~Dequeue time : ", (DateTime.Now - a.AddTime).TotalMilliseconds.ToString()));
                            loggerAgent.Log(logFormat.Category, logFormat);
                        }

                        return a;

                        //return allWriteQueue[socketName].Dequeue();
                    }
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return null;
            }
        }

        private SendAndReceive GetDataFromReadQueue(string socketName)
        {
            try
            {
                lock (allReadObject[socketName])
                {
                    if (allReadQueue[socketName].Count > 0)
                    {

                        LogFormat logFormat = new LogFormat(mipcDebug, "7", "", device, "", String.Concat("Dec Read, Count : ", allReadQueue[socketName].Count.ToString()));
                        loggerAgent.Log(logFormat.Category, logFormat);

                        return allReadQueue[socketName].Dequeue();
                    }
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return null;
            }
        }

        private void AddReaeQueue(string socketName, SendAndReceive data, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                LogFormat logFormat = new LogFormat(mipcDebug, "7", "", device, memberName, "Add Read");
                loggerAgent.Log(logFormat.Category, logFormat);

                lock (allReadObject[socketName])
                {
                    allReadQueue[socketName].Enqueue(data);
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        public void AddWriteQueue(string socketName, SendAndReceive data, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                LogFormat logFormat = new LogFormat(mipcDebug, "7", "", device, memberName, "Add Write");
                loggerAgent.Log(logFormat.Category, logFormat);

                data.AddTime = DateTime.Now;

                lock (allWriteObject[socketName])
                {
                    allWriteQueue[socketName].Enqueue(data);
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }
        #endregion

        private void ResetConnectThread()
        {
            WriteLog(7, "", "ResetConnectThread Start");
            InitialSocketAndThread();
            resetAlarm = false;
        }

        public void ResetAlarm()
        {
            SafetySensorControl.ResetAlarm();
            localData.MIPCData.MotionAlarmCount = 0;
            mipcAlarm = new Dictionary<EnumMIPCControlErrorCode, bool>();

            switch (Status)
            {
                case EnumControlStatus.NotInitial:
                    SendAlarmCode(EnumMIPCControlErrorCode.MIPC初始化失敗);
                    break;

                case EnumControlStatus.Initial:
                    resetAlarm = true;
                    resetAlarmThread = new Thread(ResetConnectThread);
                    resetAlarmThread.Start();
                    break;

                case EnumControlStatus.Error:
                    resetAlarm = true;
                    resetAlarmThread = new Thread(ResetConnectThread);
                    resetAlarmThread.Start();
                    break;

                default:
                    break;
            }
        }

        private bool SendAndReceiveBySocket(Socket socket, SendAndReceive data)
        {
            try
            {
                if (config.LogMode)
                    WriteSocketLog(7, "Send", String.Concat("Seq[", data.Send.SeqNumber.ToString("0"), "]StaNum[", data.Send.StationNo.ToString("0"),
                                                            "]FunCode[", data.Send.FunctionCode.ToString("0"), "]Len[", data.Send.DataLength.ToString("0"),
                                                            "]Address[", data.Send.StartAddress.ToString("0"), "]\r\n", BitConverter.ToString(data.Send.ByteData)));

                Byte[] recieveData = new byte[data.Send.RecieveLength];

                Stopwatch sendReceiveTimer = new Stopwatch();
                sendReceiveTimer.Restart();

                socket.Send(data.Send.ByteData, 0, data.Send.ByteData.GetLength(0), SocketFlags.None);
                data.Time = DateTime.Now;

                socket.ReceiveTimeout = config.SocketTimeoutValue;
                socket.Receive(recieveData, 0, data.Send.RecieveLength, SocketFlags.None);

                sendReceiveTimer.Stop();
                data.ScanTime = sendReceiveTimer.ElapsedMilliseconds / 2;

                if (config.LogMode)
                    WriteSocketLog(7, "Receive", BitConverter.ToString(recieveData));

                LogFormat logFormat = new LogFormat(mipcDebug, "7", "", device, "", String.Concat("Timer : ", sendReceiveTimer.ElapsedMilliseconds.ToString()));
                loggerAgent.Log(logFormat.Category, logFormat);

                data.Receive = new ModbusData();
                data.Receive.ByteData = recieveData;
                return true;
            }
            catch (Exception ex)
            {
                data.Result = EnumSendAndRecieve.Error;
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        private bool ProcessRecieveData(SendAndReceive data)
        {
            try
            {
                data.Receive.GetDataByByteData();

                if (data.Send.SeqNumber != data.Receive.SeqNumber ||
                    data.Send.StationNo != data.Receive.StationNo ||
                    data.Send.FunctionCode != data.Receive.FunctionCode)
                {
                    WriteLog(3, "", "SeqNumber/StationNo/FunctionCode send and recieve不符合");
                    data.Result = EnumSendAndRecieve.Error;
                    return false;
                }
                else if (data.Receive.StartAddress != 0)
                {
                    WriteLog(3, "", "Recieve return code (Address) != 0");
                    data.Result = EnumSendAndRecieve.Error;
                    return false;
                }

                switch (data.Send.FunctionCode)
                {
                    case 0x01:
                        // 連續.
                        for (int i = 0; i < data.Receive.DataLength; i++)
                        {
                            for (int j = 0; j < 4; j++)
                                allData[(data.Send.StartAddress - dataStartAddress + i) * 4 + 3 - j] = data.Receive.ByteData[12 + i * 4 + j];
                        }

                        data.Result = EnumSendAndRecieve.OK;
                        break;

                    case 0x03:
                        for (int i = 0; i < data.Receive.DataLength; i++)
                        {
                            for (int j = 0; j < 4; j++)
                                allData[(BitConverter.ToUInt32(data.Send.DataBuffer, i * 4) - dataStartAddress) * 4 + 3 - j] = data.Receive.ByteData[12 + i * 4 + j];
                        }

                        data.Result = EnumSendAndRecieve.OK;
                        break;

                    case 0x81:
                    case 0x83:
                        data.Result = EnumSendAndRecieve.OK;

                        break;
                    default:
                        WriteLog(3, "", String.Concat("RecieveData Function Code : ", data.Receive.FunctionCode.ToString("0"), ", return false"));
                        data.Result = EnumSendAndRecieve.Error;
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        private void GetDataValueByGroup(int index, DateTime getDataTime, double scanTime)
        {
            try
            {
                MIPCData temp;
                EnumMecanumIPCdefaultTag enumTagName;

                for (int i = 0; i < pollingGroup[index].Count; i++)
                {
                    temp = pollingGroup[index][i];

                    switch (temp.DataType)
                    {
                        case EnumDataType.Boolean:
                            /// addres -> one bool.
                            byte[] boolDataAarray = new byte[4] { allData[(temp.Address - dataStartAddress) * 4 + 3],
                                                                  allData[(temp.Address - dataStartAddress) * 4 + 2],
                                                                  allData[(temp.Address - dataStartAddress) * 4 + 1],
                                                                  allData[(temp.Address - dataStartAddress) * 4 + 0]};

                            UInt32 boolValue = BitConverter.ToUInt32(boolDataAarray, 0);

                            if (boolValue == 0)
                            {
                                temp.Object = (object)((uint)0);
                                temp.Value = "0";
                            }
                            else
                            {
                                temp.Object = (object)((uint)1);
                                temp.Value = "1";
                            }

                            break;
                        case EnumDataType.UInt32:
                            byte[] uint32DataAarray = new byte[4] { allData[(temp.Address - dataStartAddress) * 4 + 3],
                                                                    allData[(temp.Address - dataStartAddress) * 4 + 2],
                                                                    allData[(temp.Address - dataStartAddress) * 4 + 1],
                                                                    allData[(temp.Address - dataStartAddress) * 4 + 0]};

                            UInt32 uint32Value = BitConverter.ToUInt32(uint32DataAarray, 0);

                            temp.Object = (object)uint32Value;
                            temp.Value = uint32Value.ToString("0");
                            break;
                        case EnumDataType.Int32:
                            byte[] int32DataAarray = new byte[4] { allData[(temp.Address - dataStartAddress) * 4 + 3],
                                                                   allData[(temp.Address - dataStartAddress) * 4 + 2],
                                                                   allData[(temp.Address - dataStartAddress) * 4 + 1],
                                                                   allData[(temp.Address - dataStartAddress) * 4 + 0]};

                            Int32 int32Value = BitConverter.ToInt32(int32DataAarray, 0);

                            temp.Object = (object)int32Value;
                            temp.Value = int32Value.ToString("0");
                            break;
                        case EnumDataType.Double_1:
                            byte[] double_1DataAarray = new byte[4] { allData[(temp.Address - dataStartAddress) * 4 + 3],
                                                                      allData[(temp.Address - dataStartAddress) * 4 + 2],
                                                                      allData[(temp.Address - dataStartAddress) * 4 + 1],
                                                                      allData[(temp.Address - dataStartAddress) * 4 + 0]};

                            double double_1Value = BitConverter.ToInt32(double_1DataAarray, 0) / 10;

                            temp.Object = (object)double_1Value;
                            temp.Value = double_1Value.ToString("0.0");
                            break;

                        case EnumDataType.Float:
                            byte[] double_DataAarray = new byte[4] { allData[(temp.Address - dataStartAddress) * 4 + 3],
                                                                     allData[(temp.Address - dataStartAddress) * 4 + 2],
                                                                     allData[(temp.Address - dataStartAddress) * 4 + 1],
                                                                     allData[(temp.Address - dataStartAddress) * 4 + 0]};

                            float double_Value = BitConverter.ToSingle(double_DataAarray, 0);
                            temp.LastObject = temp.Object;
                            temp.Object = (object)double_Value;
                            temp.Value = double_Value.ToString("0.0");

                            break;
                        default:
                            byte[] defaultArray = new byte[4] { allData[(temp.Address - dataStartAddress) * 4 + 3],
                                                                  allData[(temp.Address - dataStartAddress) * 4 + 2],
                                                                  allData[(temp.Address - dataStartAddress) * 4 + 1],
                                                                  allData[(temp.Address - dataStartAddress) * 4 + 0]};

                            float defaultFloat = BitConverter.ToSingle(defaultArray, 0);

                            temp.Object = (object)defaultFloat;
                            temp.Value = defaultFloat.ToString("0.0");
                            break;
                    }

                    try
                    {
                        if (allIPCTageName.ContainsKey(temp.IPCName) && Enum.TryParse(temp.IPCName, out enumTagName))
                        {
                            switch (enumTagName)
                            {
                                case EnumMecanumIPCdefaultTag.Feedback_Theta:
                                    LocateAGVPosition newAGVPosition = new LocateAGVPosition();
                                    newAGVPosition.GetDataTime = getDataTime;
                                    newAGVPosition.ScanTime = scanTime;

                                    //newAGVPosition.AGVPosition.Angle = computeFunction.GetCurrectAngle(-((float)(temp.Object) + 90));
                                    newAGVPosition.AGVPosition.Angle = computeFunction.GetCurrectAngle((float)(temp.Object));

                                    if (allIPCTageName.ContainsKey(EnumMecanumIPCdefaultTag.Feedback_X.ToString()))
                                        newAGVPosition.AGVPosition.Position.Y = (float)(allDataByIPCTagName[EnumMecanumIPCdefaultTag.Feedback_X.ToString()].Object);

                                    if (allIPCTageName.ContainsKey(EnumMecanumIPCdefaultTag.Feedback_Y.ToString()))
                                        newAGVPosition.AGVPosition.Position.X = (float)(allDataByIPCTagName[EnumMecanumIPCdefaultTag.Feedback_Y.ToString()].Object);

                                    localData.MoveControlData.MotionControlData.EncoderAGVPosition = newAGVPosition;

                                    break;
                                case EnumMecanumIPCdefaultTag.Feedback_MoveStatus:
                                    EnumAxisMoveStatus tempMoveStatus = (((float)(temp.Object) == 1) ? EnumAxisMoveStatus.Move : EnumAxisMoveStatus.Stop);

                                    if (tempMoveStatus != localData.MoveControlData.MotionControlData.moveStatus)
                                        WriteLog(7, "", String.Concat("MoveStatus Change : ", tempMoveStatus.ToString()));

                                    localData.MoveControlData.MotionControlData.MoveStatus = tempMoveStatus;
                                    //localData.MoveControlData.MotionControlData.MoveStatus = (((float)(temp.Object) == 1) ? EnumAxisMoveStatus.Move : EnumAxisMoveStatus.Stop);
                                    break;

                                case EnumMecanumIPCdefaultTag.Feedback_線速度:
                                    localData.MoveControlData.MotionControlData.LineVelocity = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.Feedback_線速度方向:
                                    localData.MoveControlData.MotionControlData.LineVelocityAngle = computeFunction.GetCurrectAngle((float)(temp.Object));
                                    break;
                                case EnumMecanumIPCdefaultTag.Feedback_線加速度:
                                    localData.MoveControlData.MotionControlData.LineAcc = computeFunction.GetCurrectAngle((float)(temp.Object));
                                    break;
                                case EnumMecanumIPCdefaultTag.Feedback_線減速度:
                                    localData.MoveControlData.MotionControlData.LineDec = computeFunction.GetCurrectAngle((float)(temp.Object));
                                    break;
                                case EnumMecanumIPCdefaultTag.Feedback_線急跳度:
                                    localData.MoveControlData.MotionControlData.LineJerk = computeFunction.GetCurrectAngle((float)(temp.Object));
                                    break;
                                case EnumMecanumIPCdefaultTag.Feedback_角速度:
                                    localData.MoveControlData.MotionControlData.ThetaVelocity = computeFunction.GetCurrectAngle((float)(temp.Object));
                                    break;
                                case EnumMecanumIPCdefaultTag.Feedback_角加速度:
                                    localData.MoveControlData.MotionControlData.ThetaAcc = computeFunction.GetCurrectAngle((float)(temp.Object));
                                    break;
                                case EnumMecanumIPCdefaultTag.Feedback_角減速度:
                                    localData.MoveControlData.MotionControlData.ThetaDec = computeFunction.GetCurrectAngle((float)(temp.Object));
                                    break;
                                case EnumMecanumIPCdefaultTag.Feedback_角急跳度:
                                    localData.MoveControlData.MotionControlData.ThetaJerk = computeFunction.GetCurrectAngle((float)(temp.Object));
                                    break;
                                case EnumMecanumIPCdefaultTag.Feedback_TimeStamp:
                                    TimeStampData nowTimeStamp = timeStampData;

                                    if (nowTimeStamp == null || nowTimeStamp.Time.Day != getDataTime.Day)
                                    {
                                        TimeStampData newTimeStampData = new TimeStampData();
                                        newTimeStampData.Time = getDataTime.AddMilliseconds(-scanTime - (double)((float)(temp.Object)));
                                        newTimeStampData.GetTime = newTimeStampData.Time;
                                        newTimeStampData.SendTime = newTimeStampData.Time;

                                        if (nowTimeStamp == null)
                                            WriteLog(7, "", String.Concat("newTimeStamp : ", newTimeStampData.Time.ToString("HH:mm:ss.fff")));
                                        else
                                            WriteLog(7, "", String.Concat("newTimeStamp : ", newTimeStampData.Time.ToString("HH:mm:ss.fff"),
                                                                          ", delta time : ", (nowTimeStamp.GetTime - newTimeStampData.GetTime).TotalMilliseconds), " ms");

                                        timeStampData = newTimeStampData;
                                    }

                                    break;
                                case EnumMecanumIPCdefaultTag.XRR_QA:
                                    uint servoOnOffStatus = 0;
                                    AxisFeedbackData tempAxisFeedbackData;
                                    EnumDefaultAxisName axisName;
                                    string tempTag;

                                    #region axisData lsit.
                                    for (int axisIndex = 0; axisIndex < axisList.Count; axisIndex++)
                                    {
                                        tempAxisFeedbackData = new AxisFeedbackData();
                                        axisName = axisList[axisIndex];
                                        tempAxisFeedbackData.GetDataTime = getDataTime;

                                        tempTag = String.Concat(axisName.ToString(), "_", DefaultAxisTag.ServoStatus.ToString());

                                        if (allDataByIPCTagName.ContainsKey(tempTag))
                                            tempAxisFeedbackData.AxisServoOnOff = ((float)(allDataByIPCTagName[tempTag].Object) == 1) ? EnumAxisServoOnOff.ServoOn : EnumAxisServoOnOff.ServoOff;

                                        tempTag = String.Concat(axisName.ToString(), "_", DefaultAxisTag.Encoder.ToString());

                                        if (allDataByIPCTagName.ContainsKey(tempTag))
                                            tempAxisFeedbackData.Position = (float)allDataByIPCTagName[tempTag].Object;

                                        tempTag = String.Concat(axisName.ToString(), "_", DefaultAxisTag.RPM.ToString());

                                        if (allDataByIPCTagName.ContainsKey(tempTag))
                                            tempAxisFeedbackData.Velocity = (float)allDataByIPCTagName[tempTag].Object;

                                        tempTag = String.Concat(axisName.ToString(), "_", DefaultAxisTag.DA.ToString());

                                        if (allDataByIPCTagName.ContainsKey(tempTag))
                                            tempAxisFeedbackData.DA = (float)allDataByIPCTagName[tempTag].Object;

                                        tempTag = String.Concat(axisName.ToString(), "_", DefaultAxisTag.QA.ToString());

                                        if (allDataByIPCTagName.ContainsKey(tempTag))
                                            tempAxisFeedbackData.QA = (float)allDataByIPCTagName[tempTag].Object;

                                        tempTag = String.Concat(axisName.ToString(), "_", DefaultAxisTag.V.ToString());

                                        if (allDataByIPCTagName.ContainsKey(tempTag))
                                            tempAxisFeedbackData.V = (float)allDataByIPCTagName[tempTag].Object;

                                        tempTag = String.Concat(axisName.ToString(), "_", DefaultAxisTag.EC.ToString());

                                        if (allDataByIPCTagName.ContainsKey(tempTag))
                                        {
                                            if (((float)(allDataByIPCTagName[tempTag].Object) != 0))
                                                tempAxisFeedbackData.AxisStatus = EnumAxisStatus.Error;

                                            tempAxisFeedbackData.ErrorCode = String.Concat(tempAxisFeedbackData.ErrorCode, DefaultAxisTag.EC.ToString(), ":", ((float)(allDataByIPCTagName[tempTag].Object)).ToString("0"));
                                        }

                                        tempTag = String.Concat(axisName.ToString(), "_", DefaultAxisTag.MF.ToString());

                                        if (allDataByIPCTagName.ContainsKey(tempTag))
                                        {
                                            if (((float)(allDataByIPCTagName[tempTag].Object) != 0))
                                                tempAxisFeedbackData.AxisStatus = EnumAxisStatus.Error;

                                            tempAxisFeedbackData.ErrorCode = String.Concat(tempAxisFeedbackData.ErrorCode, DefaultAxisTag.MF.ToString(), ":", ((float)(allDataByIPCTagName[tempTag].Object)).ToString("0"));
                                        }

                                        tempTag = String.Concat(axisName.ToString(), "_", DefaultAxisTag.GetwayError.ToString());

                                        if (allDataByIPCTagName.ContainsKey(tempTag) && allDataByIPCTagName[tempTag].Object != null)
                                        {
                                            if (((float)(allDataByIPCTagName[tempTag].Object) != 0))
                                                tempAxisFeedbackData.AxisStatus = EnumAxisStatus.Error;

                                            tempAxisFeedbackData.ErrorCode = String.Concat(tempAxisFeedbackData.ErrorCode, DefaultAxisTag.GetwayError.ToString(), ":", ((float)(allDataByIPCTagName[tempTag].Object)).ToString("0"));
                                        }

                                        servoOnOffStatus = ((servoOnOffStatus << 1) + (uint)(tempAxisFeedbackData.AxisServoOnOff == EnumAxisServoOnOff.ServoOn ? 1 : 0));

                                        if (localData.MoveControlData.MotionControlData.AllAxisFeedbackData.ContainsKey(axisName))
                                        {
                                            if (localData.MoveControlData.MotionControlData.AllAxisFeedbackData[axisName].AxisStatus == EnumAxisStatus.Normal &&
                                                tempAxisFeedbackData.AxisStatus == EnumAxisStatus.Error)
                                            {
                                                WriteLog(3, "", String.Concat(axisName.ToString(), " ErrorStop On!"));
                                                WriteLog(3, "", String.Concat(axisName.ToString(), ", ", tempAxisFeedbackData.ErrorCode));
                                            }

                                            localData.MoveControlData.MotionControlData.AllAxisFeedbackData[axisName] = tempAxisFeedbackData;
                                        }
                                        else
                                        {
                                            if (tempAxisFeedbackData.AxisStatus == EnumAxisStatus.Error)
                                            {
                                                WriteLog(3, "", String.Concat(axisName.ToString(), " ErrorStop On!"));
                                                WriteLog(3, "", String.Concat(axisName.ToString(), ", ", tempAxisFeedbackData.ErrorCode));
                                            }

                                            localData.MoveControlData.MotionControlData.AllAxisFeedbackData.Add(axisName, tempAxisFeedbackData);
                                        }
                                    }
                                    #endregion

                                    localData.MoveControlData.MotionControlData.AllServoStatus = servoOnOffStatus;
                                    break;
                                case EnumMecanumIPCdefaultTag.JoystickOnOff:
                                    localData.MoveControlData.MotionControlData.JoystickMode = ((float)(temp.Object) == 1);
                                    break;
                                case EnumMecanumIPCdefaultTag.Joystick_LineVelocity:
                                    localData.MoveControlData.MotionControlData.JoystickLineAxisData.Velocity = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.Joystick_LineAcc:
                                    localData.MoveControlData.MotionControlData.JoystickLineAxisData.Acceleration = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.Joystick_LineDec:
                                    localData.MoveControlData.MotionControlData.JoystickLineAxisData.Deceleration = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.Joystick_ThetaVelocity:
                                    localData.MoveControlData.MotionControlData.JoystickThetaAxisData.Velocity = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.Joystick_ThetaAcc:
                                    localData.MoveControlData.MotionControlData.JoystickThetaAxisData.Acceleration = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.Joystick_ThetaDec:
                                    localData.MoveControlData.MotionControlData.JoystickThetaAxisData.Deceleration = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.MIPC_Test1:
                                    localData.MIPCData.MIPCTestArray[0] = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.MIPC_Test2:
                                    localData.MIPCData.MIPCTestArray[1] = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.MIPC_Test3:
                                    localData.MIPCData.MIPCTestArray[2] = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.MIPC_Test4:
                                    localData.MIPCData.MIPCTestArray[3] = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.MIPC_Test5:
                                    localData.MIPCData.MIPCTestArray[4] = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.MIPC_Test6:
                                    localData.MIPCData.MIPCTestArray[5] = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.MIPC_Test7:
                                    localData.MIPCData.MIPCTestArray[6] = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.MIPC_Test8:
                                    localData.MIPCData.MIPCTestArray[7] = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.MIPC_Test9:
                                    localData.MIPCData.MIPCTestArray[8] = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.MIPC_Test10:
                                    localData.MIPCData.MIPCTestArray[9] = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.Battery_SOC:
                                    localData.BatteryInfo.SOC = (float)(temp.Object);
                                    if (localData.BatteryInfo.SOC < config.SOCLowBattery && !mipcAlarm.ContainsKey(EnumMIPCControlErrorCode.LowBattery))
                                    {
                                        SendAlarmCode(EnumMIPCControlErrorCode.LowBattery);
                                        mipcAlarm.Add(EnumMIPCControlErrorCode.LowBattery, true);
                                    }

                                    break;
                                case EnumMecanumIPCdefaultTag.Battery_V:
                                    localData.BatteryInfo.V = (float)(temp.Object);
                                    break;
                                case EnumMecanumIPCdefaultTag.MIPCAlarmCode_1:
                                    if ((float)(temp.Object) == 1 && !mipcAlarm.ContainsKey(EnumMIPCControlErrorCode.MIPC_DeviceHeartBeatLoss))
                                    {
                                        SendAlarmCode(EnumMIPCControlErrorCode.MIPC_DeviceHeartBeatLoss);
                                        mipcAlarm.Add(EnumMIPCControlErrorCode.MIPC_DeviceHeartBeatLoss, true);
                                        localData.MIPCData.MotionAlarmCount++;
                                    }

                                    break;
                                case EnumMecanumIPCdefaultTag.MIPCAlarmCode_2:
                                    if ((float)(temp.Object) == 1 && !mipcAlarm.ContainsKey(EnumMIPCControlErrorCode.MIPC_IPCHeartBeatLoss))
                                    {
                                        SendAlarmCode(EnumMIPCControlErrorCode.MIPC_IPCHeartBeatLoss);
                                        mipcAlarm.Add(EnumMIPCControlErrorCode.MIPC_IPCHeartBeatLoss, true);
                                        localData.MIPCData.MotionAlarmCount++;
                                    }

                                    break;
                                case EnumMecanumIPCdefaultTag.MIPCAlarmCode_3:
                                    if ((float)(temp.Object) == 1 && !mipcAlarm.ContainsKey(EnumMIPCControlErrorCode.MIPC_Motin_EMO))
                                    {
                                        SendAlarmCode(EnumMIPCControlErrorCode.MIPC_Motin_EMO);
                                        mipcAlarm.Add(EnumMIPCControlErrorCode.MIPC_Motin_EMO, true);
                                        localData.MIPCData.MotionAlarmCount++;
                                    }

                                    break;
                                case EnumMecanumIPCdefaultTag.MIPCAlarmCode_4:
                                    if ((float)(temp.Object) == 1 && !mipcAlarm.ContainsKey(EnumMIPCControlErrorCode.MIPC_誤差過大))
                                    {
                                        SendAlarmCode(EnumMIPCControlErrorCode.MIPC_誤差過大);
                                        mipcAlarm.Add(EnumMIPCControlErrorCode.MIPC_誤差過大, true);
                                        localData.MIPCData.MotionAlarmCount++;
                                    }

                                    break;
                                case EnumMecanumIPCdefaultTag.MIPCAlarmCode_5:
                                    if ((float)(temp.Object) == 1 && !mipcAlarm.ContainsKey(EnumMIPCControlErrorCode.MIPC_SLAM過久沒更新))
                                    {
                                        SendAlarmCode(EnumMIPCControlErrorCode.MIPC_SLAM過久沒更新);
                                        mipcAlarm.Add(EnumMIPCControlErrorCode.MIPC_SLAM過久沒更新, true);
                                    }

                                    break;
                                case EnumMecanumIPCdefaultTag.MIPCAlarmCode_6:
                                    if ((float)(temp.Object) == 1 && !mipcAlarm.ContainsKey(EnumMIPCControlErrorCode.MIPC_Alarm6))
                                    {
                                        SendAlarmCode(EnumMIPCControlErrorCode.MIPC_Alarm6);
                                        mipcAlarm.Add(EnumMIPCControlErrorCode.MIPC_Alarm6, true);
                                    }

                                    break;
                                case EnumMecanumIPCdefaultTag.MIPCAlarmCode_7:
                                    if ((float)(temp.Object) == 1 && !mipcAlarm.ContainsKey(EnumMIPCControlErrorCode.MIPC_Alarm7))
                                    {
                                        SendAlarmCode(EnumMIPCControlErrorCode.MIPC_Alarm7);
                                        mipcAlarm.Add(EnumMIPCControlErrorCode.MIPC_Alarm7, true);
                                    }

                                    break;
                                case EnumMecanumIPCdefaultTag.MIPCAlarmCode_8:
                                    if ((float)(temp.Object) == 1 && !mipcAlarm.ContainsKey(EnumMIPCControlErrorCode.MIPC_Alarm8))
                                    {
                                        SendAlarmCode(EnumMIPCControlErrorCode.MIPC_Alarm8);
                                        mipcAlarm.Add(EnumMIPCControlErrorCode.MIPC_Alarm8, true);
                                    }

                                    break;
                                case EnumMecanumIPCdefaultTag.MIPCAlarmCode_9:
                                    if ((float)(temp.Object) == 1 && !mipcAlarm.ContainsKey(EnumMIPCControlErrorCode.MIPC_Alarm9))
                                    {
                                        SendAlarmCode(EnumMIPCControlErrorCode.MIPC_Alarm9);
                                        mipcAlarm.Add(EnumMIPCControlErrorCode.MIPC_Alarm9, true);
                                    }

                                    break;
                                case EnumMecanumIPCdefaultTag.MIPCAlarmCode_10:
                                    if ((float)(temp.Object) == 1 && !mipcAlarm.ContainsKey(EnumMIPCControlErrorCode.MIPC_Alarm10))
                                    {
                                        SendAlarmCode(EnumMIPCControlErrorCode.MIPC_Alarm10);
                                        mipcAlarm.Add(EnumMIPCControlErrorCode.MIPC_Alarm10, true);
                                    }

                                    break;

                                case EnumMecanumIPCdefaultTag.SafetyRelay:
                                    if ((float)(temp.Object) == 0 && !mipcAlarm.ContainsKey(EnumMIPCControlErrorCode.SafetyRelayNotOK))
                                    {
                                        SendAlarmCode(EnumMIPCControlErrorCode.SafetyRelayNotOK);
                                        mipcAlarm.Add(EnumMIPCControlErrorCode.SafetyRelayNotOK, true);
                                    }

                                    break;
                                case EnumMecanumIPCdefaultTag.Reset_Front:
                                case EnumMecanumIPCdefaultTag.Reset_Back:

                                    if ((float)temp.Object == 1 && (temp.LastObject == null || (float)temp.LastObject == 0))
                                        main.ResetAlarm();
                                    break;
                                case EnumMecanumIPCdefaultTag.Start_Front:
                                    localData.MIPCData.StartButton_Front = ((float)temp.Object == 1);
                                    break;
                                case EnumMecanumIPCdefaultTag.Start_Back:
                                    localData.MIPCData.StartButton_Back = ((float)temp.Object == 1);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(7, "", String.Concat("Case : ", temp.IPCName, ", Exception : ", ex.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        private void SocketThread(object index)
        {
            try
            {
                string socketName = config.PortList[(int)index].SocketName;

                Socket socket = allSocket[socketName];
                SendAndReceive sendAndRecieve;

                while (Status != EnumControlStatus.Closing && Status != EnumControlStatus.Error)
                {
                    sendAndRecieve = GetDataFromWriteQueue(socketName);

                    if (sendAndRecieve == null)
                        sendAndRecieve = GetDataFromReadQueue(socketName);

                    if (sendAndRecieve != null)
                    {
                        if (SendAndReceiveBySocket(socket, sendAndRecieve))
                        {
                            if (sendAndRecieve.IsMotionCommand)
                                sendAndRecieve.Result = EnumSendAndRecieve.OK;
                            else
                            {
                                Stopwatch timer = new Stopwatch();
                                timer.Restart();
                                /// task??
                                if (ProcessRecieveData(sendAndRecieve))
                                {
                                    if (sendAndRecieve.PollingGroup > 0)
                                        GetDataValueByGroup(sendAndRecieve.PollingGroup, sendAndRecieve.Time, sendAndRecieve.ScanTime);
                                }
                                else
                                {
                                    // ??處理..?..
                                    SendAlarmCode(EnumMIPCControlErrorCode.MIPC回傳資料異常);
                                }

                                LogFormat logFormat = new LogFormat(mipcDebug, "7", "", device, "", String.Concat("Process time : ", timer.ElapsedMilliseconds.ToString()));
                                loggerAgent.Log(logFormat.Category, logFormat);
                            }

                            communicationErrorCount = 0;
                        }
                        else
                        {
                            // ??處理..?..
                            communicationErrorCount++;

                            SendAlarmCode(EnumMIPCControlErrorCode.MIPC通訊異常);

                            if (communicationErrorCount > changeErrorCount)
                            {
                                for (int i = 0; i < socketListConnect.Count; i++)
                                    socketListConnect[i] = false;

                                WriteLog(5, "", String.Concat("通訊連續失敗次數超過 : ", changeErrorCount.ToString(), "次, 切成Error狀態"));
                                SendAlarmCode(EnumMIPCControlErrorCode.MIPC斷線);
                                Status = EnumControlStatus.Error;
                            }
                        }
                    }

                    //Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Socket Thread Exception : ", ex.ToString()));
            }
        }

        private int communicationErrorCount = 0;
        private int changeErrorCount = 10;

        private bool CheckAllSockeThreadEnd()
        {
            foreach (Thread thread in allSocketThread.Values)
            {
                if (thread != null && thread.IsAlive)
                    return false;
            }

            return true;
        }

        private void WriteHeartBeat()
        {
            try
            {
                if (allDataByIPCTagName.ContainsKey(EnumMecanumIPCdefaultTag.Heartbeat_IPC.ToString()) &&
                    MoveControlHeartBeat != lastMoveControlHeartBeat)
                //&&  allDataByIPCTagName[EnumMecanumIPCdefaultTag.IPC_Heartbeat.ToString()].Object != null)
                {
                    if (sendHeartbeat == null || sendHeartbeat.Result != EnumSendAndRecieve.None)
                    {
                        lastMoveControlHeartBeat = MoveControlHeartBeat;
                        ipcHeartbeatNumber++;

                        sendHeartbeat = new SendAndReceive();
                        List<Byte[]> writeData = new List<byte[]>();
                        writeData.Add(BitConverter.GetBytes((float)ipcHeartbeatNumber));
                        sendHeartbeat.Send = Write_連續(allDataByIPCTagName[EnumMecanumIPCdefaultTag.Heartbeat_IPC.ToString()].Address, 1, writeData);
                        sendHeartbeat.IsHearBeat = true;

                        AddWriteQueue(EnumMIPCSocketName.Normal.ToString(), sendHeartbeat);
                    }
                    else
                        WriteLog(3, "", "ipc write heartbeat lag");
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        private void PollingThread()
        {
            try
            {
                List<Stopwatch> pollingIntervalTimerList = new List<Stopwatch>();

                for (int i = 0; i < pollingDataList.Count; i++)
                {
                    pollingIntervalTimerList.Add(new Stopwatch());
                    pollingIntervalTimerList[i].Restart();
                }

                Stopwatch heartbeatTimer = new Stopwatch();
                Stopwatch safetySensorTimer = new Stopwatch();

                heartbeatTimer.Restart();
                safetySensorTimer.Restart();

                List<SendAndReceive> pollingList = new List<SendAndReceive>();

                for (int i = 0; i < pollingDataList.Count; i++)
                {
                    pollingList.Add(new SendAndReceive());
                    pollingList[pollingList.Count - 1].Result = EnumSendAndRecieve.OK;
                }

                while ((Status != EnumControlStatus.Closing || !CheckAllSockeThreadEnd()) && Status != EnumControlStatus.Error)
                {
                    for (int i = 0; i < pollingDataList.Count; i++)
                    {
                        if (pollingIntervalTimerList[i].ElapsedMilliseconds >= pollingIntervalList[i])
                        {
                            pollingIntervalTimerList[i].Restart();

                            if (pollingList[i].Result == EnumSendAndRecieve.None)
                                WriteLog(3, "", String.Concat("polling lag, groupe = ", pollingDataList[i].GroupNumber.ToString("0")));
                            else
                            {
                                pollingList[i] = new SendAndReceive();
                                pollingList[i].Send = Read_連續(pollingDataList[i].StartAddress, pollingDataList[i].Length);
                                pollingList[i].PollingGroup = pollingDataList[i].GroupNumber;

                                if (pollingList[i].Send == null)
                                    pollingList[i].Result = EnumSendAndRecieve.OK;
                                else
                                    AddReaeQueue(EnumMIPCSocketName.Normal.ToString(), pollingList[i]);
                            }
                        }
                    }

                    if (heartbeatTimer.ElapsedMilliseconds >= config.HeartbeatInterval)
                    {
                        heartbeatTimer.Restart();
                        WriteHeartBeat();

                        if (localData.MIPCData.GetDataByIPCTagName(EnumMecanumIPCdefaultTag.Light_Yellow.ToString()) != (alarmHandler.HasWarn ? 1 : 0))
                        {
                            SendMIPCDataByIPCTagName(new List<EnumMecanumIPCdefaultTag>() { EnumMecanumIPCdefaultTag.Light_Yellow }, new List<float>() { (alarmHandler.HasWarn ? 1 : 0) });
                        }

                        if (localData.MIPCData.GetDataByIPCTagName(EnumMecanumIPCdefaultTag.Light_Red.ToString()) != (alarmHandler.HasAlarm ? 1 : 0))
                        {
                            SendMIPCDataByIPCTagName(new List<EnumMecanumIPCdefaultTag>() { EnumMecanumIPCdefaultTag.Light_Red }, new List<float>() { (alarmHandler.HasAlarm ? 1 : 0) });
                        }
                    }

                    if (safetySensorTimer.ElapsedMilliseconds >= config.SafetySensorUpdateInterval)
                    {
                        safetySensorTimer.Restart();
                        SafetySensorControl.UpdateAllSafetySensor();
                    }

                    Thread.Sleep(5);
                }

                if (Status == EnumControlStatus.Closing)
                    Status = EnumControlStatus.Closed;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        #region MotionCommand.
        private Int32 GetValueByTagDataType(string ipcTagName, double value)
        {
            switch (allDataByIPCTagName[ipcTagName].DataType)
            {
                case EnumDataType.Double_1:
                    return (Int32)(value * 10);
                case EnumDataType.Int32:
                    return (Int32)value;
                default:
                    return 0;
            }
        }

        public bool WriteJogjoystickData(double lineVelocity, double lineAcc, double lineDec, double thetaVelocity, double thetaAcc, double thetaDec)
        {
            try
            {
                if (localData.SimulateMode)
                {
                    return true;
                }

                if (jogjoystickDataArray.Length == 0)
                {
                    WriteLog(3, "", String.Concat("Motion Command :: move 定義的tag在MIPC Config內有缺,因此無法執行移動命令"));
                    return false;
                }

                List<Byte[]> byteValueArray = new List<byte[]>();

                if (lineVelocity != -1)
                    byteValueArray.Add(BitConverter.GetBytes((float)(lineVelocity)));
                else
                    byteValueArray.Add(BitConverter.GetBytes((float)(localData.MoveControlData.MotionControlData.JoystickLineAxisData.Velocity)));

                if (lineAcc != -1)
                    byteValueArray.Add(BitConverter.GetBytes((float)(lineAcc)));
                else
                    byteValueArray.Add(BitConverter.GetBytes((float)(localData.MoveControlData.MotionControlData.JoystickLineAxisData.Acceleration)));

                if (lineDec != -1)
                    byteValueArray.Add(BitConverter.GetBytes((float)(lineDec)));
                else
                    byteValueArray.Add(BitConverter.GetBytes((float)(localData.MoveControlData.MotionControlData.JoystickLineAxisData.Deceleration)));


                if (thetaVelocity != -1)
                    byteValueArray.Add(BitConverter.GetBytes((float)(thetaVelocity)));
                else
                    byteValueArray.Add(BitConverter.GetBytes((float)(localData.MoveControlData.MotionControlData.JoystickThetaAxisData.Velocity)));

                if (thetaAcc != -1)
                    byteValueArray.Add(BitConverter.GetBytes((float)(thetaAcc)));
                else
                    byteValueArray.Add(BitConverter.GetBytes((float)(localData.MoveControlData.MotionControlData.JoystickThetaAxisData.Acceleration)));

                if (thetaDec != -1)
                    byteValueArray.Add(BitConverter.GetBytes((float)(thetaDec)));
                else
                    byteValueArray.Add(BitConverter.GetBytes((float)(localData.MoveControlData.MotionControlData.JoystickThetaAxisData.Deceleration)));

                SendAndReceive sendAndReceive = new SendAndReceive();

                sendAndReceive.Send = Write_非連續(jogjoystickDataArray, (UInt16)jogjoystickDataArray.Length, byteValueArray);
                AddWriteQueue(EnumMIPCSocketName.Normal.ToString(), sendAndReceive);

                Stopwatch timer = new Stopwatch();
                timer.Restart();

                while (sendAndReceive.Result == EnumSendAndRecieve.None)
                {
                    if (timer.ElapsedMilliseconds > config.CommandTimeoutValue)
                    {
                        WriteLog(3, "", String.Concat("timeout"));
                        return false;
                    }

                    Thread.Sleep(1);
                }

                if (sendAndReceive.Result == EnumSendAndRecieve.OK)
                {
                    localData.MoveControlData.MotionControlData.PreMoveStatus = EnumAxisMoveStatus.PreMove;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        public bool AGV_Move(MapAGVPosition end, double lineVelocity, double lineAcc, double lineDec, double lineJerk, double thetaVelocity, double thetaAcc, double thetaDec, double thetaJerk)
        {
            try
            {
                WriteLog(7, "", String.Concat("Move :: end ", computeFunction.GetMapAGVPositionStringWithAngle(end, "0"), ",lineVel : ", lineVelocity.ToString("0"),
                                              ",lineAcc : ", lineAcc.ToString("0"), ",lineDec : ", lineDec.ToString("0"), ",lineJerk : ", lineJerk.ToString("0"),
                                              ", thetaVel : ", thetaVelocity.ToString("0"), ", thetaAcc : ", thetaAcc.ToString("0"), ", thetaDec : ", thetaDec.ToString("0"),
                                              ", thetaJerk : ", thetaJerk.ToString("0")));

                if (localData.SimulateMode)
                {
                    localData.MoveControlData.MotionControlData.PreMoveStatus = EnumAxisMoveStatus.PreMove;
                    return true;
                }

                float x = (float)(end.Position.Y);
                float y = (float)(end.Position.X);
                float angle = (float)(end.Angle);

                if (moveCommandAddressArray.Length == 0)
                {
                    WriteLog(3, "", String.Concat("Motion Command :: move 定義的tag在MIPC Config內有缺,因此無法執行移動命令"));
                    return false;
                }

                List<Byte[]> byteValueArray = new List<byte[]>();

                byteValueArray.Add(BitConverter.GetBytes(x));
                byteValueArray.Add(BitConverter.GetBytes(y));
                byteValueArray.Add(BitConverter.GetBytes(angle));

                byteValueArray.Add(BitConverter.GetBytes((float)(lineVelocity)));
                byteValueArray.Add(BitConverter.GetBytes((float)(lineAcc)));
                byteValueArray.Add(BitConverter.GetBytes((float)(lineDec)));
                byteValueArray.Add(BitConverter.GetBytes((float)(lineJerk)));

                byteValueArray.Add(BitConverter.GetBytes((float)(thetaVelocity)));
                byteValueArray.Add(BitConverter.GetBytes((float)(thetaAcc)));
                byteValueArray.Add(BitConverter.GetBytes((float)(thetaDec)));
                byteValueArray.Add(BitConverter.GetBytes((float)(thetaJerk)));
                byteValueArray.Add(BitConverter.GetBytes((float)(1)));

                SendAndReceive sendAndReceive = new SendAndReceive();

                sendAndReceive.Send = Write_非連續(moveCommandAddressArray, (UInt16)moveCommandAddressArray.Length, byteValueArray);
                AddWriteQueue(EnumMIPCSocketName.Normal.ToString(), sendAndReceive);

                Stopwatch timer = new Stopwatch();
                timer.Restart();

                while (sendAndReceive.Result == EnumSendAndRecieve.None)
                {
                    if (timer.ElapsedMilliseconds > config.CommandTimeoutValue)
                    {
                        WriteLog(3, "", String.Concat("timeout"));
                        return false;
                    }

                    Thread.Sleep(1);
                }

                if (sendAndReceive.Result == EnumSendAndRecieve.OK)
                {
                    localData.MoveControlData.MotionControlData.PreMoveStatus = EnumAxisMoveStatus.PreMove;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        public bool AGV_ChangeVelocity(double newVelociy)
        {
            try
            {
                WriteLog(7, "", String.Concat("VChange :: velocity : ", newVelociy.ToString("0")));

                if (localData.SimulateMode)
                    return true;

                if (changeVelociyAddressArray.Length == 0)
                {
                    WriteLog(3, "", String.Concat("vChange Command :: vChange 定義的tag在MIPC Config內有缺,因此無法執行移動命令"));
                    return false;
                }

                List<Byte[]> byteValueArray = new List<byte[]>();

                byteValueArray.Add(BitConverter.GetBytes((float)(newVelociy)));

                //byteValueArray.Add(BitConverter.GetBytes((float)(1)));

                SendAndReceive sendAndReceive = new SendAndReceive();

                sendAndReceive.Send = Write_非連續(changeVelociyAddressArray, (UInt16)changeVelociyAddressArray.Length, byteValueArray);
                AddWriteQueue(EnumMIPCSocketName.Normal.ToString(), sendAndReceive);

                Stopwatch timer = new Stopwatch();
                timer.Restart();

                while (sendAndReceive.Result == EnumSendAndRecieve.None)
                {
                    if (timer.ElapsedMilliseconds > config.CommandTimeoutValue)
                    {
                        WriteLog(3, "", String.Concat("timeout"));
                        return false;
                    }

                    Thread.Sleep(1);
                }

                return sendAndReceive.Result == EnumSendAndRecieve.OK;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        public bool AGV_ChangeEnd(MapAGVPosition end)
        {
            try
            {
                WriteLog(7, "", String.Concat("ChangeEnd :: end : ", computeFunction.GetMapAGVPositionStringWithAngle(end, "0")));

                if (localData.SimulateMode)
                    return true;

                float x = (float)(end.Position.Y);
                float y = (float)(end.Position.X);
                float angle = (float)(end.Angle);

                if (changeEndAddressArray.Length == 0)
                {
                    WriteLog(3, "", String.Concat("Motion changeEnd :: changeEnd 定義的tag在MIPC Config內有缺,因此無法執行移動命令"));
                    return false;
                }

                List<Byte[]> byteValueArray = new List<byte[]>();

                byteValueArray.Add(BitConverter.GetBytes(x));
                byteValueArray.Add(BitConverter.GetBytes(y));
                byteValueArray.Add(BitConverter.GetBytes(angle));

                //byteValueArray.Add(BitConverter.GetBytes((float)(1)));

                SendAndReceive sendAndReceive = new SendAndReceive();

                sendAndReceive.Send = Write_非連續(changeEndAddressArray, (UInt16)changeEndAddressArray.Length, byteValueArray);
                AddWriteQueue(EnumMIPCSocketName.Normal.ToString(), sendAndReceive);

                Stopwatch timer = new Stopwatch();
                timer.Restart();

                while (sendAndReceive.Result == EnumSendAndRecieve.None)
                {
                    if (timer.ElapsedMilliseconds > config.CommandTimeoutValue)
                    {
                        WriteLog(3, "", String.Concat("timeout"));
                        return false;
                    }

                    Thread.Sleep(1);
                }

                return sendAndReceive.Result == EnumSendAndRecieve.OK;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        public bool AGV_Turn(MapAGVPosition start, double R, double RAngle, double movingVelocity, double movingAngle, double deltaAngle)
        {
            try
            {
                if (localData.SimulateMode)
                {
                    localData.MoveControlData.MotionControlData.PreMoveStatus = EnumAxisMoveStatus.PreMove;
                    return true;
                }

                float x = (float)(start.Position.Y);
                float y = (float)(start.Position.X);
                float angle = (float)(start.Angle);

                if (turnCommandAddressArray.Length == 0)
                {
                    WriteLog(3, "", String.Concat("Motion Command :: move 定義的tag在MIPC Config內有缺,因此無法執行移動命令"));
                    return false;
                }

                List<Byte[]> byteValueArray = new List<byte[]>();

                byteValueArray.Add(BitConverter.GetBytes(x));
                byteValueArray.Add(BitConverter.GetBytes(y));
                byteValueArray.Add(BitConverter.GetBytes(angle));

                byteValueArray.Add(BitConverter.GetBytes((float)(R)));
                byteValueArray.Add(BitConverter.GetBytes((float)(RAngle)));
                byteValueArray.Add(BitConverter.GetBytes((float)(movingVelocity)));
                byteValueArray.Add(BitConverter.GetBytes((float)(movingAngle)));

                byteValueArray.Add(BitConverter.GetBytes((float)(deltaAngle)));
                byteValueArray.Add(BitConverter.GetBytes((float)(1)));

                SendAndReceive sendAndReceive = new SendAndReceive();

                sendAndReceive.Send = Write_非連續(turnCommandAddressArray, (UInt16)turnCommandAddressArray.Length, byteValueArray);
                AddWriteQueue(EnumMIPCSocketName.Normal.ToString(), sendAndReceive);

                Stopwatch timer = new Stopwatch();
                timer.Restart();

                while (sendAndReceive.Result == EnumSendAndRecieve.None)
                {
                    if (timer.ElapsedMilliseconds > config.CommandTimeoutValue)
                    {
                        WriteLog(3, "", String.Concat("timeout"));
                        return false;
                    }

                    Thread.Sleep(1);
                }

                if (sendAndReceive.Result == EnumSendAndRecieve.OK)
                {
                    localData.MoveControlData.MotionControlData.PreMoveStatus = EnumAxisMoveStatus.PreMove;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        public bool AGV_CheckTimeEnd()
        {
            try
            {
                return false;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        public bool AGV_Stop(double lineDec, double lineJerk, double thetaDec, double thetaJerk)
        {
            try
            {
                WriteLog(7, "", String.Concat("Stop :: lineDec : ", lineDec.ToString("0"), ",lineJerk : ", lineJerk.ToString("0"),
                                               ", thetaDec : ", thetaDec.ToString("0"), ", thetaJerk : ", thetaJerk.ToString("0")));

                if (localData.SimulateMode)
                {
                    localData.MoveControlData.MotionControlData.MoveStatus = EnumAxisMoveStatus.PreStop;
                    return true;
                }

                if (stopCommandAddressArray.Length == 0)
                {
                    WriteLog(3, "", String.Concat("Motion Command :: stop 定義的tag在MIPC Config內有缺,因此無法執行移動命令"));
                    return false;
                }

                List<Byte[]> byteValueArray = new List<byte[]>();


                byteValueArray.Add(BitConverter.GetBytes((float)(lineDec)));
                byteValueArray.Add(BitConverter.GetBytes((float)(lineJerk)));

                byteValueArray.Add(BitConverter.GetBytes((float)(thetaDec)));
                byteValueArray.Add(BitConverter.GetBytes((float)(thetaJerk)));
                byteValueArray.Add(BitConverter.GetBytes((float)(1)));

                SendAndReceive sendAndReceive = new SendAndReceive();

                sendAndReceive.Send = Write_非連續(stopCommandAddressArray, (UInt16)stopCommandAddressArray.Length, byteValueArray);
                AddWriteQueue(EnumMIPCSocketName.Normal.ToString(), sendAndReceive);

                Stopwatch timer = new Stopwatch();
                timer.Restart();

                while (sendAndReceive.Result == EnumSendAndRecieve.None)
                {
                    if (timer.ElapsedMilliseconds > config.CommandTimeoutValue)
                    {
                        WriteLog(3, "", String.Concat("timeout"));
                        return false;
                    }

                    Thread.Sleep(1);
                }

                bool result = sendAndReceive.Result == EnumSendAndRecieve.OK;

                if (result)
                {
                    localData.MoveControlData.MotionControlData.MoveStatus = EnumAxisMoveStatus.PreStop;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        public bool AGV_EMS()
        {
            try
            {
                if (localData.SimulateMode)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        public void AGV_Reset()
        {
            try
            {
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        private bool ServoOnOff(bool onOff)
        {
            try
            {
                WriteLog(7, "", String.Concat("ServoOnOff : ", (onOff ? "on" : "off")));

                if (localData.SimulateMode)
                    return true;

                if (allDataByIPCTagName.ContainsKey(EnumMecanumIPCdefaultTag.ServoOnOff.ToString()))
                {
                    SendAndReceive sendAndReceive = new SendAndReceive();

                    List<Byte[]> data = new List<byte[]>();

                    switch (allDataByIPCTagName[EnumMecanumIPCdefaultTag.ServoOnOff.ToString()].DataType)
                    {
                        case EnumDataType.Boolean:
                            if (onOff)
                                data.Add(new Byte[4] { 1, 0, 0, 0 });
                            else
                                data.Add(new Byte[4] { 0, 0, 0, 0 });

                            break;

                        case EnumDataType.Float:
                            data.Add(BitConverter.GetBytes((float)(onOff ? 1 : 0)));
                            break;

                        default:
                            WriteLog(3, "", String.Concat(EnumMecanumIPCdefaultTag.ServoOnOff.ToString(), " type not boolean or float"));
                            return false;
                    }

                    sendAndReceive.Send = Write_連續(allDataByIPCTagName[EnumMecanumIPCdefaultTag.ServoOnOff.ToString()].Address, 1, data);
                    AddWriteQueue(EnumMIPCSocketName.Normal.ToString(), sendAndReceive);

                    Stopwatch timer = new Stopwatch();
                    timer.Restart();

                    while (sendAndReceive.Result == EnumSendAndRecieve.None)
                    {
                        if (timer.ElapsedMilliseconds > config.CommandTimeoutValue)
                        {
                            WriteLog(3, "", String.Concat("timeout"));
                            return false;
                        }

                        Thread.Sleep(1);
                    }

                    return sendAndReceive.Result == EnumSendAndRecieve.OK;
                }
                else
                {
                    WriteLog(3, "", String.Concat(EnumMecanumIPCdefaultTag.ServoOnOff.ToString(), " not define in mipcConfig"));
                    return false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        public bool JogjoystickOnOff(bool onOff)
        {
            try
            {
                WriteLog(7, "", String.Concat("JogjoystickOnOff : ", (onOff ? "on" : "off")));

                if (localData.SimulateMode)
                    return true;

                if (allDataByIPCTagName.ContainsKey(EnumMecanumIPCdefaultTag.JoystickOnOff.ToString()))
                {
                    SendAndReceive sendAndReceive = new SendAndReceive();

                    List<Byte[]> data = new List<byte[]>();

                    switch (allDataByIPCTagName[EnumMecanumIPCdefaultTag.JoystickOnOff.ToString()].DataType)
                    {
                        case EnumDataType.Boolean:
                            if (onOff)
                                data.Add(new Byte[4] { 1, 0, 0, 0 });
                            else
                                data.Add(new Byte[4] { 0, 0, 0, 0 });

                            break;

                        case EnumDataType.Float:
                            data.Add(BitConverter.GetBytes((float)(onOff ? 1 : 0)));
                            break;

                        default:
                            WriteLog(3, "", String.Concat(EnumMecanumIPCdefaultTag.JoystickOnOff.ToString(), " type not boolean or float"));
                            return false;
                    }

                    sendAndReceive.Send = Write_連續(allDataByIPCTagName[EnumMecanumIPCdefaultTag.JoystickOnOff.ToString()].Address, 1, data);
                    AddWriteQueue(EnumMIPCSocketName.Normal.ToString(), sendAndReceive);

                    Stopwatch timer = new Stopwatch();
                    timer.Restart();

                    while (sendAndReceive.Result == EnumSendAndRecieve.None)
                    {
                        if (timer.ElapsedMilliseconds > config.CommandTimeoutValue)
                        {
                            WriteLog(3, "", String.Concat("timeout"));
                            return false;
                        }

                        Thread.Sleep(1);
                    }

                    return sendAndReceive.Result == EnumSendAndRecieve.OK;
                }
                else
                {
                    WriteLog(3, "", String.Concat(EnumMecanumIPCdefaultTag.JoystickOnOff.ToString(), " not define in mipcConfig"));
                    return false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        public void ResetMIPCAlarm()
        {
            try
            {
                if (localData.SimulateMode)
                    return;

                if (allDataByIPCTagName.ContainsKey(EnumMecanumIPCdefaultTag.ResetAlarm.ToString()))
                {
                    SendAndReceive sendAndReceive = new SendAndReceive();

                    List<Byte[]> data = new List<byte[]>();

                    switch (allDataByIPCTagName[EnumMecanumIPCdefaultTag.ResetAlarm.ToString()].DataType)
                    {
                        case EnumDataType.Float:
                            data.Add(BitConverter.GetBytes((float)(1)));
                            break;
                        default:
                            return;
                    }

                    sendAndReceive.Send = Write_連續(allDataByIPCTagName[EnumMecanumIPCdefaultTag.ResetAlarm.ToString()].Address, 1, data);
                    AddWriteQueue(EnumMIPCSocketName.Normal.ToString(), sendAndReceive);
                }
                else
                {
                    WriteLog(3, "", String.Concat(EnumMecanumIPCdefaultTag.ResetAlarm.ToString(), " not define in mipcConfig"));
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        public bool AGV_ServoOn()
        {
            return ServoOnOff(true);
        }

        public bool AGV_ServoOff()
        {
            return ServoOnOff(false);
        }

        private float GetSendTimeStamp(DateTime getDataTime, double scnaTime)
        {
            TimeStampData nowTimeStamp = timeStampData;

            if (nowTimeStamp == null)
            {
                WriteLog(3, "", "TimeStampError, timeStampData == null");
                return 0;
            }

            double timeStampValue = (getDataTime - nowTimeStamp.SendTime).TotalMilliseconds - scnaTime;

            while (timeStampValue > overflowValue)
            {
                nowTimeStamp.SendTime = nowTimeStamp.SendTime.AddMilliseconds(overflowValue);
                timeStampValue -= overflowValue;
            }

            if (timeStampValue < 0)
            {
                WriteLog(3, "", "TimeStampError < 0!");
                return 0;
            }

            return (float)timeStampValue;
        }

        public bool SetPosition(LocateAGVPosition locateAGVPosition, bool waitResult)
        {
            try
            {
                if (timeStampData == null)
                {
                    WriteLog(7, "", String.Concat("由於timeStamp未設定,因此不能SetPosition"));
                    return false;
                }

                float x = (float)(locateAGVPosition.AGVPosition.Position.Y);
                float y = (float)(locateAGVPosition.AGVPosition.Position.X);
                float angle = (float)(locateAGVPosition.AGVPosition.Angle);

                if (setPositionAddressArray.Length == 0)
                {
                    WriteLog(3, "", String.Concat("SetPosition Command :: setPosition 定義的tag在MIPC Config內有缺,因此無法執行移動命令"));
                    return false;
                }

                List<Byte[]> byteValueArray = new List<byte[]>();

                byteValueArray.Add(BitConverter.GetBytes(x));
                byteValueArray.Add(BitConverter.GetBytes(y));
                byteValueArray.Add(BitConverter.GetBytes(angle));

                // timeStmap.
                byteValueArray.Add(BitConverter.GetBytes(GetSendTimeStamp(locateAGVPosition.GetDataTime, locateAGVPosition.ScanTime)));

                // start.
                byteValueArray.Add(BitConverter.GetBytes((float)(1)));

                SendAndReceive sendAndReceive = new SendAndReceive();

                sendAndReceive.Send = Write_非連續(setPositionAddressArray, (UInt16)setPositionAddressArray.Length, byteValueArray);
                AddWriteQueue(EnumMIPCSocketName.Normal.ToString(), sendAndReceive);

                if (!waitResult)
                    return true;
                else
                {
                    Stopwatch timer = new Stopwatch();
                    timer.Restart();

                    while (sendAndReceive.Result == EnumSendAndRecieve.None)
                    {
                        if (timer.ElapsedMilliseconds > config.CommandTimeoutValue)
                        {
                            WriteLog(3, "", String.Concat("timeout"));
                            return false;
                        }

                        Thread.Sleep(1);
                    }

                    return sendAndReceive.Result == EnumSendAndRecieve.OK;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        public string MotionCommand(string command)
        {
            try
            {
                if (localData.AutoManual != EnumAutoState.Manual && localData.MoveControlData.MoveCommand == null)
                {
                    SendAndReceive data = new SendAndReceive();
                    data.Send = new ModbusData();
                    data.Send.ByteData = Encoding.ASCII.GetBytes(command);
                    data.Send.RecieveLength = 256;
                    data.IsMotionCommand = true;

                    AddWriteQueue(EnumMIPCSocketName.MotionCommand.ToString(), data);

                    Stopwatch timer = new Stopwatch();
                    timer.Restart();

                    while (data.Result == EnumSendAndRecieve.None)
                    {
                        if (timer.ElapsedMilliseconds > config.CommandTimeoutValue)
                            return "Timeout";

                        Thread.Sleep(1);
                    }

                    return System.Text.Encoding.ASCII.GetString(data.Receive.ByteData, 0, 256);
                }
                else
                    return "Manual 下才可使用";
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return "Exception";
            }
        }
        #endregion

        public bool SendMIPCDataByMIPCTagName(List<string> tagNameList, List<float> dataList)
        {
            try
            {
                if (tagNameList.Count != dataList.Count)
                {
                    WriteLog(3, "", String.Concat("tagNameList count : ", tagNameList.Count.ToString("0"),
                                                  ", dataList count : ", dataList.Count.ToString("0"), " 數量不相符"));
                    return false;
                }

                uint[] tagAddressList = new uint[tagNameList.Count];
                List<Byte[]> byteValueArray = new List<byte[]>();

                for (int i = 0; i < tagNameList.Count; i++)
                {
                    tagAddressList[i] = allDataByMIPCTagName[tagNameList[i]].Address;
                    byteValueArray.Add(BitConverter.GetBytes(dataList[i]));
                }

                SendAndReceive sendAndReceive = new SendAndReceive();

                sendAndReceive.Send = Write_非連續(tagAddressList, (UInt16)tagAddressList.Length, byteValueArray);
                AddWriteQueue(EnumMIPCSocketName.Normal.ToString(), sendAndReceive);

                Stopwatch timer = new Stopwatch();
                timer.Restart();

                while (sendAndReceive.Result == EnumSendAndRecieve.None)
                {
                    if (timer.ElapsedMilliseconds > config.CommandTimeoutValue)
                    {
                        WriteLog(3, "", String.Concat("timeout"));
                        return false;
                    }

                    Thread.Sleep(1);
                }

                return sendAndReceive.Result == EnumSendAndRecieve.OK;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        public bool SendMIPCDataByIPCTagName(List<EnumMecanumIPCdefaultTag> IPCTagNameList, List<float> dataList)
        {
            try
            {
                if (IPCTagNameList.Count != dataList.Count)
                {
                    WriteLog(3, "", String.Concat("tagNameList count : ", IPCTagNameList.Count.ToString("0"),
                                                  ", dataList count : ", dataList.Count.ToString("0"), " 數量不相符"));
                    return false;
                }

                uint[] tagAddressList = new uint[IPCTagNameList.Count];
                List<Byte[]> byteValueArray = new List<byte[]>();

                for (int i = 0; i < IPCTagNameList.Count; i++)
                {
                    tagAddressList[i] = allDataByIPCTagName[IPCTagNameList[i].ToString()].Address;
                    byteValueArray.Add(BitConverter.GetBytes(dataList[i]));
                }

                SendAndReceive sendAndReceive = new SendAndReceive();

                sendAndReceive.Send = Write_非連續(tagAddressList, (UInt16)tagAddressList.Length, byteValueArray);
                AddWriteQueue(EnumMIPCSocketName.Normal.ToString(), sendAndReceive);

                Stopwatch timer = new Stopwatch();
                timer.Restart();

                while (sendAndReceive.Result == EnumSendAndRecieve.None)
                {
                    if (timer.ElapsedMilliseconds > config.CommandTimeoutValue)
                    {
                        WriteLog(3, "", String.Concat("timeout"));
                        return false;
                    }

                    Thread.Sleep(1);
                }

                return sendAndReceive.Result == EnumSendAndRecieve.OK;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        #region NewSendPackage.
        public ModbusData Read_連續(UInt32 startAddress, UInt16 length)
        {
            try
            {
                lock (newSendLockObject)
                {
                    ModbusData returnModbusData = new ModbusData();
                    returnModbusData.SeqNumber = count;
                    returnModbusData.FunctionCode = 0x01;
                    returnModbusData.StartAddress = startAddress;
                    returnModbusData.DataLength = length;
                    returnModbusData.RecieveLength = 12 + 4 * length;

                    returnModbusData.InitialByteData();
                    count++;
                    return returnModbusData;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return null;
            }
        }

        public ModbusData Read_非連續()
        {
            try
            {
                lock (newSendLockObject)
                {
                    ModbusData returnModbusData = null;

                    count++;

                    return returnModbusData;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return null;
            }
        }

        public ModbusData Write_連續(UInt32 startAddress, UInt16 length, UInt32[] writeData)
        {
            try
            {
                lock (newSendLockObject)
                {
                    ModbusData returnModbusData = new ModbusData();
                    returnModbusData.SeqNumber = count;
                    returnModbusData.FunctionCode = 0x81;
                    returnModbusData.StartAddress = startAddress;
                    returnModbusData.DataLength = length;
                    returnModbusData.RecieveLength = 12;

                    returnModbusData.DataBuffer = new byte[length * 4];

                    byte[] tempArray;

                    for (int i = 0; i < length; i++)
                    {
                        tempArray = BitConverter.GetBytes(writeData[i]);
                        tempArray.CopyTo(returnModbusData.DataBuffer, i * 4);
                    }

                    returnModbusData.InitialByteData();
                    count++;

                    return returnModbusData;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return null;
            }
        }

        public ModbusData Write_連續(UInt32 startAddress, UInt16 length, List<Byte[]> writeData)
        {
            try
            {
                lock (newSendLockObject)
                {
                    if (length != writeData.Count)
                    {
                        WriteLog(3, "", "addressList.Count != writeData.Count");
                        return null;
                    }

                    ModbusData returnModbusData = new ModbusData();
                    returnModbusData.SeqNumber = count;
                    returnModbusData.FunctionCode = 0x81;
                    returnModbusData.StartAddress = startAddress;
                    returnModbusData.DataLength = length;
                    returnModbusData.RecieveLength = 12;

                    returnModbusData.DataBuffer = new byte[length * 4];

                    for (int i = 0; i < length; i++)
                    {
                        returnModbusData.DataBuffer[i * 4] = writeData[i][0];
                        returnModbusData.DataBuffer[i * 4 + 1] = writeData[i][1];
                        returnModbusData.DataBuffer[i * 4 + 2] = writeData[i][2];
                        returnModbusData.DataBuffer[i * 4 + 3] = writeData[i][3];
                    }

                    returnModbusData.InitialByteData();
                    count++;

                    return returnModbusData;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return null;
            }
        }

        public ModbusData Write_非連續(UInt32[] addressList, UInt16 length, List<Byte[]> writeData)
        {
            try
            {
                lock (newSendLockObject)
                {
                    if (addressList == null || writeData == null || addressList.Length != writeData.Count || length != writeData.Count)
                    {
                        WriteLog(3, "", "addressList.Count != writeData.Count");
                        return null;
                    }

                    ModbusData returnModbusData = new ModbusData();
                    returnModbusData.SeqNumber = count;
                    returnModbusData.FunctionCode = 0x83;
                    returnModbusData.StartAddress = 0;
                    returnModbusData.DataLength = length;

                    returnModbusData.RecieveLength = 12;

                    returnModbusData.DataBuffer = new byte[length * 4 * 2];

                    byte[] tempArray;

                    for (int i = 0; i < length; i++)
                    {
                        tempArray = BitConverter.GetBytes(addressList[i]);
                        tempArray.CopyTo(returnModbusData.DataBuffer, i * 4);
                    }

                    for (int i = 0; i < length; i++)
                    {
                        returnModbusData.DataBuffer[length * 4 + i * 4] = writeData[i][0];
                        returnModbusData.DataBuffer[length * 4 + i * 4 + 1] = writeData[i][1];
                        returnModbusData.DataBuffer[length * 4 + i * 4 + 2] = writeData[i][2];
                        returnModbusData.DataBuffer[length * 4 + i * 4 + 3] = writeData[i][3];
                    }

                    returnModbusData.InitialByteData();
                    count++;

                    return returnModbusData;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return null;
            }
        }

        public ModbusData Write_非連續(UInt32[] addressList, UInt16 length, Int32[] writeData)
        {
            try
            {
                lock (newSendLockObject)
                {
                    if (addressList == null || writeData == null || addressList.Length != writeData.Length)
                    {
                        WriteLog(3, "", "addressList.Count != writeData.Count");
                        return null;
                    }

                    ModbusData returnModbusData = new ModbusData();
                    returnModbusData.SeqNumber = count;
                    returnModbusData.FunctionCode = 0x83;
                    returnModbusData.StartAddress = 0;
                    returnModbusData.DataLength = length;

                    returnModbusData.RecieveLength = 12;

                    returnModbusData.DataBuffer = new byte[length * 4 * 2];

                    byte[] tempArray;

                    for (int i = 0; i < length; i++)
                    {
                        tempArray = BitConverter.GetBytes(addressList[i]);
                        tempArray.CopyTo(returnModbusData.DataBuffer, i * 4);
                    }

                    for (int i = 0; i < length; i++)
                    {
                        tempArray = BitConverter.GetBytes(writeData[i]);
                        tempArray.CopyTo(returnModbusData.DataBuffer, length * 4 + i * 4);
                    }

                    returnModbusData.InitialByteData();
                    count++;

                    return returnModbusData;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return null;
            }
        }
        #endregion

        private void BatteryCSVThread()
        {
            Stopwatch timer = new Stopwatch();
            string logString;
            DateTime now;

            while (Status != EnumControlStatus.Closing && Status != EnumControlStatus.Error)
            {
                timer.Restart();
                logString = "";
                now = DateTime.Now;
                logString = now.ToString("yyyy/MM/dd HH:mm:ss");

                logString = String.Concat(logString, ",", localData.BatteryInfo.SOC.ToString("0.00"));
                logString = String.Concat(logString, ",", localData.BatteryInfo.V.ToString("0.00"));

                logger.LogString(logString);

                while (timer.ElapsedMilliseconds < config.BatteryCSVInterval)
                    Thread.Sleep(1);
            }
        }

        public bool StartCharging()
        {
            return false;
        }

        public bool StopCharging()
        {
            return false;
        }
    }
}