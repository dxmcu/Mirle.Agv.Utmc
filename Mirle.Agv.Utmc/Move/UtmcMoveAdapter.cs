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

        public void GetMoveStatus()
        {
            
        }

        public void InitialPosition()
        {
            
        }

        public void PartMoveBegin()
        {
            
        }

        public void PartMoveEnd(EnumSlotSelect openSlot = EnumSlotSelect.None)
        {
            
        }

        public void PauseMove()
        {
           
        }

        public void ReserveOkPartMove(MapSection mapSection)
        {
            
        }

        public void ResumeMove()
        {
            
        }

        public void SetMoveStatusFrom(MoveStatus moveStatus)
        {
            
        }

        public void SetMovingGuide(MovingGuide movingGuide)
        {
           
        }

        public void StopMove()
        {
           
        }
    }
}
