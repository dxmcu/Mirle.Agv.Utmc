using Mirle.Agv.Utmc.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.ConnectionMode
{
    public class NullObjConnectionModeHandler : IConnectionModeHandler
    {
        public event EventHandler<EnumAutoState> OnModeChangeEvent;
        public event EventHandler<MessageHandlerArgs> OnLogDebugEvent;
        public event EventHandler<MessageHandlerArgs> OnLogErrorEvent;

        public EnumAutoState AutoState { get; set; }

        public NullObjConnectionModeHandler(EnumAutoState autoState)
        {
            SetAutoState(autoState);
        }

        public void AgvcDisconnect()
        {
            
        }

        public void SetAutoState(EnumAutoState autoState)
        {
            OnModeChangeEvent?.Invoke(this, autoState);          
        }
    }
}
