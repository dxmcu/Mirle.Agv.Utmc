using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    [Serializable]
    public class MapAGVPosition
    {
        public double Angle { get; set; } = 0;
        public MapPosition Position { get; set; } = new MapPosition();

        public MapAGVPosition(MapAGVPosition old)
        {
            Angle = old.Angle;
            Position = new MapPosition(old.Position.X, old.Position.Y);
        }

        public MapAGVPosition(MapPosition position, double theta)
        {
            Position = position;
            Angle = theta;
        }

        public MapAGVPosition()
        {
        }
    }
}
