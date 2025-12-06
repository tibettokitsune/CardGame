using System.Collections.Generic;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public class FirstEnterInGameState : EffectDrivenState<FirstEnterInGameState>
    {
        public FirstEnterInGameState(
            IEnumerable<IStateEnterEffect<FirstEnterInGameState>> enterEffects,
            IEnumerable<IStateExitEffect<FirstEnterInGameState>> exitEffects,
            IEnumerable<IStateExitRequestEffect<FirstEnterInGameState>> exitRequestEffects)
            : base(enterEffects, exitEffects, exitRequestEffects, needsExitTime: false, isGhostState: false)
        {
        }
    }
}
