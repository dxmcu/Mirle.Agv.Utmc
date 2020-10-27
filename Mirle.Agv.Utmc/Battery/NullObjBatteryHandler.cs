using Mirle.Agv.Utmc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Battery
{
    public class NullObjBatteryHandler : IBatteryHandler
    {
        public event EventHandler<BatteryStatus> OnUpdateBatteryStatusEvent;
        public event EventHandler<bool> OnUpdateChargeStatusEvent;

        public BatteryStatus BatteryStatus { get; set; }
        public bool IsCharging { get; set; }

        public NullObjBatteryHandler(BatteryStatus batteryStatus)
        {
            this.BatteryStatus = batteryStatus;
            IsCharging = false;
        }

        public void SetPercentageTo(int percentage)
        {
            BatteryStatus.Percentage = percentage;
            OnUpdateBatteryStatusEvent?.Invoke(this, BatteryStatus);
        }

        public void StartCharge(EnumAddressDirection chargeDirection)
        {
            Task.Run(() =>
            {
                try
                {
                    System.Threading.Thread.Sleep(2000);
                    IsCharging = true;
                    OnUpdateChargeStatusEvent?.Invoke(this, IsCharging);

                    while (BatteryStatus.Percentage < 100 && IsCharging)
                    {
                        System.Threading.Thread.Sleep(2000);

                        BatteryStatus.Percentage = Math.Min(BatteryStatus.Percentage + 10, 100);

                        OnUpdateBatteryStatusEvent?.Invoke(this, BatteryStatus);
                    }
                }
                catch (Exception) { }

                System.Threading.Thread.Sleep(2000);

                IsCharging = false;
                OnUpdateChargeStatusEvent?.Invoke(this, IsCharging);
            });
        }

        public void StopCharge()
        {
            Task.Run(() =>
            {
                if (IsCharging)
                {
                    System.Threading.Thread.Sleep(2000);
                    IsCharging = false;
                }
            });
        }

        public void GetBatteryAndChargeStatus()
        {
            OnUpdateBatteryStatusEvent?.Invoke(this, BatteryStatus);
            OnUpdateChargeStatusEvent?.Invoke(this, IsCharging);
        }
    }
}
