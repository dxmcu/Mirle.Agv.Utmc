using Mirle.Agv.Utmc.Customer;
using Mirle.Agv.Utmc.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.ConnectionMode
{
    public class UtmcConnectionModeAdapter : ConnectionMode.IConnectionModeHandler
    {
        public event EventHandler<EnumAutoState> OnModeChangeEvent;
        public event EventHandler<MessageHandlerArgs> OnLogDebugEvent;
        public event EventHandler<MessageHandlerArgs> OnLogErrorEvent;
        public LocalPackage LocalPackage { get; set; }

        public UtmcConnectionModeAdapter(LocalPackage localPackage)
        {
            this.LocalPackage = localPackage;
        }

        public void AgvcDisconnect()
        {

        }

        public void SetAutoState(EnumAutoState autoState)
        {

        }
    }
}
