﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Model.Configs
{

    public class InitialConfig
    {
        public int StartOkShowMs { get; set; } = 1000;
        public int StartNgCloseSec { get; set; } = 30;
    }
}
