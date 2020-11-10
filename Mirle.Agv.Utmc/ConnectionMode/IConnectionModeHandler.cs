using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.ConnectionMode
{
    interface IConnectionModeHandler : Tools.IMessageHandler
    {
        event EventHandler<EnumAutoState> OnModeChangeEvent;

        void AgvcDisconnect();

        void SetAutoState(EnumAutoState autoState);
    }
}
