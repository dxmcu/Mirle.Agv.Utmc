using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Battery
{
    interface IBatteryHandler : Tools.IMessageHandler
    {
        event EventHandler<Model.BatteryStatus> OnUpdateBatteryStatusEvent;
        event EventHandler<bool> OnUpdateChargeStatusEvent;

        void SetPercentageTo(int percentage);
        void StopCharge();
        void StartCharge(EnumAddressDirection chargeDirection);
        void GetBatteryAndChargeStatus();
    }
}
