using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    [Serializable]
    public class MapPosition
    {
        public double X { get; set; }
        public double Y { get; set; }

        public MapPosition(double x, double y)
        {
            X = x;
            Y = y;
        }

        public MapPosition()
        {
            X = 0;
            Y = 0;
        }
    }
}
