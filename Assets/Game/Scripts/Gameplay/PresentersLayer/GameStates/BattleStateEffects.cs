using System;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.DataLayer;
using Game.Scripts.Infrastructure.SceneManagment;
using UnityEngine;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public interface IBattleVictoryUseCase
    {
        Task ExecuteAsync();
    }

    public class BattleVictoryUseCase : IBattleVictoryUseCase
    {
        public async Task ExecuteAsync()
        {
            Debug.Log("Battle victory reward granted (stub).");
            await Task.CompletedTask;
        }
    }

    [EffectOrder(-10)]
    public class LoadBattleSceneEffect : IStateEnterEffect<BattleState>
    {
        private readonly ISceneManagerService _sceneManagerService;

        public LoadBattleSceneEffect(ISceneManagerService sceneManagerService)
        {
            _sceneManagerService = sceneManagerService;
        }

        public async Task OnEnterAsync()
        {
            try
            {
                
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }

    [EffectOrder(0)]
    public class BattleFlowEffect : IStateEnterEffect<BattleState>
    {
        private static readonly TimeSpan BattleDuration = TimeSpan.FromSeconds(5);

        private readonly IBattleVictoryUseCase _battleVictoryUseCase;
        private readonly ILobbyDataProvider _lobbyDataProvider;

        public BattleFlowEffect(
            IBattleVictoryUseCase battleVictoryUseCase,
            ILobbyDataProvider lobbyDataProvider)
        {
            _battleVictoryUseCase = battleVictoryUseCase;
            _lobbyDataProvider = lobbyDataProvider;
        }

        public async Task OnEnterAsync()
        {
            try
            {
                await Task.Delay(BattleDuration);
                await _battleVictoryUseCase.ExecuteAsync();
                _lobbyDataProvider.LobbyState.Value = LobbyState.PrepareToRound;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }

    [EffectOrder(0)]
    public class UnloadBattleSceneEffect : IStateExitRequestEffect<BattleState>
    {
        private readonly ISceneManagerService _sceneManagerService;

        public UnloadBattleSceneEffect(ISceneManagerService sceneManagerService)
        {
            _sceneManagerService = sceneManagerService;
        }

        public async Task OnExitRequestAsync()
        {
            try
            {
                
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
