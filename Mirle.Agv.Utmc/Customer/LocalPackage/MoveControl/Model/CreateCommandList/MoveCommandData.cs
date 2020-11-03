using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class MoveCommandData
    {
        public bool IsAutoCommand { get; set; } = false;
        public EnumMoveStatus MoveStatus { get; set; } = EnumMoveStatus.Moving;

        public double CommandEncoder { get; set; } = 0;

        public List<Command> CommandList { get; set; }
        public int IndexOfCommandList { get; set; } = 0;

        public List<SectionLine> SectionLineList { get; set; }
        public int IndexOflisSectionLine { get; set; } = 0;

        public List<ReserveData> ReserveList { get; set; }
        public int IndexOfReserveList { get; set; } = 0;

        public int WaitReserveIndex { get; set; } = -1;

        public DateTime StartTime { get; set; } = DateTime.Now;

        // 正常速度命令.
        public double NormalVelocity { get; set; } = 0;
        // 目前速度命令.
        public double NowVelocity { get; set; } = 0;

        public MapAGVPosition EndAGVPosition { get; set; } = null;
        public EnumVehicleSafetyAction SensorStatus { get; set; } = EnumVehicleSafetyAction.Normal;
        public EnumVehicleStopLevel VehicleStopLevel { get; set; } = EnumVehicleStopLevel.None;
        public bool VehicleStopFlag { get; set; } = false;
        public EnumVehicleSafetyAction AGVPause { get; set; } = EnumVehicleSafetyAction.Normal;
        public bool Cancel { get; set; } = false;

        public EnumEMSResetFlow EMSResetStatus { get; set; } = EnumEMSResetFlow.None;

        public EnumVehicleSafetyAction KeepsLowSpeedStateByEQVChange { get; set; } = EnumVehicleSafetyAction.SlowStop;
        //public List<List<Command>> CheckTurnAction { get; set; }
        //public int MoveDirIndex { get; set; }

        public MapAddress EndAddress { get; set; } = new MapAddress();
        public MapAddress StartAddress { get; set; } = new MapAddress();

        public bool IsLocalCommand { get; set; } = true;
        public string CommandID { get; set; } = "";
        public EnumStageDirection StageDirection { get; set; } = EnumStageDirection.None;
        public int StageNumber { get; set; } = 0;
        public bool IsLoadUnloadMove { get; set; } = false;

        public EnumMoveCommandStartStatus CommandStatus { get; set; } = EnumMoveCommandStartStatus.WaitStart;

        public bool OntimeReviseThetaDelaying { get; set; } = false;
        private System.Diagnostics.Stopwatch ontimeReviseThetaDelayTimer { get; set; } = new System.Diagnostics.Stopwatch();

        public bool OntimeReviseThetaOn
        {
            get
            {
                if (OntimeReviseThetaDelaying)
                    return ontimeReviseThetaDelayTimer.ElapsedMilliseconds > LocalData.Instance.MoveControlData.MoveControlConfig.TimeValueConfig.DelayTimeList[EnumDelayTimeType.OntimeReviseAlarmDelayTime];
                else
                    return false;
            }

            set
            {
                if (value != OntimeReviseThetaDelaying)
                {
                    if (value)
                    {   // 開始計算delay.
                        ontimeReviseThetaDelayTimer.Restart();
                        OntimeReviseThetaDelaying = true;
                    }
                    else
                    {   // 停掉.
                        OntimeReviseThetaDelaying = false;
                        ontimeReviseThetaDelayTimer.Stop();
                    }
                }
            }
        }

        public bool OntimeReviseDeviationDelaying { get; set; } = false;
        private System.Diagnostics.Stopwatch ontimeReviseDeviationTimer { get; set; } = new System.Diagnostics.Stopwatch();

        public bool OntimeReviseDeviationOn
        {
            get
            {
                if (OntimeReviseDeviationDelaying)
                    return ontimeReviseDeviationTimer.ElapsedMilliseconds > LocalData.Instance.MoveControlData.MoveControlConfig.TimeValueConfig.DelayTimeList[EnumDelayTimeType.OntimeReviseAlarmDelayTime];
                else
                    return false;
            }

            set
            {
                if (value != OntimeReviseDeviationDelaying)
                {
                    if (value)
                    {   // 開始計算delay.
                        ontimeReviseDeviationTimer.Restart();
                        OntimeReviseDeviationDelaying = true;
                    }
                    else
                    {   // 停掉.
                        OntimeReviseDeviationDelaying = false;
                        ontimeReviseDeviationTimer.Stop();
                    }
                }
            }
        }
    }
}
