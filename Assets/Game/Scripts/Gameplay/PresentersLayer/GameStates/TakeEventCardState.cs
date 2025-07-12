using Game.Scripts.Infrastructure.SceneManagment;
using Game.Scripts.UI;
using UnityEngine;
using UnityHFSM;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public class TakeEventCardState : StateBase
    {
        private readonly ISceneManagerService _sceneManagerService;

        public TakeEventCardState(ISceneManagerService sceneManagerService) 
            : base(needsExitTime: false, isGhostState: false)
        {
            _sceneManagerService = sceneManagerService;
        }

        public override void OnEnter()
        {
            Debug.Log("TakeEventCardState Enter");
            _sceneManagerService.LoadScene("GameplayTakeEvent", SceneLayer.GameplayElement, false);
            _sceneManagerService.LoadScene("Cliffs_red_cave", SceneLayer.GameplayElement, true);
        }

        public override void OnExit()
        {
            Debug.Log("TakeEventCardState Exit");
            _sceneManagerService.UnloadScene("GameplayTakeEvent", SceneLayer.GameplayElement);
            _sceneManagerService.UnloadScene("Cliffs_red_cave", SceneLayer.GameplayElement);
        }
    }
}