using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class CommunicationData
    {
        public bool AutoRetryConnect { get; set; } = true;
        public bool Connectedd { get; set; } = false;
        private double timeoutValue = 2000;

        private Stopwatch timeoutTimer = new Stopwatch();
        public bool connected { get; set; } = false;

        public bool Connected
        {
            get
            {
                if (connected)
                    return connected;
                else
                {
                    if (timeoutTimer.ElapsedMilliseconds > timeoutValue)
                        return connected;
                    else
                        return true;
                }
            }

            set
            {
                if (connected != value)
                {
                    if (value)
                        timeoutTimer.Reset();
                    else
                        timeoutTimer.Restart();

                    connected = value;
                }
            }
        }

        public CommunicationData(double timeoutValue)
        {
            this.timeoutValue = timeoutValue;
            timeoutTimer.Restart();
        }
    }
}
