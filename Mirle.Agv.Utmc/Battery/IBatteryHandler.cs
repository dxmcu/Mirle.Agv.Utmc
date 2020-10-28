using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Battery
{
    public interface IBatteryHandler : Tools.IMessageHandler
    {
        public event EventHandler<Model.BatteryStatus> OnUpdateBatteryStatusEvent;
        public event EventHandler<bool> OnUpdateChargeStatusEvent;

        public void SetPercentageTo(int percentage);
        public void StopCharge();
        public void StartCharge(EnumAddressDirection chargeDirection);
        public void GetBatteryAndChargeStatus();
    }
}
