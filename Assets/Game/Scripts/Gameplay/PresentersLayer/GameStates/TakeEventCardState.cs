using Game.Scripts.Gameplay.DataLayer;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.Infrastructure.SceneManagment;
using Game.Scripts.UI;
using UnityEngine;
using UnityHFSM;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public interface IFinishTakeEventCardStateUseCase
    {
        void Execute();
    }
    public class TakeEventCardState : StateBase, IFinishTakeEventCardStateUseCase
    {
        private readonly ISceneManagerService _sceneManagerService;
        private readonly ITakeEventCardUseCase _takeEventCardUseCase;
        private readonly ILobbyDataProvider _lobbyDataProvider;

        public TakeEventCardState(ISceneManagerService sceneManagerService, 
            ITakeEventCardUseCase takeEventCardUseCase,
            ILobbyDataProvider lobbyDataProvider) 
            : base(needsExitTime: false, isGhostState: false)
        {
            _sceneManagerService = sceneManagerService;
            _takeEventCardUseCase = takeEventCardUseCase;
            _lobbyDataProvider = lobbyDataProvider;
        }

        public override async void OnEnter()
        {
            Debug.Log("TakeEventCardState Enter");
            await _takeEventCardUseCase.Execute();
            await _sceneManagerService.LoadScene("GameplayTakeEvent", SceneLayer.GameplayElement, false);
            await _sceneManagerService.LoadScene("Cliffs_red_cave", SceneLayer.GameplayElement, true);
        }

        public override void OnExit()
        {
            Debug.Log("TakeEventCardState Exit");
            _sceneManagerService.UnloadScene("GameplayTakeEvent", SceneLayer.GameplayElement);
            _sceneManagerService.UnloadScene("Cliffs_red_cave", SceneLayer.GameplayElement);
        }

        void IFinishTakeEventCardStateUseCase.Execute()
        {
            _lobbyDataProvider.LobbyState.Value = LobbyState.Battle;
        }
    }
}