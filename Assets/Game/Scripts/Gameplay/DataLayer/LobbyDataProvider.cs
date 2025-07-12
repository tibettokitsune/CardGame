using Game.Scripts.Infrastructure.TimeManagement;
using UniRx;

namespace Game.Scripts.Gameplay.DataLayer
{
    public enum LobbyState
    {
        PrepareToRound,
        TakeEventCard,
        Battle
    }
    public interface ILobbyDataProvider
    {
        ReactiveProperty<LobbyState> LobbyState { get; }
    }

    public class LobbyDataProvider : ILobbyDataProvider
    {
        public ReactiveProperty<LobbyState> LobbyState { get; } = new ReactiveProperty<LobbyState>();
        private readonly ITimerUpdateService _timerUpdateService;
        private readonly PrepareRoundStateModel _prepareRoundStateModel;
        private readonly TakeEventCardModel _takeEventCardModel;
        public LobbyDataProvider(ITimerService timerService, ITimerUpdateService timerUpdateService)
        {
            _timerUpdateService = timerUpdateService;
            _prepareRoundStateModel = new PrepareRoundStateModel(timerService, LobbyState, _timerUpdateService);
            _takeEventCardModel = new TakeEventCardModel(timerService, LobbyState, _timerUpdateService);
        }
    }
}