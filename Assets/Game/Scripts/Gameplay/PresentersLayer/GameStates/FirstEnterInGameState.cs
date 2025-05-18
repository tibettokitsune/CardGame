using System.Threading.Tasks;
using Game.Scripts.Gameplay.Lobby.Player;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.UI;
using UnityEngine;
using UnityHFSM;

namespace Game.Scripts.Gameplay.Lobby.GameStates
{
    public class FirstEnterInGameState : StateBase
    {
        private readonly IFillStartHandUseCase _fillStartHandUseCase;
        private readonly IUIService<UIScreen> _uiService;
        public bool IsReadyToSwitch { get; private set; }
        public FirstEnterInGameState(IFillStartHandUseCase fillStartHandUseCase, IUIService<UIScreen> uiService) 
            : base(needsExitTime: false, isGhostState: false)
        {
            _fillStartHandUseCase = fillStartHandUseCase;
            _uiService = uiService;
        }
        
        public override void Init() { }

        public override async void OnEnter()
        {
            Debug.Log("FirstEnterInGameState Enter");
            await _uiService.ShowScreen<UIScreen>("CharacterActiveCards");
            await _uiService.ShowScreen<UIScreen>("CharacterStats");
            await _uiService.ShowScreen<UIScreen>("PlayerHand");
            await FillPlayerStartHand();
            IsReadyToSwitch = true;
        }
        public override void OnLogic() { }

        public override void OnExit()
        {
            IsReadyToSwitch = false;
            Debug.Log("FirstEnterInGame Exit");
        }

        public override void OnExitRequest() { }

        private async Task FillPlayerStartHand()
        {
            await _fillStartHandUseCase.Execute();
        }
    }
}