using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class MoveCommandRecord
    {
        public string CommandID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public double ActionTime { get; set; } // s

        public EnumMoveComplete Report { get; set; }
        public string Result { get; set; } = "";
        public string LogString { get; set; }
        public MoveCommandData Command { get; set; }

        public MoveCommandRecord(MoveCommandData command, EnumMoveComplete report)
        {
            Command = command;
            CommandID = command.CommandID;
            StartTime = command.StartTime;
            EndTime = DateTime.Now;
            ActionTime = (EndTime - StartTime).TotalSeconds;
            Report = report;

            switch (report)
            {
                case EnumMoveComplete.End:
                    Result = "Scuess";
                    break;
                case EnumMoveComplete.Error:
                    Result = "Error";
                    break;
                case EnumMoveComplete.Cancel:
                    Result = "Cancel";
                    break;
            }

            LogString = String.Concat(CommandID, "\t\t\t", StartTime.ToString("HH:mm:ss"), "\t", EndTime.ToString("HH:mm:ss"), "\t", Result);
        }
    }
}
