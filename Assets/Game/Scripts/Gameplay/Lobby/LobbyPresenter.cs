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

        public FirstEnterInGameState(IPlayerPresenter playerPresenter)
        {
            _playerPresenter = playerPresenter;
        }

        public void Enter()
        {
            Debug.Log("FirstEnterInGameState Enter");
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