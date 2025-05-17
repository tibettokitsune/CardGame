using Game.Scripts.Gameplay.Lobby.GameStates;
using UnityEngine;
using UnityHFSM;
using Zenject;

namespace Game.Scripts.Gameplay.Lobby
{
    public interface ILobbyPresenter
    {
    }
    
    public class LobbyPresenter : ILobbyPresenter, IInitializable, ITickable
    {
        private readonly PreparePlayerState _preparePlayerState;
        private readonly FirstEnterInGameState _firstEnterInGameState;
        
        private StateMachine _gameStateMachine;

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
            _gameStateMachine = new StateMachine();
            
            _gameStateMachine.AddState("FirstEnter", _firstEnterInGameState);
            _gameStateMachine.AddState("PrepareRound", _preparePlayerState);
            
            _gameStateMachine.AddTransition(new Transition("FirstEnter", "PrepareRound", 
                condition => _firstEnterInGameState.IsReadyToSwitch));
            
            _gameStateMachine.SetStartState("FirstEnter");
            
            _gameStateMachine.Init();
        }

        public void Tick()
        {
            _gameStateMachine?.OnLogic();
        }
    }
}