using Mirle.Agv.Utmc.Controller;
using Mirle.Agv.Utmc.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Alarms
{
    public class NullObjAlarmHandler : IAlarmHandler
    {
        public event EventHandler<MessageHandlerArgs> OnLogDebugEvent;
        public event EventHandler<MessageHandlerArgs> OnLogErrorEvent;
        public event EventHandler<AgvcAlarmArgs> OnSetAlarmToAgvcEvent;
        public event EventHandler OnResetAlarmToAgvcEvent;

        public MainAlarmHandler MainAlarmHandler { get; set; } = new MainAlarmHandler();

        public NullObjAlarmHandler()
        {

        }

        public void ResetAllAlarmsFromAgvc()
        {
            MainAlarmHandler.ResetAllAlarms();
        }

        public void ResetAllAlarmsFromAgvm()
        {
            MainAlarmHandler.ResetAllAlarms();
            OnResetAlarmToAgvcEvent?.Invoke(this, default);
        }

        public void SetAlarmFromAgvm(int errorCode)
        {
            MainAlarmHandler.SetAlarm(errorCode);
            if (!MainAlarmHandler.dicHappeningAlarms.ContainsKey(errorCode))
            {
                MainAlarmHandler.SetAlarm(errorCode);

                var isAlarm = MainAlarmHandler.IsAlarm(errorCode);

                OnSetAlarmToAgvcEvent?.Invoke(this, new AgvcAlarmArgs()
                {
                    ErrorCode = errorCode,
                    IsAlarm = isAlarm,
                });               
            }
        }

        public string GetLastAlarmMsg()
        {
            return MainAlarmHandler.LastAlarmMsg;
        }

        public string GetAlarmLogMsg()
        {
            return MainAlarmHandler.AlarmLogMsg;
        }

        public string GetAlarmHistoryLogMsg()
        {
            return MainAlarmHandler.AlarmHistoryLogMsg;
        }

        public bool HasHappeningAlarm()
        {
            return MainAlarmHandler.dicHappeningAlarms.Any();
        }
    }
}
