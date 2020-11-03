using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  Mirle.Agv.INX.Controller.Tools
{
    [Serializable]
    public class LogBasicConfigs
    {       
        public int Number { get; set; }
        public string SectionBaseName { get; set; } = "CategoryType";
    }
}
