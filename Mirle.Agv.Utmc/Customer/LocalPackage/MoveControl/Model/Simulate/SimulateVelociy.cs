using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class SimulateVelociy
    {
        public bool DirFlag { get; set; }
        public AxisData Axis { get; set; }

        public AxisData Command { get; set; }
        public SimulateData[] SimulateDataList { get; set; }
        public DateTime Time { get; set; }
    }
}
