using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model.Configs
{
    public class BarcodeMapSystemConfig
    {
        public List<LocateDriver_SR2000Config> BarcodeReaderConfigs { get; set; } = new List<LocateDriver_SR2000Config>();
        public bool LogMode { get; set; } = false;
        public int SleepTime { get; set; } = 5;
        public int TimeoutValue { get; set; } = 60;
        public int BarcodeIDInterval { get; set; } = 3;
        public double BarcodeDistanceInterval { get; set; } = 10;
        public double DistanceTolerance { get; set; } = 0.05;
    }
}
