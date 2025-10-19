using System;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.PresentersLayer.Contracts.UI;
using Game.Scripts.Infrastructure.SceneManagment;
using Game.Scripts.UI;
using UnityEngine;
using UnityHFSM;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public class PreparePlayerState : StateBase
    {
        private readonly IUIService _uiService;
        private readonly ISceneManagerService _sceneManagerService;
        private bool _isExitInProgress;
        
        public PreparePlayerState(IUIService uiService, ISceneManagerService sceneManagerService) 
            : base(needsExitTime: true, isGhostState: false)
        {
            _uiService = uiService;
            _sceneManagerService = sceneManagerService;
        }

        public override async void OnEnter()
        {
            Debug.Log("PreparePlayerState Enter");
            try
            {
                await _sceneManagerService.LoadScene("GameplayPrepare", SceneLayer.GameplayElement, true);
                await _uiService.ShowAsync<ITimerScreen>();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public override void OnExit()
        {
            Debug.Log("PreparePlayerState Exit");
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
                await _uiService.ClearAsync();
                await _sceneManagerService.UnloadScene("GameplayPrepare", SceneLayer.GameplayElement);
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
