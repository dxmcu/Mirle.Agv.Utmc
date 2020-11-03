using Mirle.Agv.INX.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Control
{
    public class LoadUnload
    {
        protected MIPCControlHandler mipcControl = null;
        protected EnumPIOType pioType = EnumPIOType.None;

        protected PIOFlow pioControl = null;

        public void Initial(MIPCControlHandler mipcControl, EnumPIOType pioType)
        {
            this.mipcControl = mipcControl;
            this.pioType = pioType;

            switch (pioType)
            {
                //case EnumPIOType.???:
                //    pioControl = new ???();
                //    break;
                default:
                    break;
            }
        }

        public virtual bool LoadUnloadRequest(EnumLoadUnload action, EnumStageDirection stageDir, int stageNumber, int speedPercent, bool needPIO)
        {
            return false;
        }
    }
}
