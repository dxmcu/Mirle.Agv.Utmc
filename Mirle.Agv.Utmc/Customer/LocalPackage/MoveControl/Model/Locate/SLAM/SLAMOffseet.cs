using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class SLAMOffseet
    {
        public double ThetaOffset { get; set; } = 0;
        public double Polar_Theta { get; set; } = 0;
        public double Polar_Distance { get; set; } = 0;

        public SLAMOffseet(double thetaOffset, double polar_Theta, double polar_Distance)
        {
            ThetaOffset = thetaOffset;
            Polar_Theta = polar_Theta;
            Polar_Distance = polar_Distance;
        }
    }
}
