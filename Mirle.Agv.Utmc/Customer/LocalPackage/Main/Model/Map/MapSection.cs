using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    [Serializable]
    public class MapSection
    {
        public string Id { get; set; } = "";
        public MapAddress FromAddress { get; set; } = new MapAddress();
        public MapAddress ToAddress { get; set; } = new MapAddress();
        public double Distance { get; set; }
        public List<MapSection> NearbySection { get; set; } = new List<MapSection>();
        public double Speed { get; set; }

        public double FromVehicleAngle { get; set; } = 0;
        public double ToVehicleAngle { get; set; } = 0;

        public double CosTheta { get; set; } = 0;
        public double SinTheta { get; set; } = 0;
        public double SectionAngle { get; set; } = 0;
    }
}
