using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    [Serializable]
    public class MapAddress
    {
        public string Id { get; set; } = "FakeAddress";

        public MapAGVPosition AGVPosition { get; set; } = new MapAGVPosition();

        public bool CanSpin { get; set; } = false;
        public string SpecialTurn { get; set; } = "";
        public string InsideSectionId { get; set; } = "";

        public MapSection InsideSection { get; set; } = null;
        public List<MapSection> NearbySection { get; set; } = new List<MapSection>();
    }

}

