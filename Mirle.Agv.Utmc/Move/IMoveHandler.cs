using Mirle.Agv.Utmc.Model;
using Mirle.Agv.Utmc.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Utmc.Move
{
    interface IMoveHandler : Tools.IMessageHandler
    {
        event EventHandler<Model.MoveStatus> OnUpdateMoveStatusEvent;

        event EventHandler<Model.PositionArgs> OnUpdatePositionArgsEvent;

        event EventHandler<bool> OnOpPauseOrResumeEvent;

        void SetMovingGuide(Model.MovingGuide movingGuide);
        void ReserveOkPartMove(Model.MapSection mapSection);
        void StopMove();
        void PauseMove();
        void ResumeMove();
        void GetMoveStatus();
        void SetMoveStatusFrom(MoveStatus moveStatus);
        void InitialPosition();
    }
}
