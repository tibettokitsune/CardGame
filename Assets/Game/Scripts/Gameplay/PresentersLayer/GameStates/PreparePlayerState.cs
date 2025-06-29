using Game.Scripts.Infrastructure.TimeManagement;
using Game.Scripts.UI;
using UnityEngine;
using UnityHFSM;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public class PreparePlayerState : StateBase
    {
        private readonly IUIService _uiService;

        public PreparePlayerState(IUIService uiService) 
            : base(needsExitTime: false, isGhostState: false)
        {
            _uiService = uiService;
        }

        public override void OnEnter()
        {
            Debug.Log("PreparePlayerState Enter");
            _uiService.ShowScreen("TimerScreen");
        }

        public override void OnExit()
        {
            Debug.Log("PreparePlayerState Exit");
            _uiService.Clear();
        }
    }
}