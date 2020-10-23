﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Utmc.Controller;

namespace Mirle.Agv.Utmc.Model.TransferSteps
{

    public class LoadCmdInfo : RobotCommand
    {
        public LoadCmdInfo(AgvcTransferCommand transferCommand) : base(transferCommand)
        {
            this.type = EnumTransferStepType.Load;
            this.PortAddressId = transferCommand.LoadAddressId;
        }    
    }
}
