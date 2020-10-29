using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.ConnectionMode
{
    public interface IConnectionModeHandler : Tools.IMessageHandler
    {
        public event EventHandler<EnumAutoState> OnModeChangeEvent;

        public void AgvcDisconnect();

        public void SetAutoState(EnumAutoState autoState);
    }
}
