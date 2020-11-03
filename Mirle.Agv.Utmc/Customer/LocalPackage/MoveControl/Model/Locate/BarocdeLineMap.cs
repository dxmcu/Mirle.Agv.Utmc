using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class BarocdeLineMap
    {
        public string ID { get; set; } = "";
        public int Start_Number { get; set; } = 0;
        public MapPosition Start_Position { get; set; } = new MapPosition();
        public int End_Number { get; set; } = 0;
        public MapPosition End_Position { get; set; } = new MapPosition();
        public EnumAGVPositionType Type { get; set; } = EnumAGVPositionType.Normal;

        public BarocdeLineMap()
        {
        }

        public BarocdeLineMap(string id, int start_Number, MapPosition start_Position, int end_Number, MapPosition end_Position, EnumAGVPositionType type)
        {
            ID = id;
            Start_Number = start_Number;
            Start_Position = start_Position;
            End_Number = end_Number;
            End_Position = end_Position;
            Type = type;
        }
    }
}
