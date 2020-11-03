﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class ThetaSectionDeviation
    {
        public double Theta { get; set; }
        public double SectionDeviation { get; set; }

        public ThetaSectionDeviation(double theta, double sectionDeviation)
        {
            Theta = theta;
            SectionDeviation = sectionDeviation;
        }

        public ThetaSectionDeviation()
        {
            Theta = 0;
            SectionDeviation = 0;
        }
    }
}
