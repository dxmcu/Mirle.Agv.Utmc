﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Model.Configs
{

    public class MainFlowConfig
    {
        public int VisitTransferStepsSleepTimeMs { get; set; } = 50;
        public int TrackPositionSleepTimeMs { get; set; } = 5000;
        public int WatchLowPowerSleepTimeMs { get; set; } = 5000;
        public int StartChargeWaitingTimeoutMs { get; set; } = 30 * 1000;
        public int StopChargeWaitingTimeoutMs { get; set; } = 30 * 1000;
        public int RealPositionRangeMm { get; set; }
        public int ChargeIntervalInRobotingSec { get; set; } = -1;
        public bool IsSimulation { get; set; }
        public bool DualCommandOptimize { get; set; } = false;
        public bool TripleCommandSwap { get; set; } = false;
        public bool BcrByPass { get; set; } = false;
        public int HighPowerPercentage { get; set; } = 90;
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public EnumSlotSelect SlotDisable { get; set; } = EnumSlotSelect.None;
        public int InitialPositionRangeMm { get; set; } = 500;
        public bool UseChargeSystemV2 { get; set; } = false;
        public int IdleReportRangeMm { get; set; } = 100;
        public int IdleReportIntervalMs { get; set; } = 2 * 60 * 1000;
        public int DischargeRetryTimes { get; set; } = 3;
        public int ChargeRetryTimes { get; set; } = 3;
        public int LowPowerRepeatChargeIntervalSec { get; set; } = 3 * 60;
        public int LowPowerRepeatedlyChargeCounterMax { get; set; } = 3;
        public int SleepLowPowerWatcherSec { get; set; } = 60;
        public bool AgreeAgvcSetCoupler { get; set; } = true;
    }
}
