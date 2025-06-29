using System;

namespace Game.Scripts.Infrastructure.TimeManagement
{
    public interface ITimerModel
    {
        public string TaskId { get; }
        public string Source { get; }
        public DateTime StartDateTime { get; }
        public DateTime EndDateTime { get; }
        public TimeSpan TimeLeft { get; set; }
    }
}