using Mirle.Agv.Utmc;
using Mirle.Agv.Utmc.Controller;
using Mirle.Agv.Utmc.Model;
using Mirle.Agv.Utmc.Model.TransferSteps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Robot
{
    public class NullObjRobotHandler : IRobotHandler
    {
        public event EventHandler<CarrierSlotStatus> OnUpdateCarrierSlotStatusEvent;
        public event EventHandler<RobotStatus> OnUpdateRobotStatusEvent;
        public event EventHandler<EnumRobotEndType> OnRobotEndEvent;

        public RobotStatus RobotStatus { get; set; }
        public CarrierSlotStatus CarrierSlotStatus { get; set; }

        public NullObjRobotHandler(RobotStatus robotStatus, CarrierSlotStatus carrierSlotStatus)
        {
            this.RobotStatus = robotStatus;
            this.CarrierSlotStatus = carrierSlotStatus;
        }

        public void ClearRobotCommand()
        {

        }

        public void DoRobotCommandFor(RobotCommand robotCommand)
        {
            Task.Run(() =>
            {
                if (robotCommand.GetTransferStepType() == EnumTransferStepType.Load)
                {
                    RobotStatus = new RobotStatus() { EnumRobotState = EnumRobotState.Busy, IsHome = false };

                    OnUpdateRobotStatusEvent?.Invoke(this, RobotStatus);

                    System.Threading.Thread.Sleep(2000);

                    CarrierSlotStatus = new CarrierSlotStatus() { CarrierId = robotCommand.CassetteId, EnumCarrierSlotState = EnumCarrierSlotState.Loading, SlotNumber = robotCommand.SlotNumber };

                    OnUpdateCarrierSlotStatusEvent?.Invoke(this, CarrierSlotStatus);

                    System.Threading.Thread.Sleep(2000);

                    RobotStatus = new RobotStatus() { EnumRobotState = EnumRobotState.Idle, IsHome = true };

                    OnUpdateRobotStatusEvent?.Invoke(this, RobotStatus);

                    OnRobotEndEvent?.Invoke(this, EnumRobotEndType.Finished);
                }
                else if (robotCommand.GetTransferStepType() == EnumTransferStepType.Unload)
                {
                    RobotStatus = new RobotStatus() { EnumRobotState = EnumRobotState.Busy, IsHome = false };

                    OnUpdateRobotStatusEvent?.Invoke(this, RobotStatus);

                    System.Threading.Thread.Sleep(2000);

                    CarrierSlotStatus = new CarrierSlotStatus() { CarrierId = "", EnumCarrierSlotState = EnumCarrierSlotState.Empty, SlotNumber = robotCommand.SlotNumber };

                    OnUpdateCarrierSlotStatusEvent?.Invoke(this, CarrierSlotStatus);

                    System.Threading.Thread.Sleep(2000);

                    RobotStatus = new RobotStatus() { EnumRobotState = EnumRobotState.Idle, IsHome = true };

                    OnUpdateRobotStatusEvent?.Invoke(this, RobotStatus);

                    OnRobotEndEvent?.Invoke(this, EnumRobotEndType.Finished);
                }
            });
        }

        public void GetRobotAndCarrierSlotStatus()
        {
            OnUpdateRobotStatusEvent?.Invoke(this, RobotStatus);
            OnUpdateCarrierSlotStatusEvent?.Invoke(this, CarrierSlotStatus);
        }

        public void SetCarrierSlotStatusTo(CarrierSlotStatus carrierSlotStatus)
        {
            CarrierSlotStatus = carrierSlotStatus;
            OnUpdateCarrierSlotStatusEvent?.Invoke(this, CarrierSlotStatus);
        }
    }
}
