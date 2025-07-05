using System;
using System.Collections.Generic;
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
    
    internal class PrepareModel : ITimerHandler, IDisposable{
        public IEnumerable<string> Sources => new []{"PrepareToRound"};
        private readonly ITimerService _timerService;
        private readonly ReactiveProperty<LobbyState> _lobbyState;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();
        public PrepareModel(ITimerService timerService, ReactiveProperty<LobbyState> lobbyState)
        {
            _timerService = timerService;
            _lobbyState = lobbyState;
            _lobbyState
                .Where(x => x == LobbyState.PrepareToRound)
                .Subscribe(_=> Setup())
                .AddTo(_disposable);
        }

        private void Setup()
        {
            foreach (var source in Sources)
            {
                _timerService.SetupTimer(source, source, TimeSpan.FromMinutes(5));
            }
        }
        public void Handle(ITimerModel model)
        {
            _lobbyState.Value = LobbyState.TakeEventCard;
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }

    public class LobbyDataProvider : ILobbyDataProvider
    {
        private readonly ITimerUpdateService _timerUpdateService;
        public ReactiveProperty<LobbyState> LobbyState { get; } = new ReactiveProperty<LobbyState>();

        private readonly PrepareModel _prepareModel;
        
        public LobbyDataProvider(ITimerService timerService, ITimerUpdateService timerUpdateService)
        {
            _timerUpdateService = timerUpdateService;
            _prepareModel = new PrepareModel(timerService, LobbyState);
            
            _timerUpdateService.RegisterHandler(_prepareModel);
        }
    }
}