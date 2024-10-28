using System.Collections.Generic;

namespace Game.Scripts.Infrastructure
{
    public interface IDataStorage<T> : IEnumerable<T>
    {
        TC Get<TC>(string id) where TC : T;
        IReadOnlyList<TC> Get<TC>() where TC : T;
    }
}