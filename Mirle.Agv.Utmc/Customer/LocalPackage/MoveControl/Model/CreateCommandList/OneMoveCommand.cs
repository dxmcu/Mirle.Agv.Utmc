using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  Mirle.Agv.INX.Model
{
    public class OneceMoveCommand
    {
        public List<MapPosition> PositionList { get; set; }
        public List<double> AGVAngleList { get; set; }
        public List<string> AddressActions { get; set; }
        public List<double> SpeedList { get; set; }
        
        public OneceMoveCommand( )
        {
            PositionList = new List<MapPosition>();
            AddressActions = new List<string>();
            SpeedList = new List<double>();
            AGVAngleList = new List<double>();
        }
    }
}
