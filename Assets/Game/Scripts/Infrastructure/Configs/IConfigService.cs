namespace Game.Scripts.Infrastructure.Configs
{
    public interface IConfigService<T>
    {
        TC Get<TC>(string id) where TC : T;
    }
}