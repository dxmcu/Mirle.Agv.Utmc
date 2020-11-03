using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class SectionLine
    {
        public MapSection Section { get; set; }

        public MapAddress Start { get; set; }
        public MapAddress End { get; set; }
        public double Distance { get; set; }

        public double EncoderAddSectionDistanceStart { get; set; }
        public bool SectionDirFlag { get; set; }

        public double EncoderStart { get; set; }
        public double EncoderEnd { get; set; }

        public double Angle { get; set; }
        public double CosTheta { get; set; }
        public double SinTheta { get; set; }

        public SectionLine(MapSection section, MapAddress start, MapAddress end, double angle, double distance, double encoderAddSectionDistanceStart, bool sectionDirFlag, double encoderStart)
        {
            Section = section;
            Start = start;
            End = end;
            Distance = distance;
            EncoderAddSectionDistanceStart = encoderAddSectionDistanceStart;
            SectionDirFlag = sectionDirFlag;
            EncoderStart = encoderStart;
            EncoderEnd = encoderStart + distance;
            Angle = angle;
            CosTheta = Math.Cos(angle / 180 * Math.PI);
            SinTheta = Math.Sin(angle / 180 * Math.PI);
        }
    }
}
