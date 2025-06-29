using System;
using System.Collections.Generic;
using Zenject;

namespace Game.Scripts.Infrastructure.TimeManagement
{
    public interface ITimerHandler
    {
        public IEnumerable<string> Sources { get; }
        public void Handle(ITimerModel model);
    }
    
    public interface ITimerUpdateService
    {
        public void RegisterHandler(ITimerHandler handler);
        public void UnRegisterHandler(ITimerHandler handler);
    }
    
    internal sealed class TimerHandlerService : ITickable , ITimerUpdateService
    {
        private readonly ITimerService _timerService;
        
        private readonly List<ITimerModel> _completedTimers = new();
        private readonly Dictionary<string, List<ITimerHandler>> _timersHandlers = new();

        private TimerHandlerService(
            ITimerService timerService)
        {
            _timerService = timerService;
        }

        public void Tick()
        {
            UpdateTimers();
        }

        private void UpdateTimers()
        {
            HandleTimers();
            HandleCompletedTimers();
        }

        public void RegisterHandler(ITimerHandler handler)
        {
            foreach (var source in handler.Sources)
            {
                if (!_timersHandlers.TryGetValue(source, out var list))
                {
                    _timersHandlers.Add(source, list = new List<ITimerHandler>());
                }

                list.Add(handler);
            }

            UpdateTimers();
        }

        public void UnRegisterHandler(ITimerHandler handler)
        {
            foreach (var source in handler.Sources)
            {
                if (!_timersHandlers.TryGetValue(source, out var list))
                {
                    throw new Exception("Source has not initialized");
                }

                if (!list.Contains(handler))
                {
                    throw new Exception("Handler has not found");
                }

                list.Remove(handler);
            }            
        }
        
        private void HandleTimers()
        {
            var timers = _timerService.GetTimers();

            for (var i = 0; i < timers.Count; i++)
            {
                HandleTimer(timers[i]);
            }
        }

        private void HandleTimer(ITimerModel timer)
        {
            var currentTime = DateTime.Now;
            var endTime = timer.EndDateTime;

            timer.TimeLeft = endTime - currentTime;

            if (currentTime < endTime) return;

            _completedTimers.Add(timer);
        }

        private void HandleCompletedTimers()
        {
            for (var i = 0; i < _completedTimers.Count; i++)
            {
                HandleCompletedTimer(_completedTimers[i]);
            }

            _completedTimers.Clear();
        }

        private void HandleCompletedTimer(ITimerModel model)
        {
            if (!_timersHandlers.TryGetValue(model.Source, out var handlers)) return;

            for (var i = 0; i < handlers.Count; i++)
            {
                handlers[i].Handle(model);
            }
        }
    }
}