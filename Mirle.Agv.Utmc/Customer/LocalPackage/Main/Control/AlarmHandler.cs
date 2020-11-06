using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Mirle.Agv.INX.Model;
using Mirle.Agv.INX.Model.Configs;
using Mirle.Agv.INX.Controller.Tools;
using System.Collections.Concurrent;
using System.Reflection;
using System.Diagnostics;

namespace Mirle.Agv.INX.Controller
{
    [Serializable]
    public class AlarmHandler
    {
        public Dictionary<int, Alarm> AllAlarms { get; set; } = new Dictionary<int, Alarm>();
        private Dictionary<int, Alarm> dicHappeningAlarms = new Dictionary<int, Alarm>();
        private object lockObject = new object();

        public bool HasAlarm { get; set; } = false;
        public bool HasWarn { get; set; } = false;
        public bool BuzzOff { get; set; } = false;

        private string alarmFileName = "AlarmCode.csv";
        private LoggerAgent loggerAgent = LoggerAgent.Instance;

        private string errorLogName = "Error";
        private string alarmHistoryLogName = "AlarmHistory";
        private string device = MethodInfo.GetCurrentMethod().ReflectedType.Name;

        public string NowAlarm { get; set; } = "";
        public string AlarmHistory { get; set; } = "";
        private int alarmHistoryMaxLength = 10000;

        public event EventHandler<Alarm> OnAlarmSetEvent;
        public event EventHandler OnAlarmResetEvent;

        public AlarmHandler()
        {
            LoadAlarmFile();
        }

        public void WriteLog(int logLevel, string carrierId, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            LogFormat logFormat = new LogFormat(errorLogName, logLevel.ToString(), memberName, device, carrierId, message);
            loggerAgent.Log(logFormat.Category, logFormat);
        }

        private void SetAlarmLog(string message)
        {
            loggerAgent.LogString(alarmHistoryLogName, message);
        }

        private void SetAlarmHistroyLog(DateTime now, string message)
        {
            AlarmHistory = String.Concat("[Reset] ", now.ToString("MM/dd-HH:mm:ss"), " ", message, "\r\n", AlarmHistory);

            if (AlarmHistory.Length > alarmHistoryMaxLength)
                AlarmHistory = AlarmHistory.Substring(0, alarmHistoryMaxLength);

            SetAlarmLog(String.Concat(now.ToString("yyyy/MM/dd HH:mm:ss.fff"), ",", message));
        }

        private void SetAlarmHistroyLogByAlarm(Alarm alarm)
        {
            string alarmMessage = string.Concat("[", alarm.Level.ToString(), "] ", alarm.Id.ToString("0"), " ", alarm.SetTime.ToString("MM/dd-HH:mm:ss"), " ", alarm.AlarmText, "\r\n");

            AlarmHistory = String.Concat(alarmMessage, AlarmHistory);
            NowAlarm = String.Concat(alarmMessage, NowAlarm);

            SetAlarmLog(String.Concat(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), ",", alarm.Id.ToString(), ",", alarm.AlarmText, ",", alarm.Level.ToString(), ",",
                                     alarm.SetTime.ToString("yyyy/MM/dd HH:mm:ss.fff"), ",", alarm.Description, ",", LocalData.Instance.AutoManual.ToString()));
        }

        #region Read AlarmCode.csv.
        private void LoadAlarmFile()
        {
            try
            {
                string alarmFullPath = Path.Combine(LocalData.Instance.MapConfig.FileDirectory, alarmFileName);

                if (!File.Exists(alarmFullPath))
                {
                    WriteLog(3, "", String.Concat("找不到AlarmCode.csv, path : ", alarmFullPath));
                    return;
                }

                Dictionary<string, int> dicAlarmIndexes = new Dictionary<string, int>();
                AllAlarms.Clear();

                string[] allRows = File.ReadAllLines(alarmFullPath, Encoding.UTF8);

                if (allRows == null || allRows.Length < 2)
                {
                    WriteLog(3, "", "There are no alarms in file");
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                //Id, AlarmText, PlcAddress, PlcBitNumber, Level, Description
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();

                    if (!string.IsNullOrWhiteSpace(keyword))
                        dicAlarmIndexes.Add(keyword, i);
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = LoadAlarmFile_SplitCsvLine(allRows[i]);
                    Alarm oneRow = new Alarm();
                    oneRow.Id = int.Parse(getThisRow[dicAlarmIndexes["Id"]]);
                    oneRow.AlarmText = getThisRow[dicAlarmIndexes["AlarmText"]];
                    oneRow.Level = EnumAlarmLevelParse(getThisRow[dicAlarmIndexes["Level"]]);
                    oneRow.Description = getThisRow[dicAlarmIndexes["Description"]];

                    if (AllAlarms.ContainsKey(oneRow.Id))
                        WriteLog(3, "", String.Concat("Alarm code : ", oneRow.Id.ToString(), "repeat"));
                    else
                        AllAlarms.Add(oneRow.Id, oneRow);
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                WriteLog(3, "", String.Concat("Exception : ", ex.StackTrace));
            }
        }

        public void TryAddNewAlarmCode(int alarmCodeID, string alarmText, EnumAlarmLevel level, string description)
        {
            Alarm newAlarm = new Alarm();
            newAlarm.Id = alarmCodeID;
            newAlarm.AlarmText = alarmText;
            newAlarm.Level = level;
            newAlarm.Description = description;

            if (!AllAlarms.ContainsKey(newAlarm.Id))
                AllAlarms.Add(newAlarm.Id, newAlarm);
        }

        private string[] LoadAlarmFile_SplitCsvLine(string strLine)
        {
            string[] result = new string[6];

            if (strLine.Contains('"'))
            {
                string[] temp = strLine.Split(',');
                int resultIndex = 0;
                bool quoteFlag = false;
                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i].Contains('"') && quoteFlag == false)
                    {
                        quoteFlag = true;
                        result[resultIndex] = temp[i].Substring(1);
                    }
                    else if (temp[i].Contains('"') && quoteFlag == true)
                    {
                        result[resultIndex] = result[resultIndex] + "," + temp[i].Substring(0, temp[i].Length - 2);
                        quoteFlag = false;
                        resultIndex++;
                    }
                    else if (!temp[i].Contains('"') && quoteFlag == false)
                    {
                        result[resultIndex] = temp[i];
                        resultIndex++;
                    }
                    else if (!temp[i].Contains('"') && quoteFlag == true)
                    {
                        result[resultIndex] = result[resultIndex] + "," + temp[i];
                    }
                }

            }
            else
            {
                result = strLine.Split(',');
            }

            return result;

        }

        private EnumAlarmLevel EnumAlarmLevelParse(string v)
        {
            try
            {
                v = v.Trim();

                return (EnumAlarmLevel)Enum.Parse(typeof(EnumAlarmLevel), v);
            }
            catch (Exception ex)
            {
                loggerAgent.Log("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                  , ex.StackTrace));
                return EnumAlarmLevel.Warn;
            }
        }
        #endregion

        public void SetAlarm(int alarmCodeID)
        {
            try
            {
                DateTime timeStamp = DateTime.Now;
                Alarm alarm = AllAlarms.ContainsKey(alarmCodeID) ? AllAlarms[alarmCodeID] : new Alarm { Id = alarmCodeID };
                alarm.SetTime = timeStamp;

                lock (lockObject)
                {
                    if (dicHappeningAlarms.ContainsKey(alarmCodeID))
                        ;
                    else
                    {
                        dicHappeningAlarms.Add(alarmCodeID, alarm);
                        SetAlarmHistroyLogByAlarm(alarm);

                        switch (alarm.Level)
                        {
                            case EnumAlarmLevel.Alarm:
                                HasAlarm = true;
                                break;
                            case EnumAlarmLevel.Warn:
                            default:
                                HasWarn = true;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(5, "", "Exception : ", ex.ToString());
                WriteLog(5, "", "Exception : ", ex.StackTrace);
            }
        }

        public void ResetAllAlarms()
        {
            try
            {
                DateTime now = DateTime.Now;

                lock (lockObject)
                {
                    if (HasAlarm && HasWarn)
                        SetAlarmHistroyLog(now, "清除所有Alarm, Warn");
                    else if (HasAlarm)
                        SetAlarmHistroyLog(now, "清除所有Alarm");
                    else if (HasWarn)
                        SetAlarmHistroyLog(now, "清除所有Warn");

                    dicHappeningAlarms = new Dictionary<int, Alarm>();
                    HasAlarm = false;
                    HasWarn = false;
                    BuzzOff = false;

                    NowAlarm = "";
                }
            }
            catch (Exception ex)
            {
                WriteLog(5, "", "Exception : ", ex.ToString());
                WriteLog(5, "", "Exception : ", ex.StackTrace);
            }
        }
    }
}
