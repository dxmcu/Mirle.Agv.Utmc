using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  Mirle.Agv.INX.Model
{
    public class LocateControlConfig
    {
        public double BarcodeAllowAngleRange { get; set; } = 10;
        public double OrderLowerDelay { get; set; } = 3000;
        public List<LocateDriverConfig> Driver { get; set; } = new List<LocateDriverConfig>();
    }
}
