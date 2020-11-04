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
            //TODO: LocalPackage publish ModeChangeEvent
        }

        public void AgvcDisconnect()
        {

        }

        public void SetAutoState(EnumAutoState autoState)
        {
            try
            {
                Agv.EnumAutoState localPackageAutoState = GetLocalPackageAutoStateFrom(autoState);
                string errorMessage = "";
                if (LocalPackage.MainFlowHandler.ChangeAutoManual(localPackageAutoState, ref errorMessage))
                {
                    OnModeChangeEvent?.Invoke(this, autoState);
                }
                else
                {
                    OnLogDebugEvent?.Invoke(this, new MessageHandlerArgs()
                    {
                        ClassMethodName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name,
                        Message = $"LocalPackage.ChangeMode fail.[Error={errorMessage}]"
                    });
                }
            }
            catch (Exception ex)
            {
                OnLogErrorEvent?.Invoke(this, new MessageHandlerArgs()
                {
                    ClassMethodName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message
                });
            }
        }

        private Agv.EnumAutoState GetLocalPackageAutoStateFrom(EnumAutoState autoState)
        {
            switch (autoState)
            {
                case EnumAutoState.Auto:
                    return Agv.EnumAutoState.Auto;
                case EnumAutoState.Manual:
                    return Agv.EnumAutoState.Manual;
                case EnumAutoState.None:
                    return Agv.EnumAutoState.PreAuto;
                default:
                    return Agv.EnumAutoState.PreAuto;
            }
        }
    }
}
