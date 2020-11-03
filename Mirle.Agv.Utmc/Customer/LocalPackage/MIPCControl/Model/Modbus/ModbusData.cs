using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class ModbusData
    {
        public UInt16 SeqNumber { get; set; } = 0;
        public UInt16 StationNo { get; set; } = 1;
        public UInt16 FunctionCode { get; set; } = 1;
        public UInt16 DataLength { get; set; } = 1;
        public UInt32 StartAddress { get; set; } = 1;

        public Byte[] DataBuffer { get; set; } = new byte[0];

        public Byte[] ByteData { get; set; }

        public int RecieveLength { get; set; }

        public void InitialByteData()
        {
            Byte[] seqNumberArray = BitConverter.GetBytes(SeqNumber);
            Byte[] stationNoArray = BitConverter.GetBytes(StationNo);
            Byte[] functionCodeArray = BitConverter.GetBytes(FunctionCode);
            Byte[] dataLengthArray = BitConverter.GetBytes(DataLength);
            Byte[] startAddressArray = BitConverter.GetBytes(StartAddress);
            
            ByteData = new Byte[12 + DataBuffer.Length];
            seqNumberArray.CopyTo(ByteData, 0);
            stationNoArray.CopyTo(ByteData, 2);
            functionCodeArray.CopyTo(ByteData, 4);
            dataLengthArray.CopyTo(ByteData, 6);
            startAddressArray.CopyTo(ByteData, 8);
            
            if (DataBuffer.Length != 0)
                DataBuffer.CopyTo(ByteData, 12);
        }

        public void GetDataByByteData()
        {
            SeqNumber = BitConverter.ToUInt16(ByteData, 0);
            StationNo = BitConverter.ToUInt16(ByteData, 2);
            FunctionCode = BitConverter.ToUInt16(ByteData, 4);
            DataLength = BitConverter.ToUInt16(ByteData, 6);
            StartAddress = BitConverter.ToUInt32(ByteData, 8);
        }
    }
}