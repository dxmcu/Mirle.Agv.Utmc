﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Model.Configs
{
    public class PspConnectionConfig
    {
        public string Ip { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 5000;
        public bool IsServer { get; set; } = true;
        public string SpecVersion { get; set; } = "1.0";
        public int T3Timeout { get; set; } = 10;
        public int T6Timeout { get; set; } = 10;
        public int LinkTestIntervalMs { get; set; } = 30 * 1000;
    }
}
