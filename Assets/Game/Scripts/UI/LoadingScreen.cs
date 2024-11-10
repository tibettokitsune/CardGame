using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class LoadingScreen : UIScreen
    {
        [SerializeField] private Image fill;
        public async UniTask Loading(List<AsyncOperation> loadingOperation)
        {
            Open();
            await UniTask.WaitUntil(() =>
            {
                return loadingOperation.Count(x => x.isDone) < loadingOperation.Count;
            });
            Close();
        }
    }
    
    
    
}