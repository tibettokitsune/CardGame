using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Game.Scripts.Infrastructure.TimeManagement;

namespace Game.Scripts.Gameplay.PresentersLayer.Battle
{
    public interface IBattlePreparationCoordinator : ITimerHandler
    {
        string TimerId { get; }
        TimeSpan PreparationDuration { get; }
        void Begin();
        Task WaitForCompletionAsync(CancellationToken token);
        void MarkReady();
        void Cancel();
    }

    public class BattlePreparationCoordinator : IBattlePreparationCoordinator
    {
        public const string DefaultTimerId = "BattlePreparation";

        public string TimerId => DefaultTimerId;
        public TimeSpan PreparationDuration { get; }
        public IEnumerable<string> Sources => new[] { TimerId };

        private readonly ITimerService _timerService;
        private readonly ITimerUpdateService _timerUpdateService;
        private TaskCompletionSource<bool> _stageCompletionSource;
        private bool _handlerRegistered;

        public BattlePreparationCoordinator(
            ITimerService timerService,
            ITimerUpdateService timerUpdateService,
            TimeSpan? duration = null)
        {
            _timerService = timerService;
            _timerUpdateService = timerUpdateService;
            PreparationDuration = duration ?? TimeSpan.FromSeconds(5);
        }

        public void Begin()
        {
            Cancel();
            _stageCompletionSource = new TaskCompletionSource<bool>();
            _timerService.SetupTimer(TimerId, TimerId, PreparationDuration);
            Register();
        }

        public async Task WaitForCompletionAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_stageCompletionSource == null)
                _stageCompletionSource = new TaskCompletionSource<bool>();

            using (token.Register(CancelCompletion))
            {
                await _stageCompletionSource.Task;
            }
        }

        public void MarkReady()
        {
            CompleteStage();
        }

        public void Cancel()
        {
            if (_stageCompletionSource != null && !_stageCompletionSource.Task.IsCompleted)
                _stageCompletionSource.TrySetCanceled();

            _stageCompletionSource = null;
            Cleanup();
        }

        public void Handle(ITimerModel model)
        {
            if (model?.TaskId != TimerId)
                return;

            CompleteStage();
        }

        private void CompleteStage()
        {
            Cleanup();
            _stageCompletionSource?.TrySetResult(true);
        }

        private void Cleanup()
        {
            _timerService.DeleteTimer(TimerId);
            Unregister();
        }

        private void Register()
        {
            if (_handlerRegistered)
                return;

            _timerUpdateService.RegisterHandler(this);
            _handlerRegistered = true;
        }

        private void Unregister()
        {
            if (!_handlerRegistered)
                return;

            _timerUpdateService.UnRegisterHandler(this);
            _handlerRegistered = false;
        }

        private void CancelCompletion()
        {
            Cleanup();
            _stageCompletionSource?.TrySetCanceled();
        }
    }
}
