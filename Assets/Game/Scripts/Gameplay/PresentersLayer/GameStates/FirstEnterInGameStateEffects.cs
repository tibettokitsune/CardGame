using System.Threading.Tasks;
using Game.Scripts.UIContracts;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.UI;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    [EffectOrder(-10)]
    public class ShowInitialGameplayUiEffect : IStateEnterEffect<FirstEnterInGameState>
    {
        private readonly IUIService _uiService;

        public ShowInitialGameplayUiEffect(IUIService uiService)
        {
            _uiService = uiService;
        }

        public async Task OnEnterAsync()
        {
            await _uiService.ShowAsync<ICharacterActiveCardsScreen>();
            await _uiService.ShowAsync<ICharacterStatsScreen>();
            await _uiService.ShowAsync<IPlayerHandScreen>();
        }
    }

    [EffectOrder(0)]
    public class FillStartHandEffect : IStateEnterEffect<FirstEnterInGameState>
    {
        private readonly IFillStartHandUseCase _fillStartHandUseCase;

        public FillStartHandEffect(IFillStartHandUseCase fillStartHandUseCase)
        {
            _fillStartHandUseCase = fillStartHandUseCase;
        }

        public Task OnEnterAsync()
        {
            return _fillStartHandUseCase.Execute();
        }
    }
}
