using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class SLAMTransferData
    {
        public List<SLAMTransfer> TransferList { get; set; } = new List<SLAMTransfer>();
        public int Index { get; set; } = 0;
        public bool DirFlag { get; set; }
    }
}
