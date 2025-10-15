using System;
using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.Gameplay.PresentersLayer.Player
{
    public class StatEntity
    {
        public PlayerStat Stat { get; set; }
        public string Name => Stat.ToString();
        public string Format => "F0";

        public string Icon => $"Icons/{Stat.ToString()}";
        public float Value;
    }
}
