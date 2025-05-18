using System.Threading.Tasks;
using UnityEngine;

namespace Game.Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.3f;

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual async Task ShowAsync()
        {
            gameObject.SetActive(true);
            
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                await LerpAlpha(0, 1, _fadeDuration);
            }
        }

        public virtual async Task HideAsync()
        {
            if (_canvasGroup != null)
            {
                await LerpAlpha(1, 0, _fadeDuration);
            }
            
            gameObject.SetActive(false);
        }

        private async Task LerpAlpha(float from, float to, float duration)
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                elapsed += Time.deltaTime;
                await Task.Yield();
            }
            _canvasGroup.alpha = to;
        }
    }
}