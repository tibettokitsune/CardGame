using System;
using Game.Scripts.Gameplay.Lobby.Player;

namespace Game.Scripts.Gameplay.PresentersLayer.Player
{
    public class StatEntity
    {
        public PlayerStat Stat { get; set; }
        public string Name => Stat.ToString();
        public string Format { get; set; }

        public string Icon;
        public float Value;
    }
}