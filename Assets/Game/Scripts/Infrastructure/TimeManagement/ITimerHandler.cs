using System;
using System.Collections.Generic;
using Zenject;

namespace Game.Scripts.Infrastructure.TimeManagement
{
    public interface ITimerHandler
    {
        public IEnumerable<string> Sources { get; }
        public void Handle(ITimerModel model);
    }
    
    public interface ITimerUpdateService
    {
        public void Start();
        public void RegisterHandler(ITimerHandler handler);
        public void UnRegisterHandler(ITimerHandler handler);
    }
}