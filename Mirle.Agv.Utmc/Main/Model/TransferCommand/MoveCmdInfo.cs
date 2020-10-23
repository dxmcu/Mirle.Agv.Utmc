using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Utmc.Controller;

using Mirle.Agv.Utmc.Model.Configs;
using Mirle.Tools;

namespace Mirle.Agv.Utmc.Model.TransferSteps
{
    public class MoveCmdInfo : TransferStep
    {
        public MapAddress EndAddress { get; set; } = new MapAddress();

        public MoveCmdInfo(MapAddress endAddress, string cmdId) : base(cmdId)
        {
            type = EnumTransferStepType.Move;
            this.EndAddress = endAddress;
        }
    }

    public class MoveToChargerCmdInfo : MoveCmdInfo
    {
        public MoveToChargerCmdInfo(MapAddress endAddress, string cmdId) : base(endAddress, cmdId)
        {
            type = EnumTransferStepType.MoveToCharger;
        }
    }
}
