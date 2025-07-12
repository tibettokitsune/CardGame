using Game.Scripts.Infrastructure.SceneManagment;
using UnityEngine;
using UnityHFSM;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public class BattleState : StateBase
    {
        private readonly ISceneManagerService _sceneManagerService;

        public BattleState(ISceneManagerService sceneManagerService) 
            : base(needsExitTime: false, isGhostState: false)
        {
            _sceneManagerService = sceneManagerService;
        }

        public override void OnEnter()
        {
            Debug.Log("Battle Enter");
            _sceneManagerService.LoadScene("Battle", SceneLayer.GameplayElement, false);
        }

        public override void OnExit()
        {
            Debug.Log("Battle Exit");
            _sceneManagerService.UnloadScene("Battle", SceneLayer.GameplayElement);
        }
    }
}