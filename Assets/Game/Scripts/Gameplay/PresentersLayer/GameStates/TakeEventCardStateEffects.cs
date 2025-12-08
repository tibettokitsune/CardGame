using System;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.DataLayer;
using Game.Scripts.Gameplay.DataLayer.Models;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.UI;
using Game.Scripts.UIContracts;
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
        private readonly IPlayerPresenter _playerPresenter;
        private readonly IUIService _uiService;
        private readonly IFinishTakeEventCardStateUseCase _finishTakeEventCardStateUseCase;

        public TakeEventCardFlowEffect(
            ITakeEventCardUseCase takeEventCardUseCase,
            IPlayerPresenter playerPresenter,
            IUIService uiService,
            IFinishTakeEventCardStateUseCase finishTakeEventCardStateUseCase)
        {
            _takeEventCardUseCase = takeEventCardUseCase;
            _playerPresenter = playerPresenter;
            _uiService = uiService;
            _finishTakeEventCardStateUseCase = finishTakeEventCardStateUseCase;
        }

        public async Task OnEnterAsync()
        {
            try
            {
                await _takeEventCardUseCase.Execute();

                var screen = await _uiService.ShowAsync<IOpenDoorScreen>();
                if (_playerPresenter.CurrentDoor.Value != null)
                    await screen.ShowDoorCard(_playerPresenter.CurrentDoor.Value);
                else
                    Debug.LogWarning("No current door card to show.");

                await Task.Delay(3000);
                await _uiService.ClearAsync();
                _finishTakeEventCardStateUseCase.Execute();
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

    public class FinishTakeEventCardStateUseCase : IFinishTakeEventCardStateUseCase
    {
        private readonly ILobbyDataProvider _lobbyDataProvider;
        private readonly IPlayerPresenter _playerPresenter;

        public FinishTakeEventCardStateUseCase(
            ILobbyDataProvider lobbyDataProvider,
            IPlayerPresenter playerPresenter)
        {
            _lobbyDataProvider = lobbyDataProvider;
            _playerPresenter = playerPresenter;
        }

        public void Execute()
        {
            var doorCard = _playerPresenter.CurrentDoor.Value;
            if (doorCard == null)
            {
                Debug.LogWarning("Cannot resolve next state: player has no current door card.");
                _lobbyDataProvider.LobbyState.Value = LobbyState.PrepareToRound;
                return;
            }

            _lobbyDataProvider.LobbyState.Value = ResolveNextState(doorCard);
        }

        private static LobbyState ResolveNextState(DoorCardViewData doorCard)
        {
            var typeId = CardTypeUtils.Normalize(doorCard.TypeId);
            return CardTypeUtils.IsMonster(typeId)
                ? LobbyState.Battle
                : LobbyState.PrepareToRound;
        }
    }
}
