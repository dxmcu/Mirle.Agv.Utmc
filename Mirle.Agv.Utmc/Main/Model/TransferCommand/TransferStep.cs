using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Utmc.Controller;
using Mirle.Agv.Utmc.Model.Configs;

namespace Mirle.Agv.Utmc.Model.TransferSteps
{

    public abstract class TransferStep
    {
        protected EnumTransferStepType type = EnumTransferStepType.Empty;
        public string CmdId { get; set; } = "";

        public TransferStep(string cmdId)
        {
            this.CmdId = cmdId;
        }

        public EnumTransferStepType GetTransferStepType() { return type; }
    }
}
