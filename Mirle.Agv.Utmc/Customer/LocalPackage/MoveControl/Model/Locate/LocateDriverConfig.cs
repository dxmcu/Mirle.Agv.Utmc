using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class LocateDriverConfig
    {
        public string Path { get; set; } 
        public EnumLocateDriverType LocateDriverType { get; set; } = EnumLocateDriverType.None;
        public int Order { get; set; } = 0;
        public string Device { get; set; }
    }
}
