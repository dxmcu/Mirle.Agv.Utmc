using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Model
{
    public class CarrierSlotStatus
    {
        public EnumCarrierSlotState EnumCarrierSlotState { get; set; } = EnumCarrierSlotState.Empty;
        public string CarrierId { get; set; } = "";
        public EnumSlotNumber SlotNumber { get; set; } = EnumSlotNumber.L;
        public bool ManualDeleteCST { get; set; } = false;

        public CarrierSlotStatus() { }

        public CarrierSlotStatus(EnumSlotNumber slotNumber)
        {
            this.SlotNumber = slotNumber;
        }

        public CarrierSlotStatus(CarrierSlotStatus carrierSlotStatus)
        {
            this.EnumCarrierSlotState = carrierSlotStatus.EnumCarrierSlotState;
            this.CarrierId = carrierSlotStatus.CarrierId;
            this.SlotNumber = carrierSlotStatus.SlotNumber;
            this.ManualDeleteCST = carrierSlotStatus.ManualDeleteCST;
        }
    }
}
