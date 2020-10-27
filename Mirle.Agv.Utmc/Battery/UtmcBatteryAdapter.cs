using Mirle.Agv.Utmc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Battery
{
    public class UtmcBatteryAdapter : IBatteryHandler
    {
        public event EventHandler<BatteryStatus> OnUpdateBatteryStatusEvent;
        public event EventHandler<bool> OnUpdateChargeStatusEvent;

        public void GetBatteryAndChargeStatus()
        {
        }

        public void SetPercentageTo(int percentage)
        {
        }

        public void StartCharge(EnumAddressDirection chargeDirection)
        {
        }

        public void StopCharge()
        {
        }
    }
}
