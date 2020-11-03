using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class MoveControlSensorStatus
    {
        public bool Charging { get; set; } = false;
        public bool ForkReady { get; set; } = true;

        public EnumVehicleSafetyAction localPause { get; set; } = EnumVehicleSafetyAction.Normal;      // 原始資料,不包含Delay內容.
        private Stopwatch localPauseTimer = new Stopwatch();                                                // Delay用Timer.
        private bool localPauseDelaying = false;

        public EnumVehicleSafetyAction LocalPause                                                      // 移動控制取得資料.
        {
            get
            {
                if (localPauseDelaying)
                {
                    if (localPauseTimer.ElapsedMilliseconds > LocalData.Instance.MoveControlData.MoveControlConfig.TimeValueConfig.DelayTimeList[EnumDelayTimeType.Local_PauseStartDelayTime])
                    {
                        localPauseDelaying = false;
                        localPauseTimer.Stop();
                        return localPause;
                    }
                    else
                        return EnumVehicleSafetyAction.SlowStop;
                }
                else
                    return localPause;
            }

            set
            {
                if (value != localPause)
                {
                    if (value == EnumVehicleSafetyAction.SlowStop)
                    {   // 變成Stop.
                        localPauseDelaying = false;
                        localPauseTimer.Stop();
                    }
                    else if (localPause == EnumVehicleSafetyAction.SlowStop)
                    {     // Stop->Move.
                        localPauseTimer.Restart();
                        localPauseDelaying = true;
                    }

                    localPause = value;
                }
            }
        }
    }
}
