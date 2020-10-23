﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Model
{

    public class BatteryLog
    {
        public long MoveDistanceTotalM { get; set; }
        public int LoadUnloadCount { get; set; }
        public int ChargeCount { get; set; }
        public string ResetTime { get; set; }
        public int InitialSoc { get; set; } = 70;
    }
}
