using Game.Scripts.Infrastructure.UI;
using Game.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Infrastructure.Loading
{
    [UIScreen("LoadingScreen")]
    public class LoadingScreen : UIScreen
    {
        [SerializeField] private int _references;
        private float _progress;
        [SerializeField] private Image fill;
        
        public float Progress
        {
            get => _progress;
            set
            {
                if (value.Equals(_progress)) 
                    return;
                _progress = Mathf.Clamp01(value);
            }
        }
        public ILoading Show() => new LoadingWrapper(this);

        public void IncRef()
        {
            if (_references == 0)
                _progress = 0;
            _references++;
            UpdateVisibility();
        }

        public void DecRef()
        {
            _references--;
            UpdateVisibility();
        }
        
        private void UpdateVisibility()
        {
            if (_references > 0) 
                ShowAsync();
            else 
                HideAsync();
        }
    }
    
    
    
}