using System;

namespace Game.Scripts.Infrastructure.Configs.Configs
{
    [Serializable]
    public class BaseConfig : IBaseConfig
    {
        public string Id { get; set; }
        public string ConfigType { get; set; }
    }
}
