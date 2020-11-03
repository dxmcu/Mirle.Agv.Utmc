using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class ReserveData
    {
        public MapAGVPosition SectionEnd { get; set; }
        public bool GetReserve { get; set; }
        public string SectionID { get; set; }

        public ReserveData(string sectionID, MapAGVPosition sectionEnd)
        {
            SectionEnd = sectionEnd;
            GetReserve = false;
            SectionID = sectionID;
        }
    }
}
