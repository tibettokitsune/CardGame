using System.Collections.Generic;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public class BattleState : EffectDrivenState<BattleState>
    {
        public BattleState(
            IEnumerable<IStateEnterEffect<BattleState>> enterEffects,
            IEnumerable<IStateExitEffect<BattleState>> exitEffects,
            IEnumerable<IStateExitRequestEffect<BattleState>> exitRequestEffects)
            : base(enterEffects, exitEffects, exitRequestEffects, needsExitTime: true, isGhostState: false)
        {
        }
    }
}
