using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyScreens
{
    public class HandCardAnimation : MonoBehaviour
    {
        [Header("Animation Targets")]
        [SerializeField] private RectTransform animatedTransform;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private Vector3 startScale = new Vector3(0.85f, 0.85f, 1f);
        [SerializeField] private Vector3 endScale = Vector3.one;
        [SerializeField] private float startAlpha = 0f;
        [SerializeField] private float endAlpha = 1f;
        [SerializeField] private float duration = 0.35f;
        [SerializeField] private Ease ease = Ease.OutBack;

        private CancellationTokenSource _cancellationTokenSource;
        private Sequence _activeSequence;

        public RectTransform AnimatedTransform => animatedTransform ? animatedTransform : transform as RectTransform;
        public CanvasGroup CanvasGroup => canvasGroup;
        public Vector3 TargetScale => endScale;

        public async UniTask PlayEnterAnimation(CancellationToken externalToken = default)
        {
            CancelCurrentAnimation();
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            var token = _cancellationTokenSource.Token;

            var target = AnimatedTransform;
            if (target == null)
                throw new InvalidOperationException("HandCardAnimation requires a RectTransform target.");

            PrepareInitialState(target);

            try
            {
                _activeSequence = CreateSequence(target);
                await AwaitSequence(_activeSequence, token);
            }
            catch (OperationCanceledException)
            {
                _activeSequence?.Kill();
            }
            finally
            {
                CleanupAfterAnimation();
            }
        }

        private void PrepareInitialState(RectTransform target)
        {
            target.localScale = startScale;
            if (canvasGroup != null)
                canvasGroup.alpha = startAlpha;
        }

        private Sequence CreateSequence(RectTransform target)
        {
            var sequence = DOTween.Sequence();
            sequence.SetTarget(this);
            sequence.Append(target.DOScale(endScale, duration).SetEase(ease));

            if (canvasGroup != null)
            {
                canvasGroup.alpha = startAlpha;
                sequence.Join(DOTween.To(() => canvasGroup.alpha, value => canvasGroup.alpha = value, endAlpha, duration));
            }

            sequence.OnKill(() => _activeSequence = null);
            return sequence;
        }

        private void CancelCurrentAnimation()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            if (_activeSequence != null)
            {
                _activeSequence.Kill();
                _activeSequence = null;
            }
        }

        private void CleanupAfterAnimation()
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _activeSequence = null;
        }

        public void StopAnimation()
        {
            CancelCurrentAnimation();
            CleanupAfterAnimation();
        }

        private async UniTask AwaitSequence(Sequence sequence, CancellationToken token)
        {
            if (sequence == null)
                return;

            await UniTask.WaitUntil(
                () => sequence == null || !sequence.IsActive() || sequence.IsComplete(),
                cancellationToken: token);
        }

        private void OnDisable()
        {
            CancelCurrentAnimation();
        }

        private void OnDestroy()
        {
            CancelCurrentAnimation();
        }
    }
}
