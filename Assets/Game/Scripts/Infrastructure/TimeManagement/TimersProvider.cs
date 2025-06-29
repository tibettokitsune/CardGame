using System.Collections.Generic;

namespace Game.Scripts.Infrastructure.TimeManagement
{
    internal class TimersProvider
    {
        public List<ITimerModel> SavedTimers { get; set; } = new();
    }
}