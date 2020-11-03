using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  Mirle.Agv.INX.Model
{
    public class SimulateMoveVelocityData
    {
        public double StartVelocity { get; set; }
        public bool DirFlag { get; set; }
        public AxisData Axis { get; set; }
        public AxisData Command { get; set; }
        public SimulateData[] SimulateDataList { get; set; }
        public DateTime Time { get; set; }
        public double TargetVelocity { get; set; }
    }
}
