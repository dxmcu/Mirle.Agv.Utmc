using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class TimeStampData
    {
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public DateTime Time { get; set; }
        public DateTime SendTime { get; set; }
        public DateTime GetTime { get; set; }
    }
}
