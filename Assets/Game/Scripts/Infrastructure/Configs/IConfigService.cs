using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.Infrastructure.Configs
{
    public interface IConfigService
    {
        T Get<T>(string id) where T : BaseConfig;
    }
}