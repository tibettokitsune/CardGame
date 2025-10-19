using System;
using System.Threading.Tasks;
using Game.Scripts.Infrastructure.SceneManagment;
using UnityEngine;
using UnityHFSM;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public class BattleState : StateBase
    {
        private readonly ISceneManagerService _sceneManagerService;
        private bool _isExitInProgress;

        public BattleState(ISceneManagerService sceneManagerService) 
            : base(needsExitTime: true, isGhostState: false)
        {
            _sceneManagerService = sceneManagerService;
        }

        public override async void OnEnter()
        {
            Debug.Log("Battle Enter");
            try
            {
                await _sceneManagerService.LoadScene("Battle", SceneLayer.GameplayElement, false);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public override void OnExit()
        {
            Debug.Log("Battle Exit");
            _isExitInProgress = false;
        }

        public override void OnExitRequest()
        {
            if (_isExitInProgress)
                return;

            _isExitInProgress = true;
            _ = ExitAsync();
        }

        private async Task ExitAsync()
        {
            try
            {
                await _sceneManagerService.UnloadScene("Battle", SceneLayer.GameplayElement);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                _isExitInProgress = false;
                fsm?.StateCanExit();
            }
        }
    }
}
