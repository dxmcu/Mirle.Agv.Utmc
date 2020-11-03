using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class LocateControlData
    {
        public LocateAGVPosition LocateAGVPosition { get; set; } = null;
        //public SectionLine ReviseSectionLine { get; set; } = null;

        public string TargetSection { get; set; } = "";
        //public string NowTagName { get; set; } = "";
        //public string LastTagName { get; set; } = "";

        public LocateAGVPosition SelectOrder { get; set; } = null;
        //public List<AGVPosition> NowAGVPositionList { get; set; } = null;
    }
}
