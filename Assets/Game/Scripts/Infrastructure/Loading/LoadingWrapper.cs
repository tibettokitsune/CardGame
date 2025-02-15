using Game.Scripts.Infrastructure.Loading;
using Game.Scripts.UI;

namespace Game.Scripts.Infrastructure
{
    public class LoadingWrapper : ILoading
    {
        private readonly LoadingScreen _screen;

        public float Progress
        {
            get => _screen.Progress;
            set => _screen.Progress = value;
        }

        public LoadingWrapper(LoadingScreen screen)
        {
            _screen = screen;
            _screen.IncRef();
        }

        public void Dispose() => _screen.DecRef();
    }
}