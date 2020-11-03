using Mirle.Agv.INX.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class MIPCPollingData
    {
        public UInt32 StartAddress { get; set; }
        public UInt32 EndAddress { get; set; }
        public UInt16 Length { get; set; }

        private bool firstUpdate = true;

        public int GroupNumber { get; set; }

        public void UpdateData(MIPCData data)
        {
            UInt32 startAddress = data.Address;
            UInt32 endAddress = data.Address + (data.ByteNumber * 4 + data.BitNumber + data.Length - 1) / 32;

            if (firstUpdate)
            {
                firstUpdate = false;
                StartAddress = startAddress;
                EndAddress = endAddress;
                GroupNumber = data.PollingGroup;
            }
            else
            {
                if (startAddress < StartAddress)
                    StartAddress = startAddress;
                else if (endAddress > EndAddress)
                    EndAddress = endAddress;
            }
        }

        public void InitialPollingData()
        {
            Length = (UInt16)(EndAddress - StartAddress + 1);
        }
    }
}
