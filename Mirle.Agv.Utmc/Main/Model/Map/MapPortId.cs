﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Model
{
    public class MapPort
    {
        public string ID { get; set; } = "";
        public string ReferenceAddressId { get; set; } = "";
        public string Number { get; set; } = "1";
        public bool IsVitualPort { get; set; } = false;
    }
}
