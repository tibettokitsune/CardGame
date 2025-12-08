using Game.Scripts.Gameplay.DataLayer;
using Game.Scripts.Gameplay.PresentersLayer.GameStates;
using UnityEngine;
using UnityHFSM;
using Zenject;

namespace Game.Scripts.Gameplay.PresentersLayer
{
    public class GameplayFlowPresenter : IGameplayFlowPresenter, IInitializable, ITickable
    {
        private readonly ILobbyDataProvider _lobbyDataProvider;
        private readonly BattleState _battleState;
        private readonly TakeEventCardState _takeEventCardState;
        private readonly PreparePlayerState _preparePlayerState;
        private readonly FirstEnterInGameState _firstEnterInGameState;
        
        private StateMachine _gameStateMachine;

        public GameplayFlowPresenter(
            ILobbyDataProvider lobbyDataProvider,
            BattleState battleState, 
            TakeEventCardState takeEventCardState, 
            PreparePlayerState preparePlayerState, 
            FirstEnterInGameState firstEnterInGameState
            )
        {
            _lobbyDataProvider = lobbyDataProvider;
            _battleState = battleState;
            _preparePlayerState = preparePlayerState;
            _takeEventCardState = takeEventCardState;
            _firstEnterInGameState = firstEnterInGameState;
        }

        public void Initialize()
        {
            Debug.Log("GameplayFlowPresenter Initialize");
            _gameStateMachine = new StateMachine();
            
            _gameStateMachine.AddState("FirstEnter", _firstEnterInGameState);
            _gameStateMachine.AddState("PrepareRound", _preparePlayerState);
            _gameStateMachine.AddState("TakeEventCard", _takeEventCardState);
            _gameStateMachine.AddState("Battle", _battleState);

            _gameStateMachine.AddTransition(new Transition("FirstEnter", "PrepareRound", 
                condition => _lobbyDataProvider.LobbyState.Value == LobbyState.PrepareToRound));
            _gameStateMachine.AddTransition(new Transition("PrepareRound", "TakeEventCard", 
                condition => _lobbyDataProvider.LobbyState.Value == LobbyState.TakeEventCard));
            _gameStateMachine.AddTransition(new Transition("TakeEventCard", "PrepareRound", 
                condition => _lobbyDataProvider.LobbyState.Value == LobbyState.PrepareToRound));
            _gameStateMachine.AddTransition(new Transition("TakeEventCard", "Battle", 
                condition => _lobbyDataProvider.LobbyState.Value == LobbyState.Battle));
            _gameStateMachine.AddTransition(new Transition("Battle", "PrepareRound",
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
