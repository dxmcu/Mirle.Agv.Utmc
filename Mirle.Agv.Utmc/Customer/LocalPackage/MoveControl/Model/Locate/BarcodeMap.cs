using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class BarcodeMap
    {
        public Dictionary<int, BarcodeDataInMap> AllBarcode { get; set; } = new Dictionary<int, BarcodeDataInMap>();
        public Dictionary<string, BarocdeLineMap> AllBarcodeLine { get; set; } = new Dictionary<string, BarocdeLineMap>();
    }
}
