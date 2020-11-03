using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class ObjectData
    {
        public string Name { get; set; } = "";

        public List<MapPosition> PositionList { get; set; } = new List<MapPosition>();
    }
}
