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
        public bool HideOtherScreensOnLayer { get; set; }
        public string ScreenType { get; set; }
    }
}