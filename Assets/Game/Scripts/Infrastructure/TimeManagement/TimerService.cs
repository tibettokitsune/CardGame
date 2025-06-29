using System;
using System.Collections.Generic;
using Zenject;

namespace Game.Scripts.Infrastructure.TimeManagement
{
    public class TimerService : ITimerService, ITickable
    {
        private readonly TimersProvider _model = new();

        public void Tick()
        {
            foreach (var timer in _model.SavedTimers)
            {
                timer.TimeLeft = timer.EndDateTime > DateTime.Now
                    ? timer.EndDateTime.Subtract(DateTime.Now)
                    : TimeSpan.Zero;
            }
        }

        public void SetupTimer(string taskId, string source, TimeSpan delay)
        {
            DeleteTimer(taskId);
            var model = new TimerModel
            {
                TaskId = taskId,
                Source = source,
                StartDateTime = DateTime.Now,
                EndDateTime = DateTime.Now.Add(delay),
                TimeLeft = delay
            };
            _model.SavedTimers.Add(model);
        }

        public void DeleteTimer(string taskId)
        {
            _model.SavedTimers.RemoveAll(i => i.TaskId == taskId);
        }

        public bool HasTimer(string taskId)
        {
            for (int i = 0; i < _model.SavedTimers.Count; i++)
            {
                if (_model.SavedTimers[i].TaskId == taskId)
                    return true;
            }

            return false;
        }

        public IReadOnlyList<ITimerModel> GetTimers()
        {
            return _model.SavedTimers;
        }

        public ITimerModel GetTimer(string taskId)
        {
            for (int i = 0; i < _model.SavedTimers.Count; i++)
            {
                if (_model.SavedTimers[i].TaskId == taskId)
                    return _model.SavedTimers[i];
            }

            throw new Exception($"Timer is not exist! Timer id: {taskId}");
        }
    }
}