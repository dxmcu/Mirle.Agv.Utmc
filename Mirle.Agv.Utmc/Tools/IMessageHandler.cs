using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Tools
{
    interface IMessageHandler
    {
        event EventHandler<MessageHandlerArgs> OnLogDebugEvent;
        event EventHandler<MessageHandlerArgs> OnLogErrorEvent;
    }
}
