using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    [Serializable]
    public class MapInfo
    {
        public Dictionary<string, MapAddress> AllAddress = new Dictionary<string, MapAddress>();
        public Dictionary<string, MapSection> AllSection = new Dictionary<string, MapSection>();
        public List<ObjectData> ObjectDataList = new List<ObjectData>();
    }
}
