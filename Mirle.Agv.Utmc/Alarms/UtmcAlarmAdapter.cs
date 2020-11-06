using Mirle.Agv.Utmc.Controller;
using Mirle.Agv.Utmc.Customer;
using Mirle.Agv.Utmc.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Alarms
{
    public class UtmcAlarmAdapter : IAlarmHandler
    {
        public event EventHandler<MessageHandlerArgs> OnLogDebugEvent;
        public event EventHandler<MessageHandlerArgs> OnLogErrorEvent;
        public event EventHandler<AgvcAlarmArgs> OnSetAlarmToAgvcEvent;
        public event EventHandler OnResetAlarmToAgvcEvent;

        public LocalPackage LocalPackage { get; set; }
        public MainAlarmHandler MainAlarmHandler { get; set; } = new MainAlarmHandler();

        public UtmcAlarmAdapter(LocalPackage localPackage)
        {
            this.LocalPackage = localPackage;
            LocalPackage.MainFlowHandler.AlarmHandler.OnAlarmResetEvent += AlarmHandler_OnAlarmResetEvent;
            LocalPackage.MainFlowHandler.AlarmHandler.OnAlarmSetEvent += AlarmHandler_OnAlarmSetEvent;
        }

        private void AlarmHandler_OnAlarmSetEvent(object sender, INX.Model.Alarm e)
        {
            if (!MainAlarmHandler.dicHappeningAlarms.ContainsKey(e.Id))
            {
                MainAlarmHandler.SetAlarm(e.Id);
                OnSetAlarmToAgvcEvent?.Invoke(sender, new AgvcAlarmArgs()
                {
                    ErrorCode = e.Id,
                    IsAlarm = MainAlarmHandler.IsAlarm(e.Id)
                });
                OnLogDebugEvent?.Invoke(sender, new MessageHandlerArgs()
                {
                     ClassMethodName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    Message  = MainAlarmHandler.GetAlarmText(e.Id)
                });
            }
        }

        private void AlarmHandler_OnAlarmResetEvent(object sender, EventArgs e)
        {
            MainAlarmHandler.ResetAllAlarms();
            OnResetAlarmToAgvcEvent?.Invoke(sender, default);
        }

        public void ResetAllAlarmsFromAgvc()
        {
            MainAlarmHandler.ResetAllAlarms();
            LocalPackage.MainFlowHandler.ResetAlarm();
        }

        public void ResetAllAlarmsFromAgvm()
        {
            MainAlarmHandler.ResetAllAlarms();
            OnResetAlarmToAgvcEvent?.Invoke(this, default);
            LocalPackage.MainFlowHandler.ResetAlarm();
        }

        public void SetAlarmFromAgvm(int errorCode)
        {
            if (!MainAlarmHandler.dicHappeningAlarms.ContainsKey(errorCode))
            {
                MainAlarmHandler.SetAlarm(errorCode);
                LocalPackage.MainFlowHandler.AlarmHandler.SetAlarm(errorCode);
                OnSetAlarmToAgvcEvent?.Invoke(this, new AgvcAlarmArgs()
                {
                    ErrorCode = errorCode,
                    IsAlarm = MainAlarmHandler.IsAlarm(errorCode)
                });
                OnLogDebugEvent?.Invoke(this, new MessageHandlerArgs()
                {
                    ClassMethodName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    Message = MainAlarmHandler.GetAlarmText(errorCode)
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
            return MainAlarmHandler.dicHappeningAlarms.Any() || LocalPackage.MainFlowHandler.AlarmHandler.HasAlarm;
        }
    }
}
