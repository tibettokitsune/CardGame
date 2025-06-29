using Game.Scripts.Infrastructure.SceneManagment;
using Game.Scripts.UI;
using UnityEngine;
using UnityHFSM;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public class TakeEventCardState : StateBase
    {
        private readonly IUIService _uiService;
        private readonly ISceneManagerService _sceneManagerService;

        public TakeEventCardState(IUIService uiService, ISceneManagerService sceneManagerService) 
            : base(needsExitTime: false, isGhostState: false)
        {
            _uiService = uiService;
            _sceneManagerService = sceneManagerService;
        }

        public override void OnEnter()
        {
            Debug.Log("TakeEventCardState Enter");
            _sceneManagerService.LoadScene("GameplayTakeEvent", SceneLayer.GameplayElement, true);
        }

        public override void OnExit()
        {
            Debug.Log("TakeEventCardState Exit");
            _sceneManagerService.UnloadScene("GameplayTakeEvent", SceneLayer.GameplayElement);
        }
    }
}