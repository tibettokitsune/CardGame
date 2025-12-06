using System;
using System.Collections.Generic;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public class PreparePlayerState : EffectDrivenState<PreparePlayerState>
    {
        public PreparePlayerState(
            IEnumerable<IStateEnterEffect<PreparePlayerState>> enterEffects,
            IEnumerable<IStateExitEffect<PreparePlayerState>> exitEffects,
            IEnumerable<IStateExitRequestEffect<PreparePlayerState>> exitRequestEffects)
            : base(enterEffects, exitEffects, exitRequestEffects, needsExitTime: true, isGhostState: false)
        {
        }
    }
}
