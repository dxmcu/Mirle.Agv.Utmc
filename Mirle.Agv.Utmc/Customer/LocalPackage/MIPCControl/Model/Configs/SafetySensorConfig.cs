using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class SafetySensorConfig
    {
        public List<SafetySensorData> SafetySensorList { get; set; } = new List<SafetySensorData>();
    }
}
