using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Alarms
{
    public interface IAlarmHandler : Tools.IMessageHandler
    {
        public event EventHandler<AgvcAlarmArgs> OnSetAlarmToAgvcEvent;
        public event EventHandler OnResetAlarmToAgvcEvent;

        public void SetAlarmFromAgvm(int errorCode);
        public void ResetAllAlarmsFromAgvm();
        public void ResetAllAlarmsFromAgvc();
        public string GetLastAlarmMsg();
        public string GetAlarmLogMsg();
        public string GetAlarmHistoryLogMsg();
        public bool HasHappeningAlarm();       
    }
}
