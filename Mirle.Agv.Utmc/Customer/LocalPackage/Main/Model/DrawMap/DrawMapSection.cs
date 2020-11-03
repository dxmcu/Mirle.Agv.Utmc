using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class DrawMapSection
    {
        public EnumSectionAction Type { get; set; } = EnumSectionAction.None;
        public float X1 { get; set; } = 0;
        public float Y1 { get; set; } = 0;
        public float X2 { get; set; } = 0;
        public float Y2 { get; set; } = 0;
    }
}
