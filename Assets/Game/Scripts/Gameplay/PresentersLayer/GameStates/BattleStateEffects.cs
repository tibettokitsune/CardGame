using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Scripts.Gameplay.DataLayer;
using Game.Scripts.Gameplay.DataLayer.Battle;
using Game.Scripts.Gameplay.Lobby.Player;
using Game.Scripts.Gameplay.PresentersLayer.Battle;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.Infrastructure.SceneManagment;
using Game.Scripts.Infrastructure.Configs.Configs;
using Game.Scripts.Infrastructure.UI;
using Game.Scripts.Infrastructure.TimeManagement;
using Game.Scripts.UI;
using Game.Scripts.UIContracts;
using UnityEngine;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public interface IBattleVictoryUseCase
    {
        Task ExecuteAsync(float reward);
    }

    public class BattleVictoryUseCase : IBattleVictoryUseCase
    {
        public async Task ExecuteAsync(float reward)
        {
            Debug.Log($"Battle victory reward granted (stub). Reward: {reward}.");
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

        public async Task OnEnterAsync() => await Task.CompletedTask;
    }

    [EffectOrder(0)]
    public class BattleFlowEffect : IStateEnterEffect<BattleState>, IStateExitEffect<BattleState>
    {
        private readonly IBattleVictoryUseCase _battleVictoryUseCase;
        private readonly ILobbyDataProvider _lobbyDataProvider;
        private readonly IPlayerPresenter _playerPresenter;
        private readonly IPlayerDataProvider _playerDataProvider;
        private readonly IBattleDataProvider _battleDataProvider;
        private readonly IBattlePreparationCoordinator _battlePreparationCoordinator;
        private readonly IUIService _uiService;

        private CancellationTokenSource _cancellation;

        public BattleFlowEffect(
            IBattleVictoryUseCase battleVictoryUseCase,
            ILobbyDataProvider lobbyDataProvider,
            IPlayerPresenter playerPresenter,
            IPlayerDataProvider playerDataProvider,
            IBattleDataProvider battleDataProvider,
            IBattlePreparationCoordinator battlePreparationCoordinator,
            IUIService uiService)
        {
            _battleVictoryUseCase = battleVictoryUseCase;
            _lobbyDataProvider = lobbyDataProvider;
            _playerPresenter = playerPresenter;
            _playerDataProvider = playerDataProvider;
            _battleDataProvider = battleDataProvider;
            _battlePreparationCoordinator = battlePreparationCoordinator;
            _uiService = uiService;
        }

        public async Task OnEnterAsync()
        {
            try
            {
                _cancellation = new CancellationTokenSource();
                var monster = _playerPresenter.CurrentDoor.Value as MonsterCardViewData;
                if (monster == null)
                {
                    Debug.LogWarning("Cannot start battle: monster card is missing.");
                    _lobbyDataProvider.LobbyState.Value = LobbyState.PrepareToRound;
                    return;
                }

                var playerHealth = ResolvePlayerHealth();
                _battleDataProvider.Reset();
                _battleDataProvider.StartBattle(BuildOpponent(monster), playerHealth);
                _playerPresenter.SyncBattleHand();

                await ShowBattleUiAsync();
                await RunBattleLoopAsync(_cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public async Task OnExitAsync()
        {
            _battlePreparationCoordinator.Cancel();
            _cancellation?.Cancel();
            await HideBattleUiAsync();
            _battleDataProvider.Reset();
        }

        private async Task RunBattleLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _battleDataProvider.IsBattleActive)
            {
                await RunPreparationStageAsync(token);
                if (token.IsCancellationRequested || !_battleDataProvider.IsBattleActive)
                    break;

                await RunStrikeStageAsync(token);
                var hasWinner = _battleDataProvider.HasWinner(out var playerWon);
                await RunOutcomeStageAsync(hasWinner, token);

                if (hasWinner)
                {
                    await HandleBattleResultAsync(playerWon, token);
                    break;
                }
            }
        }

        private async Task RunPreparationStageAsync(CancellationToken token)
        {
            _battleDataProvider.Phase.Value = BattlePhase.Preparation;
            _battlePreparationCoordinator.Begin();
            await _battlePreparationCoordinator.WaitForCompletionAsync(token);
        }

        private async Task RunStrikeStageAsync(CancellationToken token)
        {
            _battleDataProvider.Phase.Value = BattlePhase.Strike;
            token.ThrowIfCancellationRequested();

            var monster = _battleDataProvider.CurrentMonster.Value;
            var playerDamage = ResolvePlayerAttack();
            var monsterDamage = monster?.Damage ?? 0f;

            _battleDataProvider.ApplyExchange(playerDamage, monsterDamage);
            ApplyDamageToPlayer(monsterDamage);
            await Task.CompletedTask;
        }

        private async Task RunOutcomeStageAsync(bool hasWinner, CancellationToken token)
        {
            _battleDataProvider.Phase.Value = BattlePhase.Outcome;
            if (!hasWinner)
                await Task.Delay(250, token);
        }

        private async Task HandleBattleResultAsync(bool playerWon, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (playerWon)
            {
                var reward = _battleDataProvider.CurrentMonster.Value?.Reward ?? 0f;
                await _battleVictoryUseCase.ExecuteAsync(reward);
            }
            else
            {
                Debug.Log("Player lost the battle.");
            }

            _lobbyDataProvider.LobbyState.Value = LobbyState.PrepareToRound;
        }

        private float ResolvePlayerHealth()
        {
            if (_playerDataProvider.PlayersStats.TryGetValue(PlayerStat.Health, out var health) && health > 0)
                return health;

            return 1f;
        }

        private float ResolvePlayerAttack()
        {
            return _playerDataProvider.PlayersStats.TryGetValue(PlayerStat.Attack, out var attack)
                ? attack
                : 0f;
        }

        private void ApplyDamageToPlayer(float damage)
        {
            if (damage <= 0)
                return;

            var currentHealth = ResolvePlayerHealth();
            var newHealth = Math.Max(0, currentHealth - damage);
            _playerDataProvider.PlayersStats[PlayerStat.Health] = newHealth;
            _battleDataProvider.PlayerHealth.Value = newHealth;
        }

        private static BattleOpponentData BuildOpponent(MonsterCardViewData monster)
        {
            var parameters = monster.Parameters ?? new MonsterParameters();
            return new BattleOpponentData
            {
                Id = monster.Id,
                Name = monster.Name,
                Damage = parameters.Damage,
                Reward = parameters.Reward,
                Health = parameters.Health,
                ViewId = monster.ViewId
            };
        }

        private async Task ShowBattleUiAsync()
        {
            await UniTask.WhenAll(
                _uiService.ShowAsync<IBattleHudScreen>(),
                _uiService.ShowAsync<IBattleHandScreen>(),
                _uiService.ShowAsync<IBattleStageScreen>());
        }

        private async Task HideBattleUiAsync()
        {
            await UniTask.WhenAll(
                _uiService.HideAsync<IBattleHudScreen>(),
                _uiService.HideAsync<IBattleHandScreen>(),
                _uiService.HideAsync<IBattleStageScreen>());
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
                await Task.CompletedTask;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
