using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class LoadingScreen : UIScreen
    {
        [SerializeField] private Image fill;
        public async UniTask Loading(AsyncOperation loadingOperation)
        {
            Open();
            await UniTask.WaitUntil(() =>
            {
                fill.fillAmount = loadingOperation.progress;
                return loadingOperation.isDone;
            });
            Close();
        }
    }
    
    
    
}