using System;
using System.Collections.Generic;
using Game.Scripts.Infrastructure.TimeManagement;
using UniRx;

namespace Game.Scripts.Gameplay.DataLayer
{
    internal class PrepareRoundStateModel : ITimerHandler, IDisposable{
        public IEnumerable<string> Sources => new []{"PrepareToRound"};
        private readonly ITimerService _timerService;
        private readonly ReactiveProperty<LobbyState> _lobbyState;
        private readonly ITimerUpdateService _timerUpdateService;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();
        public PrepareRoundStateModel(ITimerService timerService, ReactiveProperty<LobbyState> lobbyState,
            ITimerUpdateService timerUpdateService)
        {
            _timerService = timerService;
            _lobbyState = lobbyState;
            _timerUpdateService = timerUpdateService;
            _lobbyState
                .Where(x => x == LobbyState.PrepareToRound)
                .Subscribe(_=> Setup())
                .AddTo(_disposable);
            
            _timerUpdateService.RegisterHandler(this);
        }

        private void Setup()
        {
            foreach (var source in Sources)
            {
                _timerService.SetupTimer(source, source, TimeSpan.FromMinutes(1));
            }
        }
        public void Handle(ITimerModel model)
        {
            _lobbyState.Value = LobbyState.TakeEventCard;
        }

        public void Dispose()
        {
            _timerUpdateService.UnRegisterHandler(this);
            _disposable?.Dispose();
        }
    }
    
    internal class TakeEventCardModel : ITimerHandler, IDisposable{
        public IEnumerable<string> Sources => new []{"TakeEvent"};
        private readonly ITimerService _timerService;
        private readonly ReactiveProperty<LobbyState> _lobbyState;
        private readonly ITimerUpdateService _timerUpdateService;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();
        public TakeEventCardModel(ITimerService timerService, ReactiveProperty<LobbyState> lobbyState,
            ITimerUpdateService timerUpdateService)
        {
            _timerService = timerService;
            _lobbyState = lobbyState;
            _timerUpdateService = timerUpdateService;
            _lobbyState
                .Where(x => x == LobbyState.TakeEventCard)
                .Subscribe(_=> Setup())
                .AddTo(_disposable);
            _timerUpdateService.RegisterHandler(this);
        }

        private void Setup()
        {
            foreach (var source in Sources)
            {
                _timerService.SetupTimer(source, source, TimeSpan.FromSeconds(15));
            }
        }
        public void Handle(ITimerModel model)
        {
            _lobbyState.Value = LobbyState.Battle;
        }

        public void Dispose()
        {
            _timerUpdateService.UnRegisterHandler(this);
            _disposable?.Dispose();
        }
    }
}