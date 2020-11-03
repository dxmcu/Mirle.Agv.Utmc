using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class SectionLineTransferData
    {
        public List<SLAMTransferData> TransferDataLine { get; set; } = new List<SLAMTransferData>();
        public int Index { get; set; } = 0;
    }
}
