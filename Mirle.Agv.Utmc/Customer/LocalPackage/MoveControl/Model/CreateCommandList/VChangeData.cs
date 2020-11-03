using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  Mirle.Agv.INX.Model
{
    public class VChangeData
    {
        public double StartEncoder { get; set; }
        public double StartVelocity { get; set; }
        public double EndEncoder { get; set; }
        public double VelocityCommand { get; set; }
        public double BufferTime { get; set; }
        public EnumVChangeType Type { get; set; }

        public VChangeData()
        {
            StartEncoder = 0;
            StartVelocity = 0;
            EndEncoder = 0;
            VelocityCommand = 0;
            BufferTime = 0;
            Type = EnumVChangeType.Normal;
        }

        public VChangeData(double startEncoder, double startVelocity, double endEncoder, double velocityCommand, object type)
        {
            StartEncoder = startEncoder;
            StartVelocity = startVelocity;
            EndEncoder = endEncoder;
            VelocityCommand = velocityCommand;
            Type = (EnumVChangeType)type;
        }
    }
}
