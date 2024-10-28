using System;

namespace Game.Scripts.Infrastructure
{
    [Serializable]
    public abstract class BaseConfigData : IConfigEntity
    {
        public string Id { get; set; }
    }
}