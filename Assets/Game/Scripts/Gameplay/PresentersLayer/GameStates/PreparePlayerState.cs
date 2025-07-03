using Game.Scripts.Infrastructure.SceneManagment;
using Game.Scripts.Infrastructure.TimeManagement;
using Game.Scripts.UI;
using UnityEngine;
using UnityHFSM;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public class PreparePlayerState : StateBase
    {
        private readonly IUIService _uiService;
        private readonly ISceneManagerService _sceneManagerService;
        
        public PreparePlayerState(IUIService uiService, ISceneManagerService sceneManagerService) 
            : base(needsExitTime: false, isGhostState: false)
        {
            _uiService = uiService;
            _sceneManagerService = sceneManagerService;
        }

        public override void OnEnter()
        {
            Debug.Log("PreparePlayerState Enter");
            _sceneManagerService.LoadScene("GameplayPrepare", SceneLayer.GameplayElement, false);
            _sceneManagerService.LoadScene("Rocks_ice_cave", SceneLayer.GameplayElement, true);
            _uiService.ShowScreen("TimerScreen");
        }

        public override void OnExit()
        {
            Debug.Log("PreparePlayerState Exit");
            _sceneManagerService.UnloadScene("GameplayPrepare", SceneLayer.GameplayElement);
            _sceneManagerService.UnloadScene("Rocks_ice_cave", SceneLayer.GameplayElement);
            _uiService.Clear();
        }
    }
}