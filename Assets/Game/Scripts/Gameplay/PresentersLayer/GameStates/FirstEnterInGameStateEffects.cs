using System.Threading.Tasks;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.UIContracts;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
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
