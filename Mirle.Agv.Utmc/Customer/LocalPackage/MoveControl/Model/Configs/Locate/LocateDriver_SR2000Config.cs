using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model.Configs
{
    public class LocateDriver_SR2000Config
    {
        public string ID { get; set; }
        public string IpOrComport { get; set; }
        public EnumBarcodeReaderType BarcodeReaderType { get; set; }
        public double ReaderToCenterDegree { get; set; }
        public double ReaderToCenterDistance { get; set; }
        public double ReaderSetupAngle { get; set; }
        public MapPosition ViewCenter { get; set; }
        public MapAGVPosition Offset { get; set; }
        public MapPosition Target { get; set; }
        public MapPosition Change { get; set; }
        public double Up { get; set; }
        public double Down { get; set; }
    }
}
