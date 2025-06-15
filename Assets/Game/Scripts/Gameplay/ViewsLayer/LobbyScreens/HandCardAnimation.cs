using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyScreens
{
    public class HandCardAnimation : MonoBehaviour
    {
        [SerializeField] private PlayableDirector enterAnimation;

        private CancellationTokenSource _cancellationTokenSource;

        public async UniTask PlayEnterAnimation(CancellationToken externalToken = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(externalToken);

            try
            {
                enterAnimation.Play();

                await UniTask.WaitUntil(() =>
                        enterAnimation.state != PlayState.Playing,
                    cancellationToken: _cancellationTokenSource.Token
                );
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("Timeline playback was cancelled.");
                enterAnimation.Stop();
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }
    }
}