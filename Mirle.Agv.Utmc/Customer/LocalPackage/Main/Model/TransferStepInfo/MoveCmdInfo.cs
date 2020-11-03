using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.INX.Controller;
using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Model.Configs;

namespace Mirle.Agv.INX.Model
{
    [Serializable]
    public class MoveCmdInfo
    {
        public string CommandID { get; set; } = "";
        public List<MapSection> MovingSections { get; set; } = new List<MapSection>();
        public List<MapAddress> MovingAddress = new List<MapAddress>();
        public List<double> SpecifySpeed = new List<double>();
        public MapAddress EndAddress { get; set; } = new MapAddress();
        public MapAddress StartAddress { get; set; } = new MapAddress();
        public List<string> AddressActions { get; set; } = new List<string>();
        public EnumStageDirection StageDirection { get; set; } = EnumStageDirection.None;
        public bool IsMoveEndDoLoadUnload { get; set; } = false;

        public bool IsAutoCommand { get; set; } = false;
    }
}