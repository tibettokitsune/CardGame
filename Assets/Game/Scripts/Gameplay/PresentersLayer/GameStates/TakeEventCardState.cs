using System;
using System.Collections.Generic;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public class TakeEventCardState : EffectDrivenState<TakeEventCardState>
    {
        public TakeEventCardState(
            IEnumerable<IStateEnterEffect<TakeEventCardState>> enterEffects,
            IEnumerable<IStateExitEffect<TakeEventCardState>> exitEffects,
            IEnumerable<IStateExitRequestEffect<TakeEventCardState>> exitRequestEffects)
            : base(enterEffects, exitEffects, exitRequestEffects, needsExitTime: true, isGhostState: false)
        {
        }
    }
}
