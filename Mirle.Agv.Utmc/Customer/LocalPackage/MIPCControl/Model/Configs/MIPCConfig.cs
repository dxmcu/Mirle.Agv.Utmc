using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model.Configs
{
    public class MIPCConfig
    {
        public string IP { get; set; } = "192.168.29.2";
        public int PollingInterval { get; set; } = 100;
        public Dictionary<int, int> PollingGroupInterval = new Dictionary<int, int>();
        public int SocketTimeoutValue { get; set; } = 1000;
        public int CommandTimeoutValue { get; set; } = 50;
        public int HeartbeatInterval { get; set; } = 50;
        public int SafetySensorUpdateInterval { get; set; } = 50;
        public double ChargingSOC_High { get; set; } = 80;
        public double ChargingSOC_Low { get; set; } = 50;

        public double SOCLowBattery { get; set; } = 40;
        public string MIPCDataConfigPath { get; set; } = @"D:\MecanumConfigs\MIPCControl\MIPCDataConfig.csv";
        public string SafetySensorConfigPath { get; set; } = "";
        public List<MIPCPortData> PortList = new List<MIPCPortData>();
        public Dictionary<string, MIPCPortData> AllPort = new Dictionary<string, MIPCPortData>();
        public bool LogMode { get; set; } = true;
        public double BatteryCSVInterval { get; set; } = 3000;
    }
}
