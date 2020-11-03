using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class SLAMTransfer
    {
        public MapPosition Step1Offset { get; set; }
        public double Step2Cos { get; set; }
        public double Step2Sin { get; set; }
        public double Step3Mag { get; set; }
        public MapPosition Step4Offset { get; set; }
        public double ThetaOffset { get; set; }

        public double Distance { get; set; }

        public double SinTheta { get; set; }
        public double CosTheta { get; set; }

        public double ThetaOffsetStart { get; set; }
        public double ThetaOffsetEnd { get; set; }
    }
}
