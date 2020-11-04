using Mirle.Agv.INX.Model;
using Mirle.Agv.Utmc.Controller;
using Mirle.Agv.Utmc.Customer;
using Mirle.Agv.Utmc.Model;
using Mirle.Agv.Utmc.Model.TransferSteps;
using Mirle.Agv.Utmc.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Robot
{
    public class UtmcRobotAdapter : IRobotHandler
    {
        public event EventHandler<CarrierSlotStatus> OnUpdateCarrierSlotStatusEvent;
        public event EventHandler<RobotStatus> OnUpdateRobotStatusEvent;
        public event EventHandler<EnumRobotEndType> OnRobotEndEvent;
        public event EventHandler<MessageHandlerArgs> OnLogDebugEvent;
        public event EventHandler<MessageHandlerArgs> OnLogErrorEvent;

        public LocalPackage LocalPackage { get; set; }

        public UtmcRobotAdapter(LocalPackage localPackage)
        {
            this.LocalPackage = localPackage;
            LocalPackage.MainFlowHandler.LoadUnloadControl.ForkCompleteEvent += LoadUnloadControl_ForkCompleteEvent;
        }

        private void LoadUnloadControl_ForkCompleteEvent(object sender, EnumLoadUnloadComplete enumLoadUnloadComplete)
        {
            EnumRobotEndType robotEndType = GetEnumRobotEndTypeFrom(enumLoadUnloadComplete);
            OnRobotEndEvent?.Invoke(this, robotEndType);
        }

        private EnumRobotEndType GetEnumRobotEndTypeFrom(EnumLoadUnloadComplete enumLoadUnloadComplete)
        {
            //TODO : EnumLoadUnloadComplete need InterlockError
            switch (enumLoadUnloadComplete)
            {
                case EnumLoadUnloadComplete.End:
                    return EnumRobotEndType.Finished;
                case EnumLoadUnloadComplete.Error:
                    return EnumRobotEndType.RobotError;
                default:
                    return EnumRobotEndType.InterlockError;
            }
        }

        public void ClearRobotCommand()
        {
            //LocalPackage.MainFlowHandler.StopLoadUnload();
        }

        public void DoRobotCommandFor(RobotCommand robotCommand)
        {          
            if (IsReadyForRobotCommand())
            {
                Task.Run(() =>
                {
                    string addressID = robotCommand.PortAddressId;
                    EnumLoadUnload enumLoadUnload = GetEnumLoadUnloadFrom(robotCommand);
                    if (!LocalPackage.MainFlowHandler.LoadUnloadCommand(addressID, enumLoadUnload))
                    {
                        OnLogDebugEvent?.Invoke(this, new MessageHandlerArgs()
                        {
                            ClassMethodName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name,
                            Message = "LocalPackage.MainFlowHandler.LoadUnloadCommand return fail."
                        });

                        //TODO : Vertify will local package publish robot fail event?
                        //OnRobotEndEvent?.Invoke(this, EnumRobotEndType.RobotError);
                    }
                });
            }
            else
            {
                OnLogDebugEvent?.Invoke(this, new MessageHandlerArgs()
                {
                    ClassMethodName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    Message = $"LocalPackage is not ready for robot command.[ReadySignal={LocalPackage.MainFlowHandler.localData.LoadUnloadData.Ready}][ErrorSignal={LocalPackage.MainFlowHandler.localData.LoadUnloadData.ErrorBit}]"
                });
            }
        }

        private EnumLoadUnload GetEnumLoadUnloadFrom(RobotCommand robotCommand)
        {
            switch (robotCommand.GetTransferStepType())
            {
                case EnumTransferStepType.Load:
                    return EnumLoadUnload.Load;

                case EnumTransferStepType.Unload:
                    return EnumLoadUnload.Unload;

                case EnumTransferStepType.Move:
                case EnumTransferStepType.MoveToCharger:
                case EnumTransferStepType.Empty:
                default:
                    return EnumLoadUnload.PreCheck;
            }
        }

        public void GetRobotAndCarrierSlotStatus()
        {
            LocalPackage.MainFlowHandler.UpdateLoadingAndCSTID();

            var loadUnloadData = LocalPackage.MainFlowHandler.localData.LoadUnloadData;

            OnUpdateRobotStatusEvent?.Invoke(this, new RobotStatus()
            {
                EnumRobotState = GetEnumRobotStateFrom(loadUnloadData),
                IsHome = loadUnloadData.Loading
            });

            OnUpdateCarrierSlotStatusEvent?.Invoke(this, new CarrierSlotStatus()
            {
                CarrierId = loadUnloadData.CstID,
                EnumCarrierSlotState = GetEnumCarrierSlotStateFrom(loadUnloadData),
                ManualDeleteCST = string.IsNullOrEmpty(loadUnloadData.CstID.Trim())
            });
        }

        private EnumCarrierSlotState GetEnumCarrierSlotStateFrom(LoadUnloadControlData loadUnloadData)
        {
            if (string.IsNullOrEmpty(loadUnloadData.CstID.Trim()))
            {
                return EnumCarrierSlotState.Empty;
            }

            if (loadUnloadData.CstID.ToUpper().Trim() == "ERROR")
            {
                return EnumCarrierSlotState.ReadFail;
            }

            return EnumCarrierSlotState.Loading;
        }

        private EnumRobotState GetEnumRobotStateFrom(LoadUnloadControlData loadUnloadControlData)
        {
            if (loadUnloadControlData.ErrorBit)
            {
                return EnumRobotState.Error;
            }
            if (loadUnloadControlData.Ready)
            {
                return EnumRobotState.Idle;
            }
            else
            {
                return EnumRobotState.Busy;
            }
        }

        public void SetCarrierSlotStatusTo(CarrierSlotStatus carrierSlotStatus)
        {
            var oriLoadUnloadData = LocalPackage.MainFlowHandler.localData.LoadUnloadData;

            LoadUnloadControlData tempLoadUnloadControlData = new LoadUnloadControlData()
            {
                CommandStatus = oriLoadUnloadData.CommandStatus,
                CommnadID = oriLoadUnloadData.CommnadID,
                CstID = carrierSlotStatus.CarrierId,
                ErrorBit = oriLoadUnloadData.ErrorBit,
                ForkHome = oriLoadUnloadData.ForkHome,
                Loading = GetLoadingFrom(carrierSlotStatus),
                Ready = oriLoadUnloadData.Ready
            };

            LocalPackage.MainFlowHandler.localData.LoadUnloadData = tempLoadUnloadControlData;

            //TODO: DoCstRenameByAgvcFor(tempLoadUnloadControlData);
        }

        private bool GetLoadingFrom(CarrierSlotStatus carrierSlotStatus)
        {
            switch (carrierSlotStatus.EnumCarrierSlotState)
            {
                case EnumCarrierSlotState.Empty:
                    return false;
                case EnumCarrierSlotState.Loading:
                    return true;
                case EnumCarrierSlotState.PositionError:
                case EnumCarrierSlotState.ReadFail:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsReadyForRobotCommand()
        {
            return LocalPackage.MainFlowHandler.localData.LoadUnloadData.Ready && !LocalPackage.MainFlowHandler.localData.LoadUnloadData.ErrorBit;
        }
    }
}
