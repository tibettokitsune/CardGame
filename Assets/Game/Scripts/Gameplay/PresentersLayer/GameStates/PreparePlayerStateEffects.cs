using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.DataLayer;
using Game.Scripts.Gameplay.PresentersLayer.Contracts.UI;
using Game.Scripts.Infrastructure.SceneManagment;
using Game.Scripts.Infrastructure.TimeManagement;
using Game.Scripts.UI;
using UnityEngine;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    [EffectOrder(-20)]
    public class LoadPrepareSceneEffect : IStateEnterEffect<PreparePlayerState>
    {
        private readonly ISceneManagerService _sceneManagerService;

        public LoadPrepareSceneEffect(ISceneManagerService sceneManagerService)
        {
            _sceneManagerService = sceneManagerService;
        }

        public async Task OnEnterAsync()
        {
            try
            {
                await _sceneManagerService.LoadScene("GameplayPrepare", SceneLayer.GameplayElement, true);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }

    [EffectOrder(-10)]
    public class ShowPrepareTimerUiEffect : IStateEnterEffect<PreparePlayerState>
    {
        private readonly IUIService _uiService;

        public ShowPrepareTimerUiEffect(IUIService uiService)
        {
            _uiService = uiService;
        }

        public async Task OnEnterAsync()
        {
            await _uiService.ShowAsync<ITimerScreen>();
        }
    }

    [EffectOrder(0)]
    public class PrepareRoundTimerEffect : IStateEnterEffect<PreparePlayerState>, IStateExitEffect<PreparePlayerState>, ITimerHandler
    {
        private const string PrepareRoundTimerId = "PrepareToRound";
        private static readonly TimeSpan PrepareRoundDuration = TimeSpan.FromSeconds(100);

        private readonly ITimerService _timerService;
        private readonly ITimerUpdateService _timerUpdateService;
        private readonly ILobbyDataProvider _lobbyDataProvider;
        private bool _timerHandlerRegistered;

        public PrepareRoundTimerEffect(
            ITimerService timerService,
            ITimerUpdateService timerUpdateService,
            ILobbyDataProvider lobbyDataProvider)
        {
            _timerService = timerService;
            _timerUpdateService = timerUpdateService;
            _lobbyDataProvider = lobbyDataProvider;
        }

        public async Task OnEnterAsync()
        {
            _timerService.SetupTimer(PrepareRoundTimerId, PrepareRoundTimerId, PrepareRoundDuration);
            RegisterTimerHandler();
            await Task.CompletedTask;
        }

        public async Task OnExitAsync()
        {
            _timerService.DeleteTimer(PrepareRoundTimerId);
            UnregisterTimerHandler();
            await Task.CompletedTask;
        }

        public IEnumerable<string> Sources => new[] { PrepareRoundTimerId };

        public void Handle(ITimerModel model)
        {
            _timerService.DeleteTimer(PrepareRoundTimerId);
            UnregisterTimerHandler();

            if (_lobbyDataProvider.LobbyState.Value != LobbyState.PrepareToRound)
                return;

            _lobbyDataProvider.LobbyState.Value = LobbyState.TakeEventCard;
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

    [EffectOrder(0)]
    public class CleanupPreparePlayerEffect : IStateExitRequestEffect<PreparePlayerState>
    {
        private readonly IUIService _uiService;
        private readonly ISceneManagerService _sceneManagerService;

        public CleanupPreparePlayerEffect(IUIService uiService, ISceneManagerService sceneManagerService)
        {
            _uiService = uiService;
            _sceneManagerService = sceneManagerService;
        }

        public async Task OnExitRequestAsync()
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
        }
    }
}
