using Mirle.Agv.Utmc.Model;
using Mirle.Agv.Utmc.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Move
{
    public interface IMoveHandler : Tools.IMessageHandler
    {
        public event EventHandler<Model.MoveStatus> OnUpdateMoveStatusEvent;

        public event EventHandler<Model.PositionArgs> OnUpdatePositionArgsEvent;

        public event EventHandler<bool> OnOpPauseOrResumeEvent;

        public void SetMovingGuide(Model.MovingGuide movingGuide);
        public void PartMoveBegin();
        public void PartMoveEnd(EnumSlotSelect openSlot = EnumSlotSelect.None);
        public void ReserveOkPartMove(Model.MapSection mapSection);
        public void StopMove();
        public void PauseMove();
        public void ResumeMove();
        public void GetMoveStatus();
        public void SetMoveStatusFrom(MoveStatus moveStatus);
        public void InitialPosition();

    }
}
