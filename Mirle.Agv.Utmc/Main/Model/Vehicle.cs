using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Utmc.Model.TransferSteps;
using com.mirle.aka.sc.ProtocolFormat.ase.agvMessage;
using Mirle.Agv.Utmc.Model.Configs;
using Mirle.Agv.Utmc.Controller;
using System.Reflection;
using System.Collections.Concurrent;
using NUnit.Framework.Constraints;

namespace Mirle.Agv.Utmc.Model
{

    public class Vehicle
    {
        private static readonly Vehicle theVehicle = new Vehicle();
        public static Vehicle Instance { get { return theVehicle; } }
        public ConcurrentDictionary<string, AgvcTransferCommand> mapTransferCommands { get; set; } = new ConcurrentDictionary<string, AgvcTransferCommand>();
        public AgvcTransferCommand TransferCommand { get; set; } = new AgvcTransferCommand();
        public EnumAutoState AutoState { get; set; } = EnumAutoState.Manual;
        public string SoftwareVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public bool IsAgvcConnect { get; set; } = false;
        public EnumLoginLevel LoginLevel { get; set; } = EnumLoginLevel.Op;
        public EnumChargingStage ChargingStage { get; set; } = EnumChargingStage.Idle;
        public MapInfo Mapinfo { get; private set; } = new MapInfo();
        public string AskReserveQueueException { get; set; } = "NONE";

        #region AsePackage

        public bool IsLocalConnect { get; set; } = false;
        public MoveStatus MoveStatus { get; set; } = new MoveStatus();
        public RobotStatus RobotStatus { get; set; } = new RobotStatus();
        public CarrierSlotStatus CarrierSlotStatus { get; set; } = new CarrierSlotStatus();
        public CarrierSlotStatus CarrierSlotRight { get; set; } = new CarrierSlotStatus(EnumSlotNumber.R);
        public bool IsCharging { get; set; } = false;
        public BatteryStatus BatteryStatus { get; set; } = new BatteryStatus();
        public MovingGuide MovingGuide { get; set; } = new MovingGuide();
        public string PspSpecVersion { get; set; } = "1.0";

        public bool CheckStartChargeReplyEnd { get; set; } = true;
        public bool CheckStopChargeReplyEnd { get; set; } = true;

        #endregion

        #region Comm Property
        //public VHActionStatus ActionStatus { get; set; } = VHActionStatus.NoCommand;
        public VhStopSingle BlockingStatus { get; set; } = VhStopSingle.Off;
        public VhChargeStatus ChargeStatus { get; set; } = VhChargeStatus.ChargeStatusNone;
        public DriveDirction DrivingDirection { get; set; } = DriveDirction.DriveDirNone;
        public VhStopSingle ObstacleStatus { get; set; } = VhStopSingle.Off;
        public int ObstDistance { get; set; }
        public string ObstVehicleID { get; set; } = "";
        public VhPowerStatus PowerStatus { get; set; } = VhPowerStatus.PowerOn;
        public string StoppedBlockID { get; set; } = "";
        public VhStopSingle ErrorStatus { get; set; } = VhStopSingle.Off;
        public uint CmdPowerConsume { get; set; }
        public int CmdDistance { get; set; }
        public string TeachingFromAddress { get; internal set; } = "";
        public string TeachingToAddress { get; internal set; } = "";
        public VHActionStatus ActionStatus { get; set; } = VHActionStatus.NoCommand;
        public BCRReadResult LeftReadResult { get; set; } = BCRReadResult.BcrReadFail;
        public BCRReadResult RightReadResult { get; set; } = BCRReadResult.BcrReadFail;
        public VhStopSingle OpPauseStatus { get; set; } = VhStopSingle.Off;
        public ConcurrentDictionary<PauseType, bool> PauseFlags = new ConcurrentDictionary<PauseType, bool>(Enum.GetValues(typeof(PauseType)).Cast<PauseType>().ToDictionary(x => x, x => false));
        public uint WifiSignalStrength { get; set; } = 0;
        public List<PortInfo> PortInfos { get; set; } = new List<PortInfo>();

        #endregion

        #region Configs

        //Main Configs
        public MainFlowConfig MainFlowConfig { get; set; } = new MainFlowConfig();
        public AgvcConnectorConfig AgvcConnectorConfig { get; set; } = new AgvcConnectorConfig();
        public MapConfig MapConfig { get; set; } = new MapConfig();
        public AlarmConfig AlarmConfig { get; set; } = new AlarmConfig();
        public BatteryLog BatteryLog { get; set; } = new BatteryLog();

        #endregion

        private Vehicle() { }

        public CarrierSlotStatus GetCarrierSlotStatusFrom(EnumSlotNumber slotNumber)
        {
            switch (slotNumber)
            {
                case EnumSlotNumber.R:
                    return this.CarrierSlotRight;
                case EnumSlotNumber.L:
                default:
                    return this.CarrierSlotStatus;
            }
        }

        public bool IsPause()
        {
            return PauseFlags.Values.Any(x => x);
        }

        public void ResetPauseFlags()
        {
            PauseFlags = new ConcurrentDictionary<PauseType, bool>(Enum.GetValues(typeof(PauseType)).Cast<PauseType>().ToDictionary(x => x, x => false));
        }
    }
}
