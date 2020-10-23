using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Model
{
    public class PositionArgs : EventArgs
    {
        public EnumLocalArrival EnumLocalArrival { get; set; } = EnumLocalArrival.Fail;
        public MapPosition MapPosition { get; set; } = new MapPosition();
        public int HeadAngle { get; set; } = 0;
        public int MovingDirection { get; set; } = 0;
        public int Speed { get; set; } = 0;
    }
}
