using System;

namespace Game.Scripts.Infrastructure
{
    public interface ILoading: IDisposable
    {
        float Progress { get; set; }
    }
}