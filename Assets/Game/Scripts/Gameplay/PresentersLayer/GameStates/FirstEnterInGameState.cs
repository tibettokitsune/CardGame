using System.Threading.Tasks;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.UI;
using UnityEngine;
using UnityHFSM;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public class FirstEnterInGameState : StateBase
    {
        private readonly IFillStartHandUseCase _fillStartHandUseCase;
        private readonly IUIService _uiService;
        public FirstEnterInGameState(IFillStartHandUseCase fillStartHandUseCase, IUIService uiService) 
            : base(needsExitTime: false, isGhostState: false)
        {
            _fillStartHandUseCase = fillStartHandUseCase;
            _uiService = uiService;
        }
        
        public override void Init() { }

        public override async void OnEnter()
        {
            Debug.Log("FirstEnterInGameState Enter");
            await _uiService.ShowScreen("CharacterActiveCards");
            await _uiService.ShowScreen("CharacterStats");
            await _uiService.ShowScreen("PlayerHand");
            await FillPlayerStartHand();
        }
        public override void OnLogic() { }

        public override void OnExit()
        {
            Debug.Log("FirstEnterInGame Exit");
        }

        public override void OnExitRequest() { }

        private async Task FillPlayerStartHand()
        {
            await _fillStartHandUseCase.Execute();
        }
    }
}