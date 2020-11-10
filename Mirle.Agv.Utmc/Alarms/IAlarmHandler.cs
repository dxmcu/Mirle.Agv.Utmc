using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Alarms
{
    interface IAlarmHandler : Tools.IMessageHandler
    {
        event EventHandler<AgvcAlarmArgs> OnSetAlarmToAgvcEvent;
        event EventHandler OnResetAlarmToAgvcEvent;

        void SetAlarmFromAgvm(int errorCode);
        void ResetAllAlarmsFromAgvm();
        void ResetAllAlarmsFromAgvc();
        string GetLastAlarmMsg();
        string GetAlarmLogMsg();
        string GetAlarmHistoryLogMsg();
        bool HasHappeningAlarm();
    }
}
