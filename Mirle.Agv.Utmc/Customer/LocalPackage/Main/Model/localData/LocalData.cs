using Mirle.Agv.INX.Model.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class LocalData
    {
        private static readonly LocalData localData = new LocalData();
        public static LocalData Instance { get { return localData; } }

        public string ConfigPath { get; set; } = @"D:\MecanumConfigs";
        public MapConfig MapConfig { get; set; }

        public MainFlowConfig MainFlowConfig { get; set; }


        public string ErrorLogName { get; set; } = "Error";
        public int ErrorLevel { get; set; } = 5;

        public EnumLoginLevel LoginLevel { get; set; } = EnumLoginLevel.MirleAdmin;
        public bool SimulateMode { get; set; } = false;

        public MapInfo TheMapInfo { get; set; } = new MapInfo();

        public BatteryInfo BatteryInfo { get; set; } = new BatteryInfo();

        public BatteryLog BatteryLogData { get; set; }

        public MapAGVPosition Real { get; set; } = null;

        public double MoveDirectionAngle { get; set; } = 0;

        public MapAGVPosition LastAGVPosition { get; set; } = null;
        public VehicleLocation Location { get; set; } = new VehicleLocation();

        public EnumAutoState AutoManual { get; set; } = EnumAutoState.Manual;

        public MoveControlData MoveControlData { get; set; } = new MoveControlData();
        public LoadUnloadControlData LoadUnloadData { get; set; } = new LoadUnloadControlData();
        public MIPCControlData MIPCData { get; set; } = new MIPCControlData();

        public CommunicationData MiddlerStatus { get; set; }
        public CommunicationData AGVCStatus { get; set; }
    }
}
