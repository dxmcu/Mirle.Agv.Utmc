using Mirle.Agv.INX.Controller;
using Mirle.Agv.Utmc.Model;
using NUnit.Framework.Internal.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Customer
{
    public class LocalPackage
    {
        public Mirle.Agv.INX.Controller.MainFlowHandler MainFlowHandler { get; set; }
        public event EventHandler<Mirle.Agv.Utmc.Model.InitialEventArgs> OnLocalPackageComponentIntialDoneEvent;

        public LocalPackage()
        {
            MainFlowHandler = new INX.Controller.MainFlowHandler();
            MainFlowHandler.OnComponentIntialDoneEvent += MainFlowHandler_OnComponentIntialDoneEvent;           
        }

        public void InitialLocalMain()
        {
            MainFlowHandler.InitialMainFlowHander();
        }

        private void MainFlowHandler_OnComponentIntialDoneEvent(object sender, INX.Controller.InitialEventArgs e)
        {
            OnLocalPackageComponentIntialDoneEvent?.Invoke(this, new Model.InitialEventArgs()
            {
                IsOk = e.Scuess,
                ItemName = e.Message
            });
        }
    }
}
