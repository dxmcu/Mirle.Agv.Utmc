using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model.Configs
{
    public class MIPCPortData
    {
        public int PortNumber { get; set; } = -1;
        public EnumMIPCConnectType ConnectType { get; set; } = EnumMIPCConnectType.None;
        public string SocketName { get; set; } = "";
    }
}
