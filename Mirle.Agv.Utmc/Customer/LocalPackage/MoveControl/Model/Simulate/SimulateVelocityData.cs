using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class SimulateVelocityData
    {
        public AxisData Axis { get; set; }
        public AxisData Command { get; set; }
        public List<SimulateData> SimulateDataList { get; set; } = new List<SimulateData>();
        public DateTime Time { get; set; }
    }
}
