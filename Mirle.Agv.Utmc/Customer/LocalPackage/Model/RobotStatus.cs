using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Model
{
    public class RobotStatus
    {
        public EnumRobotState EnumRobotState { get; set; } = EnumRobotState.Idle;
        public bool IsHome { get; set; } = true;

        public RobotStatus() { }

        public RobotStatus(RobotStatus robotStatus)
        {
            this.EnumRobotState = robotStatus.EnumRobotState;
            this.IsHome = robotStatus.IsHome;
        }
    }
}
