using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class BarcodeReaderData
    {
        public string ReceivedData { get; set; }

        public BarcodeData Barcode1 { get; set; } = null;
        public BarcodeData Barcode2 { get; set; } = null;

        public MapPosition TargetCenter { get; set; } = null;
        public LocateAGVPosition LocateData { get; set; } = null;

        public int ScanTime { get; set; }
        public DateTime GetDataTime { get; set; }
        public uint Count { get; set; }

        public bool DataNotMatch { get; set; } = false;
        public EnumBarcodeReaderType Type { get; set; } = EnumBarcodeReaderType.None;

        public BarcodeReaderData(string data, EnumBarcodeReaderType type, uint count)
        {
            try
            {
                string[] splitResult;
                Count = count;
                ReceivedData = data;
                Type = type;

                switch (type)
                {
                    case EnumBarcodeReaderType.Keyence:
                        splitResult = Regex.Split(data, "[: / ,]+", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));

                        if (splitResult.Length == 7)
                        {
                            GetDataTime = DateTime.Now;
                            Barcode1 = new BarcodeData(Int32.Parse(Regex.Replace(splitResult[0], "[^0-9]", "")), double.Parse(splitResult[1]), double.Parse(splitResult[2]));
                            Barcode2 = new BarcodeData(Int32.Parse(Regex.Replace(splitResult[3], "[^0-9]", "")), double.Parse(splitResult[4]), double.Parse(splitResult[5]));

                            ScanTime = Int32.Parse(Regex.Replace(splitResult[6], "[^0-9]", ""));
                            Count = count;
                        }
                        else
                            DataNotMatch = true;

                        break;

                    case EnumBarcodeReaderType.Datalogic:
                        splitResult = Regex.Split(data, ",", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));

                        if (splitResult.Length == 7)
                        {
                            GetDataTime = DateTime.Now;
                            Barcode1 = new BarcodeData(Int32.Parse(Regex.Replace(splitResult[0], "[^0-9]", "")), double.Parse(splitResult[1]), double.Parse(splitResult[2]));
                            Barcode2 = new BarcodeData(Int32.Parse(Regex.Replace(splitResult[3], "[^0-9]", "")), double.Parse(splitResult[4]), double.Parse(splitResult[5]));

                            ScanTime = Int32.Parse(Regex.Replace(splitResult[6], "[^0-9]", ""));
                            Count = count;
                        }
                        else
                            DataNotMatch = true;
                        break;

                    case EnumBarcodeReaderType.None:
                    default:
                        DataNotMatch = true;
                        break;
                }
            }
            catch
            {
                DataNotMatch = true;
            }
        }
    }
}
