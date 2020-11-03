using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Configs
{
    public class MIPCData
    {
        public string DataName { get; set; } = "";
        public UInt32 Address { get; set; } = 0;
        public uint ByteNumber { get; set; } = 0;
        public uint BitNumber { get; set; } = 0;
        public uint Length { get; set; } = 1; // bits
        public EnumDataType DataType { get; set; } = EnumDataType.Boolean;
        public EnumIOType IoStatus { get; set; } = EnumIOType.Read;
        public string IPCName { get; set; } = "";
        public string Classification { get; set; } = "";
        public int PollingGroup { get; set; } = 0;
        public string Description { get; set; } = "";
        public string Value { get; set; } = "";
        public object Object { get; set; }
        public object LastObject { get; set; }
    }
}
