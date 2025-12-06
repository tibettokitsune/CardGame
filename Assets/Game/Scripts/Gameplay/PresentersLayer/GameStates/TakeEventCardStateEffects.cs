using System;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.DataLayer;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.Infrastructure.SceneManagment;
using UnityEngine;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public interface IFinishTakeEventCardStateUseCase
    {
        void Execute();
    }

    [EffectOrder(-10)]
    public class TakeEventCardFlowEffect : IStateEnterEffect<TakeEventCardState>
    {
        private readonly ITakeEventCardUseCase _takeEventCardUseCase;
        private readonly ISceneManagerService _sceneManagerService;

        public TakeEventCardFlowEffect(
            ITakeEventCardUseCase takeEventCardUseCase,
            ISceneManagerService sceneManagerService)
        {
            _takeEventCardUseCase = takeEventCardUseCase;
            _sceneManagerService = sceneManagerService;
        }

        public async Task OnEnterAsync()
        {
            try
            {
                await _takeEventCardUseCase.Execute();
                await _sceneManagerService.LoadScene("GameplayTakeEvent", SceneLayer.GameplayElement, false);
                await _sceneManagerService.LoadScene("Cliffs_red_cave", SceneLayer.GameplayElement, true);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }

    [EffectOrder(0)]
    public class UnloadEventCardScenesEffect : IStateExitRequestEffect<TakeEventCardState>
    {
        private readonly ISceneManagerService _sceneManagerService;

        public UnloadEventCardScenesEffect(ISceneManagerService sceneManagerService)
        {
            _sceneManagerService = sceneManagerService;
        }

        public async Task OnExitRequestAsync()
        {
            try
            {
                await _sceneManagerService.UnloadScene("GameplayTakeEvent", SceneLayer.GameplayElement);
                await _sceneManagerService.UnloadScene("Cliffs_red_cave", SceneLayer.GameplayElement);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }

    public class FinishTakeEventCardStateUseCase : IFinishTakeEventCardStateUseCase
    {
        private readonly ILobbyDataProvider _lobbyDataProvider;

        public FinishTakeEventCardStateUseCase(ILobbyDataProvider lobbyDataProvider)
        {
            _lobbyDataProvider = lobbyDataProvider;
        }

        public void Execute()
        {
            _lobbyDataProvider.LobbyState.Value = LobbyState.Battle;
        }
    }
}
