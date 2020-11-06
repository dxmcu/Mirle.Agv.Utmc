using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Mirle.Agv.Utmc.Model;
using Mirle.Agv.Utmc.Model.Configs;
using System.Collections.Concurrent;
using System.Reflection;
using System.Diagnostics;
using Mirle.Tools;
using Mirle.Agv.Utmc.Tools;

namespace Mirle.Agv.Utmc.Controller
{

    public class MainAlarmHandler
    {
        #region Containers
        public Dictionary<int, Alarm> allAlarms = new Dictionary<int, Alarm>();
        public ConcurrentDictionary<int, Alarm> dicHappeningAlarms = new ConcurrentDictionary<int, Alarm>();
        public string LastAlarmMsg { get; set; } = "";
        #endregion     

        private MirleLogger mirleLogger = MirleLogger.Instance;
        public Vehicle Vehicle { get; set; } = Vehicle.Instance;

        public string AlarmLogMsg { get; set; } = "";
        public string AlarmHistoryLogMsg { get; set; } = "";

        public MainAlarmHandler()
        {
            LoadAlarmFile();
        }

        private void LoadAlarmFile()
        {
            try
            {
                if (string.IsNullOrEmpty(Vehicle.AlarmConfig.AlarmFileName))
                {
                    throw new Exception($"string.IsNullOrEmpty(alarmConfig.AlarmFileName)={string.IsNullOrEmpty(Vehicle.AlarmConfig.AlarmFileName)}");
                }

                string alarmFullPath = Path.Combine(Environment.CurrentDirectory, Vehicle.AlarmConfig.AlarmFileName);
                Dictionary<string, int> dicAlarmIndexes = new Dictionary<string, int>();
                allAlarms.Clear();

                string[] allRows = File.ReadAllLines(alarmFullPath);
                if (allRows == null || allRows.Length < 2)
                {
                    throw new Exception("There are no alarms in file");
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
                    {
                        dicAlarmIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    Alarm oneRow = new Alarm();
                    oneRow.Id = int.Parse(getThisRow[dicAlarmIndexes["Id"]]);
                    oneRow.AlarmText = getThisRow[dicAlarmIndexes["AlarmText"]];
                    if (Enum.TryParse(getThisRow[dicAlarmIndexes["Level"]], out EnumAlarmLevel level))
                    {
                        oneRow.Level = level;
                    }
                    oneRow.Description = getThisRow[dicAlarmIndexes["Description"]];

                    allAlarms.Add(oneRow.Id, oneRow);
                }

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Load Alarm File Ok");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void SetAlarm(int id)
        {
            try
            {
                DateTime timeStamp = DateTime.Now;
                Alarm alarm = allAlarms.ContainsKey(id) ? allAlarms[id] : new Alarm { Id = id };
                alarm.SetTime = timeStamp;

                dicHappeningAlarms.TryAdd(id, alarm);
                LastAlarmMsg = $@"[{alarm.Id}][{alarm.AlarmText}]";

                LogAlarmHistory(alarm);
                var alarmMessage = $@"[ID={alarm.Id}][Text={alarm.AlarmText}][{alarm.Level}][SetTime={alarm.SetTime.ToString("HH-mm-ss.fff")}][Description={alarm.Description}]";
                AppendAlarmLogMsg(alarmMessage);
                AppendAlarmHistoryLogMsg(alarmMessage);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void ResetAllAlarms()
        {
            try
            {
                lock (dicHappeningAlarms)
                {
                    dicHappeningAlarms = new ConcurrentDictionary<int, Alarm>();
                    LastAlarmMsg = "";
                }

                string resetMessage = "Reset All Alarms.";
                LogAlarmHistory(resetMessage);
                AppendAlarmHistoryLogMsg(resetMessage);
                AlarmLogMsg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "\t", resetMessage);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return EnumAlarmLevel.Warn;
            }
        }

        private void LogAlarmHistory(Alarm alarm)
        {
            try
            {
                string msg = $"{alarm.Id},{alarm.AlarmText},{alarm.Level},{alarm.SetTime},{alarm.ResetTime},{alarm.Description}";

                mirleLogger.LogString("MainAlarmHistory", msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void LogAlarmHistory(string msg)
        {
            try
            {
                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff");

                mirleLogger.LogString("MainAlarmHistory", msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public string GetAlarmText(int errorCode)
        {
            try
            {
                return allAlarms[errorCode].GetJsonInfo();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return $"[Id={errorCode}][Text=Unknow]";
            }

            //if (allAlarms.ContainsKey(errorCode))
            //{
            //    var alarm = allAlarms[errorCode];
            //    return $"[Id={errorCode}][Text={alarm.AlarmText}]";
            //}

            //return $"[Id={errorCode}][Text=Unknow]";
        }

        public bool IsAlarm(int errorCode)
        {
            if (allAlarms.ContainsKey(errorCode))
            {
                return allAlarms[errorCode].Level == EnumAlarmLevel.Alarm;
            }

            return false;
        }

        public void AppendAlarmLogMsg(string msg)
        {
            try
            {
                AlarmLogMsg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "\t", msg, "\r\n", AlarmLogMsg);

                if (AlarmLogMsg.Length > 65535)
                {
                    AlarmLogMsg = AlarmLogMsg.Substring(65535);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void AppendAlarmHistoryLogMsg(string msg)
        {
            try
            {
                AlarmHistoryLogMsg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "\t", msg, "\r\n", AlarmHistoryLogMsg);

                if (AlarmHistoryLogMsg.Length > 65535)
                {
                    AlarmHistoryLogMsg = AlarmHistoryLogMsg.Substring(65535);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void LogException(string classMethodName, string msg)
        {
            mirleLogger.Log(new LogFormat("MainError", "5", classMethodName, "Device", "CarrierID", msg));
        }

        private void LogDebug(string classMethodName, string msg)
        {
            mirleLogger.Log(new LogFormat("MainDebug", "5", classMethodName, "Device", "CarrierID", msg));
        }

    }
}
