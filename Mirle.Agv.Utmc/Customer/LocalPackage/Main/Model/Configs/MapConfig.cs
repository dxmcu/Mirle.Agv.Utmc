using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model.Configs
{
    [Serializable]
    public class MapConfig
    {
        public string FileDirectory { get; set; } = "";
        public string AddressCSVFileName { get; set; } = "";
        public string SectionCSVFileName { get; set; } = "";
        public string ObjectFileName { get; set; } = "Object.csv";
        public double MapScale { get; set; } = 0.05;
        public double MapBorderLength { get; set; } = 2000;
        public int AddressWidth { get; set; } = 10;
        public int AddressLineWidth { get; set; } = 3;
    }
}
