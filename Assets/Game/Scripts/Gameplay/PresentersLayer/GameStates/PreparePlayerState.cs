using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.DataLayer;
using Game.Scripts.Gameplay.PresentersLayer.Contracts.UI;
using Game.Scripts.Infrastructure.SceneManagment;
using Game.Scripts.Infrastructure.TimeManagement;
using Game.Scripts.UI;
using UnityEngine;
using UnityHFSM;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public class PreparePlayerState : StateBase, ITimerHandler
    {
        private readonly IUIService _uiService;
        private readonly ISceneManagerService _sceneManagerService;
        private readonly ITimerService _timerService;
        private readonly ITimerUpdateService _timerUpdateService;
        private readonly ILobbyDataProvider _lobbyDataProvider;
        private bool _timerHandlerRegistered;
        private bool _isExitInProgress;
        private static readonly TimeSpan PrepareRoundDuration = TimeSpan.FromSeconds(100);
        private const string PrepareRoundTimerId = "PrepareToRound";
        
        public PreparePlayerState(
            IUIService uiService,
            ISceneManagerService sceneManagerService,
            ITimerService timerService,
            ITimerUpdateService timerUpdateService,
            ILobbyDataProvider lobbyDataProvider) 
            : base(needsExitTime: true, isGhostState: false)
        {
            _uiService = uiService;
            _sceneManagerService = sceneManagerService;
            _timerService = timerService;
            _timerUpdateService = timerUpdateService;
            _lobbyDataProvider = lobbyDataProvider;
        }

        public override async void OnEnter()
        {
            Debug.Log("PreparePlayerState Enter");
            try
            {
                await _sceneManagerService.LoadScene("GameplayPrepare", SceneLayer.GameplayElement, true);
                await _uiService.ShowAsync<ITimerScreen>();
                StartPrepareRoundTimer();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public override void OnExit()
        {
            Debug.Log("PreparePlayerState Exit");
            StopPrepareRoundTimer();
            _isExitInProgress = false;
        }

        public override void OnExitRequest()
        {
            if (_isExitInProgress)
                return;

            _isExitInProgress = true;
            _ = ExitAsync();
        }

        public IEnumerable<string> Sources => new[] { PrepareRoundTimerId };

        public void Handle(ITimerModel model)
        {
            StopPrepareRoundTimer();

            if (_lobbyDataProvider.LobbyState.Value != LobbyState.PrepareToRound)
                return;

            _lobbyDataProvider.LobbyState.Value = LobbyState.TakeEventCard;
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

        private void StartPrepareRoundTimer()
        {
            _timerService.SetupTimer(PrepareRoundTimerId, PrepareRoundTimerId, PrepareRoundDuration);
            RegisterTimerHandler();
        }

        private void StopPrepareRoundTimer()
        {
            _timerService.DeleteTimer(PrepareRoundTimerId);
            UnregisterTimerHandler();
        }

        private void RegisterTimerHandler()
        {
            if (_timerHandlerRegistered)
                return;

            _timerUpdateService.RegisterHandler(this);
            _timerHandlerRegistered = true;
        }

        private void UnregisterTimerHandler()
        {
            if (!_timerHandlerRegistered)
                return;

            _timerUpdateService.UnRegisterHandler(this);
            _timerHandlerRegistered = false;
        }
    }
}
