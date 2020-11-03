using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Controller
{
    public class InitialEventArgs : EventArgs
    {
        public string Message { get; set; }
        public bool IsEnd { get; set; } = false;
        public bool Scuess { get; set; } = false;
        
        public InitialEventArgs(string message, bool isEnd = false, bool scuess = false)
        {
            Message = message;
            IsEnd = isEnd;
            Scuess = scuess;
        }
    }
}
