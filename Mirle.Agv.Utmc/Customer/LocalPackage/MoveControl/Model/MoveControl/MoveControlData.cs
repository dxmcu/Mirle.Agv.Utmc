using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class MoveControlData
    {
        public MoveCommandData MoveCommand { get; set; } = null;

        public string CommnadID
        {
            get
            {
                MoveCommandData temp = MoveCommand;

                return (temp == null ? "" : temp.CommandID);
            }
        }

        public bool CreateCommanding { get; set; } = false;
        public object CreateCommandingLockObjcet { get; set; } = new object();

        public string MoveControlCantAutoReason { get; set; } = "";
        public string MoveControlNotReadyReason { get; set; } = "";
        public bool MoveControlCanAuto { get; set; } = false;

        private bool ready = false;
        public bool Ready
        {
            get
            {
                if (CreateCommanding)
                    return false;
                else
                    return ready;
            }

            set
            {
                ready = value;
            }
        }

        public List<MoveCommandRecord> MoveCommandRecordList { get; set; } = new List<MoveCommandRecord>();

        public string LastCommandID { get; set; } = "";

        private int maxOfCommandRecordList = 20;

        public object LockMoveCommandRecordObject { get; } = new object();

        public string MoveCommandRecordString { get; set; } = "";

        private int maxOfMoveCOmmandRecordStringLength = 5000;

        public void AddMoveCOmmandRecordList(MoveCommandData command, EnumMoveComplete report)
        {
            lock (LockMoveCommandRecordObject)
            {
                LastCommandID = command.CommandID;
                MoveCommandRecordList.Insert(0, new MoveCommandRecord(command, report));

                while (MoveCommandRecordList.Count > maxOfCommandRecordList)
                    MoveCommandRecordList.RemoveAt(MoveCommandRecordList.Count - 1);

                MoveCommandRecordString = String.Concat(MoveCommandRecordList[0].LogString, "\r\n", MoveCommandRecordString);

                if (MoveCommandRecordString.Length > maxOfMoveCOmmandRecordStringLength)
                    MoveCommandRecordString = MoveCommandRecordString.Substring(0, maxOfMoveCOmmandRecordStringLength);
            }
        }

        public bool ErrorBit { get; set; } = false;

        public MoveControlConfig MoveControlConfig { get; set; }
        public CreateMoveCommandListConfig CreateMoveCommandConfig { get; set; }

        public Dictionary<string, AGVTurnParameter> TurnParameter { get; set; } = new Dictionary<string, AGVTurnParameter>();

        public LocateControlData LocateControlData { get; set; } = new LocateControlData();
        public MotionControlData MotionControlData { get; set; } = new MotionControlData();

        public bool ReserveStop
        {
            get
            {
                MoveCommandData temp = MoveCommand;

                if (temp == null)
                    return false;
                else
                {
                    return temp.WaitReserveIndex != -1 && MotionControlData.MoveStatus == EnumAxisMoveStatus.Stop &&
                           (temp.SensorStatus == EnumVehicleSafetyAction.SlowStop || temp.SensorStatus == EnumVehicleSafetyAction.EMS);
                }
            }
        }

        public bool SafetySensorStop
        {
            get
            {
                MoveCommandData temp = MoveCommand;

                if (temp == null)
                    return false;
                else
                {
                    return MotionControlData.MoveStatus == EnumAxisMoveStatus.Stop &&
                           (LocalData.Instance.MIPCData.SafetySensorStatus == EnumSafetyLevel.EMS ||
                            LocalData.Instance.MIPCData.SafetySensorStatus == EnumSafetyLevel.SlowStop);
                }
            }
        }

        public ThetaSectionDeviation ThetaSectionDeviation { get; set; } = null;

        public MoveControlSensorStatus SensorStatus { get; set; } = new MoveControlSensorStatus();

        public bool SimulateBypassLog { get; set; } = false;
    }
}
