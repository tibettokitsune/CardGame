using Game.Scripts.Gameplay.Lobby.Player;
using Game.Scripts.UI;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.Lobby
{
    public interface ILobbyPresenter
    {
    }
    
    public class LobbyPresenter : ILobbyPresenter, IInitializable
    {
        private readonly PreparePlayerState _preparePlayerState;
        private readonly FirstEnterInGameState _firstEnterInGameState;
        
        private GameStateMachine _gameStateMachine;

        public LobbyPresenter(PreparePlayerState preparePlayerState, 
            FirstEnterInGameState firstEnterInGameState
            )
        {
            _preparePlayerState = preparePlayerState;
            _firstEnterInGameState = firstEnterInGameState;
        }

        public void Initialize()
        {
            Debug.Log("LobbyPresenter Initialize");
            _gameStateMachine = new GameStateMachine();
            _gameStateMachine.SetState(_firstEnterInGameState);
        }
    }
    
    public interface IState
    {
        public void Enter();
        public void Exit();
    }
    
    public class FirstEnterInGameState : IState
    {
        private readonly IPlayerPresenter _playerPresenter;
        private readonly IUIService<UIScreen> _uiService;

        public FirstEnterInGameState(IPlayerPresenter playerPresenter, IUIService<UIScreen> uiService)
        {
            _playerPresenter = playerPresenter;
            _uiService = uiService;
        }

        public async void Enter()
        {
            Debug.Log("FirstEnterInGameState Enter");
            await _uiService.ShowScreen<CharacterActiveCardsScreen>("CharacterActiveCards");
            await _uiService.ShowScreen<CharacterStatsScreen>("CharacterStats");
            FillPlayerStartHand();
        }

        private async void FillPlayerStartHand()
        {
            await _playerPresenter.FillStartHand();
        }

        public void Exit()
        {
            Debug.Log("FirstEnterInGame Exit");
        }
    }

    public class PreparePlayerState : IState
    {
        public void Enter()
        {
            Debug.Log("PreparePlayerState Enter");
        }

        public void Exit()
        {
            Debug.Log("PreparePlayerState Exit");
        }
    }

    public class GameStateMachine
    {
        private IState currentState;
        
        public void SetState(IState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }
    }
}