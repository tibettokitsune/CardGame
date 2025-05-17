using UnityEngine;
using UnityHFSM;

namespace Game.Scripts.Gameplay.Lobby.GameStates
{
    public class PreparePlayerState : StateBase
    {
        public PreparePlayerState() 
            : base(needsExitTime: false, isGhostState: false)
        {
        }

        public override void OnEnter()
        {
            Debug.Log("PreparePlayerState Enter");
        }

        public override void OnExit()
        {
            Debug.Log("PreparePlayerState Exit");
        }
    }
}