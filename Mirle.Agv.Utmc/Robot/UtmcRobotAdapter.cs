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
    public class UtmcRobotAdapter : IRobotHandler
    {
        public event EventHandler<CarrierSlotStatus> OnUpdateCarrierSlotStatusEvent;
        public event EventHandler<RobotStatus> OnUpdateRobotStatusEvent;
        public event EventHandler<EnumRobotEndType> OnRobotEndEvent;

        public void ClearRobotCommand()
        {
            
        }

        public void DoRobotCommandFor(RobotCommand robotCommand)
        {
            
        }

        public void GetRobotAndCarrierSlotStatus()
        {
            
        }

        public void SetCarrierSlotStatusTo(CarrierSlotStatus carrierSlotStatus)
        {
           
        }
    }
}
