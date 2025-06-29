using System;
using Newtonsoft.Json;

namespace Game.Scripts.Infrastructure.TimeManagement
{
    [JsonObject]
    public class TimerModel : ITimerModel
    {
        public string TaskId { get; set; }
        public string Source { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        [JsonIgnore] public TimeSpan TimeLeft { get; set; }
    }
}