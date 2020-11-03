using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class LoadUnloadControlData
    {
        public EnumCommandStatus CommandStatus { get; set; } = EnumCommandStatus.Idle;

        public string CommnadID { get; set; } = "";
        public bool ForkHome { get; set; } = false;
        public bool Ready { get; set; } = true;
        public bool ErrorBit { get; set; } = false;

        public bool Loading { get; set; } = false;
        public string CstID { get; set; } = "";
    }
}
