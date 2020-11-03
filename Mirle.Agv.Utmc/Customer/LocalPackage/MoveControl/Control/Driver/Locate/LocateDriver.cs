using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Model;
using Mirle.Agv.INX.Model.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Controller
{
    public class LocateDriver : Driver
    {
        public LocateDriverConfig DriverConfig { get; set; } = null;
        protected Logger logger;
        public EnumLocateType LocateType { get; set; } = EnumLocateType.Normal;

        virtual public void InitialDriver(LocateDriverConfig driverConfig, AlarmHandler alarmHandler, string normalLogName )
        {
            this.normalLogName = normalLogName;
            this.DriverConfig = driverConfig;
            this.alarmHandler = alarmHandler;
            device = driverConfig.LocateDriverType.ToString();
            InitialConfig(driverConfig.Path);
        }

        virtual protected void InitialConfig(string path)
        {
        }

        virtual public LocateAGVPosition GetLocateAGVPosition
        {
            get
            {
                return null;
            }
        }
    }
}
