using Game.Scripts.Gameplay.DataLayer;
using Game.Scripts.Gameplay.PresentersLayer.GameStates;
using UnityEngine;
using UnityHFSM;
using Zenject;

namespace Game.Scripts.Gameplay.PresentersLayer
{
    public class LobbyPresenter : ILobbyPresenter, IInitializable, ITickable
    {
        private readonly ILobbyDataProvider _lobbyDataProvider;
        private readonly PreparePlayerState _preparePlayerState;
        private readonly FirstEnterInGameState _firstEnterInGameState;
        
        private StateMachine _gameStateMachine;

        public LobbyPresenter(ILobbyDataProvider lobbyDataProvider,
            PreparePlayerState preparePlayerState, 
            FirstEnterInGameState firstEnterInGameState
            )
        {
            _lobbyDataProvider = lobbyDataProvider;
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
                condition => _lobbyDataProvider.LobbyState.Value == LobbyState.PrepareToRound));
            
            _gameStateMachine.SetStartState("FirstEnter");
            
            _gameStateMachine.Init();
        }

        public void Tick()
        {
            _gameStateMachine?.OnLogic();
        }
    }
}