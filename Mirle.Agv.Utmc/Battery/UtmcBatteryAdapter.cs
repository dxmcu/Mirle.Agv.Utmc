﻿using Mirle.Agv.Utmc.Customer;
using Mirle.Agv.Utmc.Model;
using Mirle.Agv.Utmc.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Battery
{
    public class UtmcBatteryAdapter : IBatteryHandler
    {
        public event EventHandler<BatteryStatus> OnUpdateBatteryStatusEvent;
        public event EventHandler<bool> OnUpdateChargeStatusEvent;
        public event EventHandler<MessageHandlerArgs> OnLogDebugEvent;
        public event EventHandler<MessageHandlerArgs> OnLogErrorEvent;

        public LocalPackage LocalPackage { get; set; }

        public UtmcBatteryAdapter(LocalPackage localPackage)
        {
            this.LocalPackage = localPackage;
            var batteryConfig = LocalPackage.MainFlowHandler.localData.MIPCData.Config;
            Vehicle.Instance.MainFlowConfig.HighPowerPercentage = (int)batteryConfig.ChargingSOC_High;
        }

        public void GetBatteryAndChargeStatus()
        {
            var localData = LocalPackage.MainFlowHandler.localData;

            OnUpdateBatteryStatusEvent?.Invoke(this, new BatteryStatus()
            {
                Percentage = (int)localData.BatteryInfo.SOC,
                Voltage = localData.BatteryInfo.V
            });

            OnUpdateChargeStatusEvent?.Invoke(this, localData.MIPCData.Charging);
        }

        public void SetPercentageTo(int percentage)
        {
            //TODO : LocalPackage.MainFlowHandler.SetPercentageTo(percentage);
        }

        public void StartCharge(EnumAddressDirection chargeDirection)
        {
            if (!LocalPackage.MainFlowHandler.localData.MIPCData.Charging)
            {
                Task.Run(() =>
                {
                    LocalPackage.MainFlowHandler.StartChargingByAddressID(Vehicle.Instance.MoveStatus.LastAddress.Id);
                    SpinWait.SpinUntil(() => LocalPackage.MainFlowHandler.localData.MIPCData.Charging, 30 * 1000);
                    OnUpdateChargeStatusEvent?.Invoke(this, LocalPackage.MainFlowHandler.localData.MIPCData.Charging);
                });
            }
            else
            {
                OnUpdateChargeStatusEvent?.Invoke(this, LocalPackage.MainFlowHandler.localData.MIPCData.Charging);
            }
        }

        public void StopCharge()
        {
            if (LocalPackage.MainFlowHandler.localData.MIPCData.Charging)
            {
                Task.Run(() =>
                {
                    LocalPackage.MainFlowHandler.StopCharging();
                    SpinWait.SpinUntil(() => !LocalPackage.MainFlowHandler.localData.MIPCData.Charging, 30 * 1000);
                    OnUpdateChargeStatusEvent?.Invoke(this, LocalPackage.MainFlowHandler.localData.MIPCData.Charging);
                });
            }
            else
            {
                OnUpdateChargeStatusEvent?.Invoke(this, LocalPackage.MainFlowHandler.localData.MIPCData.Charging);
            }
        }
    }
}
