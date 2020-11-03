using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class DecompositionCommandData
    {
        public double AGVMovingAngle { get; set; } = 0;

        public double StartEncoder { get; set; } = 0;

        public double StartByPassDistance { get; set; } = 0;
        public double EndByPassDistance { get; set; } = 0;

        public double AGVAngleInMap { get; set; } = 0;

        public MapAddress StartAddress { get; set; }
        public MapAddress LastAddress { get; set; }
        public MapAddress NowAddress { get; set; }
        public MapAddress NextAddress { get; set; }
        public MapSection NowSection { get; set; }
        
        public List<MapAGVPosition> LineEndAGVPositionList { get; set; } = new List<MapAGVPosition>();

        public MapAGVPosition TempEndAGVPosition { get; set; }

        public int Index { get; set; } = 0;
        public string TurnType { get; set; } = "";

        public double NowVelocityCommand { get; set; } = 0;
        public double NowVelocity { get; set; } = 0;

        public double TotalDistance { get; set; } = 0;
        public double LineDistance { get; set; } = 0;

        public int StartMoveIndex { get; set; } = 0;

        public double TurnOutDistance { get; set; } = 0;
    }
}
