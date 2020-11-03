using Mirle.Agv.INX.Controller;
using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Control
{
    public class PIOFlow
    {
        protected LoggerAgent loggerAgent = LoggerAgent.Instance;
        protected LocalData localData = LocalData.Instance;

        public EnumPIOStatus Status { get; set; } = EnumPIOStatus.Idle;
        public EnumPIOTimeout Timeout { get; set; } = EnumPIOTimeout.None;
        protected string device = "";

        public List<string> PIOInputList = new List<string>();
        public List<string> PIOIOutputList = new List<string>();

        protected MIPCControlHandler mipcControl = null;
        protected string normalLogName = "PIO";
        public string PIOName { get; set; } = "";


        public virtual void Initial(MIPCControlHandler mipcControl, string pioName, string pioDirection)
        {
            PIOName = pioName;
            this.mipcControl = mipcControl;
            device = MethodInfo.GetCurrentMethod().ReflectedType.Name;

            localData.MIPCData.AllDataByMIPCTagName.ContainsKey("XXX");

            float result = localData.MIPCData.GetDataByMIPCTagName("XXX"); // 取資料.


            List<string> tagNameList = new List<string>() { "XXX" };
            List<float> valueList = new List<float>() { 0 };

            if (!mipcControl.SendMIPCDataByMIPCTagName(tagNameList, valueList))
                ;//retry
        }

        protected void WriteLog(int logLevel, string carrierId, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            LogFormat logFormat = new LogFormat(normalLogName, logLevel.ToString(), memberName, device, carrierId, message);

            loggerAgent.Log(logFormat.Category, logFormat);

            if (logLevel <= localData.ErrorLevel)
            {
                logFormat = new LogFormat(localData.ErrorLogName, logLevel.ToString(), memberName, device, carrierId, message);
                loggerAgent.Log(logFormat.Category, logFormat);
            }
        }

        public void ResetPIO()
        {
            if (Status == EnumPIOStatus.Complete)
            {
                Status = EnumPIOStatus.Idle;
            }
            else if (Status == EnumPIOStatus.NG)
            {
                Timeout = EnumPIOTimeout.None;
                Status = EnumPIOStatus.Idle;
            }
        }

        public virtual void StopPIO()
        {

        }
    }
}
