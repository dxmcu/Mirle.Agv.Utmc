using Mirle.Agv.Utmc.Model;
using Mirle.Agv.Utmc.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Robot
{
     interface IRobotHandler : IMessageHandler
    {
         event EventHandler<Model.CarrierSlotStatus> OnUpdateCarrierSlotStatusEvent;
         event EventHandler<Model.RobotStatus> OnUpdateRobotStatusEvent;
         event EventHandler<EnumRobotEndType> OnRobotEndEvent;

         void DoRobotCommandFor(Model.TransferSteps.RobotCommand robotCommand);
         void ClearRobotCommand();
         void GetRobotAndCarrierSlotStatus();
         void SetCarrierSlotStatusTo(Model.CarrierSlotStatus carrierSlotStatus);
    }
}
