using Mirle.Agv.Utmc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Controller
{
    public interface IRobotHandler
    {
        public event EventHandler<Model.CarrierSlotStatus> OnUpdateCarrierSlotStatusEvent;
        public event EventHandler<Model.RobotStatus> OnUpdateRobotStatusEvent;
        public event EventHandler<EnumRobotEndType> OnRobotEndEvent;

        public void DoRobotCommand(Model.TransferSteps.RobotCommand robotCommand);
        public void ClearRobotCommand();
        public void RefreshRobotAndCarrierSlotState();
    }
}
