using Mirle.Agv.Utmc.Customer;
using Mirle.Agv.Utmc.Model;
using Mirle.Agv.Utmc.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Move
{
    public class UtmcMoveAdapter : Move.IMoveHandler
    {
        public event EventHandler<MoveStatus> OnUpdateMoveStatusEvent;
        public event EventHandler<PositionArgs> OnUpdatePositionArgsEvent;
        public event EventHandler<bool> OnOpPauseOrResumeEvent;
        public event EventHandler<MessageHandlerArgs> OnLogDebugEvent;
        public event EventHandler<MessageHandlerArgs> OnLogErrorEvent;
        public LocalPackage LocalPackage { get; set; }

        public UtmcMoveAdapter(LocalPackage localPackage)
        {
            this.LocalPackage = localPackage;
            localPackage.MainFlowHandler.MoveControl.MoveCompleteEvent += MoveControl_MoveCompleteEvent;
            localPackage.MainFlowHandler.MoveControl.PassAddressEvent += MoveControl_PassAddressEvent;
        }

        private void MoveControl_PassAddressEvent(object sender, string addressId)
        {
            try
            {
                var address = Vehicle.Instance.MapInfo.addressMap[addressId];
                var localData = LocalPackage.MainFlowHandler.localData;
                OnUpdatePositionArgsEvent?.Invoke(sender, new PositionArgs()
                {
                    MapPosition = address.Position,
                    EnumLocalArrival = EnumLocalArrival.Arrival,
                    HeadAngle = (int)localData.Real.Angle,
                    MovingDirection = (int)localData.MoveDirectionAngle,
                    Speed = (int)localData.MoveControlData.MotionControlData.LineVelocity
                });
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

        private void MoveControl_MoveCompleteEvent(object sender, Agv.EnumMoveComplete e)
        {
            try
            {
                var localData = LocalPackage.MainFlowHandler.localData;
                OnUpdatePositionArgsEvent?.Invoke(sender, new PositionArgs()
                {
                    EnumLocalArrival = GetEnumLocalArrival(e),
                    MapPosition = new MapPosition()
                    {
                        X = localData.Real.Position.X,
                        Y = localData.Real.Position.Y
                    },
                    HeadAngle = (int)localData.Real.Angle,
                    MovingDirection = (int)localData.MoveDirectionAngle,
                    Speed = (int)localData.MoveControlData.MotionControlData.LineVelocity

                });
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

        private EnumLocalArrival GetEnumLocalArrival(Agv.EnumMoveComplete e)
        {
            switch (e)
            {
                case Agv.EnumMoveComplete.End:
                    return EnumLocalArrival.EndArrival;
                case Agv.EnumMoveComplete.Error:
                    return EnumLocalArrival.Fail;
                case Agv.EnumMoveComplete.Cancel:
                    return EnumLocalArrival.Arrival;
                default:
                    return EnumLocalArrival.Arrival;
            }
        }

        public void GetMoveStatus()
        {
            var localData = LocalPackage.MainFlowHandler.localData;
            if (localData.Real != null)
            {
                OnUpdateMoveStatusEvent?.Invoke(this, new MoveStatus()
                {
                    LastMapPosition = new MapPosition()
                    {
                        X = localData.Real.Position.X,
                        Y = localData.Real.Position.Y
                    },
                    HeadDirection = (int)localData.Real.Angle,
                    MovingDirection = (int)localData.MoveDirectionAngle,
                    Speed = (int)localData.MoveControlData.MotionControlData.LineVelocity,
                    EnumMoveState = GetEnumMoveState(localData),
                    LastSection = GetLastSection(localData),
                    LastAddress = GetLastAddress(localData)
                });
            }
        }

        private MapAddress GetLastAddress(INX.Model.LocalData localData)
        {
            try
            {
                INX.Model.VehicleLocation nowVehicleLocation = localData.Location;
                var addressId = nowVehicleLocation.LastAddress;
                return Vehicle.Instance.MapInfo.addressMap[addressId];
            }
            catch (Exception ex)
            {
                OnLogErrorEvent?.Invoke(this, new MessageHandlerArgs()
                {
                    ClassMethodName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message
                });
            }

            return null;
        }

        private MapSection GetLastSection(INX.Model.LocalData localData)
        {
            try
            {
                INX.Model.VehicleLocation nowVehicleLocation = localData.Location;
                var sectionId = nowVehicleLocation.NowSection;
                var lastSection = Vehicle.Instance.MapInfo.sectionMap[sectionId];
                lastSection.VehicleDistanceSinceHead = localData.Location.DistanceFormSectionHead;
                return lastSection;
            }
            catch (Exception ex)
            {
                OnLogErrorEvent?.Invoke(this, new MessageHandlerArgs()
                {
                    ClassMethodName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message
                });
            }

            return null;
        }

        private EnumMoveState GetEnumMoveState(INX.Model.LocalData localData)
        {
            if (localData.MoveControlData.ReserveStop)
            {
                return EnumMoveState.ReserveStop;
            }

            if (localData.MoveControlData.SafetySensorStop)
            {
                return EnumMoveState.Block;
            }

            if (Math.Abs(localData.MoveControlData.MotionControlData.LineVelocity) > 5)
            {
                return EnumMoveState.Working;
            }
            else
            {
                return EnumMoveState.Idle;
            }

        }

        public void InitialPosition()
        {
            //TODO Set Initial Position for simulator
        }

        public void ReserveOkPartMove(MapSection mapSection)
        {
            LocalPackage.MainFlowHandler.MoveControl.AddReserve(mapSection.Id);
        }

        public void SetMoveStatusFrom(MoveStatus moveStatus)
        {
        }

        public void SetMovingGuide(MovingGuide movingGuide)
        {
            var localData = LocalPackage.MainFlowHandler.localData;

            if (IsReadyForMoveCommand())
            {
                Task.Run(() =>
                {
                    if (localData.MoveControlData.MoveCommand == null)
                    {
                        string errorMessage = "";
                        var moveCmdInfo = GetLocalMoveCmdInfoFrom(movingGuide);
                        LocalPackage.MainFlowHandler.MoveControl.VehicleMove(moveCmdInfo, ref errorMessage);

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            OnLogDebugEvent?.Invoke(this, new MessageHandlerArgs()
                            {
                                ClassMethodName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name,
                                Message = $"LocalPackage.MoveControl can not do move command.[Error={errorMessage}]"
                            });
                        }
                    }
                    else
                    {
                        OnLogDebugEvent?.Invoke(this, new MessageHandlerArgs()
                        {
                            ClassMethodName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name,
                            Message = $"LocalPackage.MoveControl has another move command.[EndAddress={localData.MoveControlData.MoveCommand.EndAddress.Id}]"
                        });
                    }
                });
            }
            else
            {
                OnLogDebugEvent?.Invoke(this, new MessageHandlerArgs()
                {
                    ClassMethodName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    Message = $"LocalPackage is not ready for move command.[ReadySignal={localData.MoveControlData.Ready}][ErrorSignal={localData.MoveControlData.ErrorBit}]"
                });
            }
        }

        private INX.Model.MoveCmdInfo GetLocalMoveCmdInfoFrom(MovingGuide movingGuide)
        {
            return new INX.Model.MoveCmdInfo()
            {
                 CommandID = movingGuide.CommandId,
                 AddressIds = movingGuide.GuideAddressIds,
                 SectionIds = movingGuide.GuideSectionIds
            };
        }

        private bool IsReadyForMoveCommand()
        {
            var localData = LocalPackage.MainFlowHandler.localData;
            return localData.MoveControlData.Ready && !localData.MoveControlData.ErrorBit;
        }

        public void PauseMove()
        {
            Task.Run(() =>
            {
                LocalPackage.MainFlowHandler.MoveControl.VehiclePause();
            });
        }

        public void ResumeMove()
        {
            Task.Run(() =>
            {
                LocalPackage.MainFlowHandler.MoveControl.VehicleContinue();
            });
        }

        public void StopMove()
        {
            Task.Run(() =>
            {
                LocalPackage.MainFlowHandler.MoveControl.VehicleCancel();
            });
        }
    }
}
