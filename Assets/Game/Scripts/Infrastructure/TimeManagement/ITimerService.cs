using System;
using System.Collections.Generic;

namespace Game.Scripts.Infrastructure.TimeManagement
{
    public interface ITimerService
    {
        void SetupTimer(string taskId, string source, TimeSpan delay);
        ITimerModel GetTimer(string taskId);
        void DeleteTimer(string taskId);
        bool HasTimer(string taskId);
        IReadOnlyList<ITimerModel> GetTimers();
    }
}