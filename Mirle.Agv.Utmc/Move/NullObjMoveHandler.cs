using Mirle.Agv.Utmc.Model;
using Mirle.Agv.Utmc.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Move
{
    public class NullObjMoveHandler : IMoveHandler
    {
        public event EventHandler<MoveStatus> OnUpdateMoveStatusEvent;
        public event EventHandler<PositionArgs> OnUpdatePositionArgsEvent;
        public event EventHandler<MessageHandlerArgs> OnLogDebugEvent;
        public event EventHandler<MessageHandlerArgs> OnLogErrorEvent;
        public event EventHandler<bool> OnOpPauseOrResumeEvent;

        public MapInfo MapInfo { get; set; }
        public MovingGuide MovingGuide { get; set; }
        public MoveStatus MoveStatus { get; set; }
        public PositionArgs PositionArgs { get; set; }
        public bool IsFakeMoveEnginePause { get; set; }
        public Task FakeMoveEngineTask { get; set; }
        public ConcurrentQueue<PositionArgs> FakeMoveArrivalQueue { get; set; }

        public NullObjMoveHandler(Model.MoveStatus moveStatus, MapInfo mapInfo)
        {
            this.MoveStatus = moveStatus;
            this.MapInfo = mapInfo;
            moveStatus.LastMapPosition = MapInfo.addressMap.First().Value.Position;
            FakeMoveArrivalQueue = new ConcurrentQueue<PositionArgs>();
            IsFakeMoveEnginePause = false;
            FakeMoveEngineTask = new Task(FakeMoveEngine);
            FakeMoveEngineTask.Start();
        }

        public void GetMoveStatus()
        {
            OnUpdateMoveStatusEvent?.Invoke(this, MoveStatus);
        }

        public void SetMoveStatusFrom(MoveStatus moveStatus)
        {
            this.MoveStatus = moveStatus;
        }

        public void PauseMove()
        {
            IsFakeMoveEnginePause = true;
            if (MoveStatus.EnumMoveState == EnumMoveState.Working)
            {
                MoveStatus.EnumMoveState = EnumMoveState.Pause;
                OnUpdateMoveStatusEvent?.Invoke(this, MoveStatus);
            }
        }

        public void ReserveOkPartMove(MapSection mapSection)
        {
            try
            {
                int sectionIndex = MovingGuide.GuideSectionIds.FindIndex(x => x.Trim() == mapSection.Id.Trim());
                if (sectionIndex < 0)
                {
                    OnLogErrorEvent?.Invoke(this, new MessageHandlerArgs()
                    {
                        ClassMethodName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name,
                        Message = $"ReserveOkPartMove fail.[{mapSection.Id.Trim()}].[{MovingGuide.GuideSectionIds.GetJsonInfo()}]"
                    });

                    return;
                }
                MapAddress address = MapInfo.addressMap[MovingGuide.GuideAddressIds[sectionIndex + 1]];

                PositionArgs positionArgs = new PositionArgs()
                {
                    EnumLocalArrival = sectionIndex == MovingGuide.GuideSectionIds.Count - 1 ? EnumLocalArrival.EndArrival : EnumLocalArrival.Arrival,
                    MapPosition = address.Position
                };

                FakeMoveArrivalQueue.Enqueue(positionArgs);
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

        public void ResumeMove()
        {
            if (MoveStatus.EnumMoveState == EnumMoveState.Pause)
            {
                MoveStatus.EnumMoveState = EnumMoveState.Working;
                OnUpdateMoveStatusEvent?.Invoke(this, MoveStatus);
            }
            IsFakeMoveEnginePause = false;
        }

        public void SetMovingGuide(MovingGuide movingGuide)
        {
            MovingGuide = movingGuide;
        }

        public void StopMove()
        {
            FakeMoveArrivalQueue = new ConcurrentQueue<PositionArgs>();
            if (MoveStatus.EnumMoveState == EnumMoveState.Working)
            {
                FakeMoveArrivalQueue.Enqueue(new PositionArgs()
                {
                    EnumLocalArrival = EnumLocalArrival.Fail,
                    MapPosition = MoveStatus.LastMapPosition
                });
            }
            else
            {
                MoveStatus.EnumMoveState = EnumMoveState.Idle;
                OnUpdateMoveStatusEvent?.Invoke(this, MoveStatus);
            }

        }

        private void FakeMoveEngine()
        {
            while (true)
            {
                if (IsFakeMoveEnginePause)
                {
                    SpinWait.SpinUntil(() => IsFakeMoveEnginePause, 1000);
                    continue;
                }

                try
                {
                    if (FakeMoveArrivalQueue.Any())
                    {
                        FakeMoveArrivalQueue.TryDequeue(out PositionArgs positionArgs);
                        OnUpdatePositionArgsEvent?.Invoke(this, positionArgs);
                    }
                    else
                    {
                        FakeMoveArrivalQueue.Enqueue(new PositionArgs()
                        {
                            EnumLocalArrival = EnumLocalArrival.Arrival,
                            MapPosition = MoveStatus.LastMapPosition
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

                SpinWait.SpinUntil(() => false, 5000);
            }
        }

        public void InitialPosition()
        {
            FakeMoveArrivalQueue.Enqueue(new PositionArgs()
            {
                EnumLocalArrival = EnumLocalArrival.Arrival,
                MapPosition = MoveStatus.LastMapPosition
            });
        }

        public void PartMoveBegin()
        {
            FakeMoveArrivalQueue.Enqueue(new PositionArgs()
            {
                EnumLocalArrival = EnumLocalArrival.Arrival,
                MapPosition = MoveStatus.LastMapPosition
            });
        }

        public void PartMoveEnd(EnumSlotSelect openSlot = EnumSlotSelect.None)
        {
            FakeMoveArrivalQueue.Enqueue(new PositionArgs()
            {
                EnumLocalArrival = EnumLocalArrival.EndArrival,
                MapPosition = MoveStatus.LastMapPosition
            });
        }        
    }
}
