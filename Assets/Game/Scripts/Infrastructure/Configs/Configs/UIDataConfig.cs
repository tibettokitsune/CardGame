using System;

namespace Game.Scripts.Infrastructure.Configs.Configs
{
    public enum UILayer
    {
        Window,
        Popup
    }

    [Serializable]
    public class UIDataConfig : BaseConfig
    {
        public string PrefabPath { get; set; }
        public UILayer Layer { get; set; }
    }
}