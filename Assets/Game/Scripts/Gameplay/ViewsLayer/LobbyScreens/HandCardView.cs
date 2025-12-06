using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Scripts.UIContracts;
using Game.Scripts.Infrastructure.AsyncAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyScreens
{
    public class HandCardView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private DragAndDropWidget dragAndDropWidget;
        [SerializeField] private TextMeshProUGUI descriptionLabel;
        [SerializeField] private HandCardAnimation enterAnimation;
        [SerializeField] private Image mainIcon;
        [SerializeField] private Image backgroundIcon;
        [SerializeField] private LayoutElement layoutElement;

        [Header("Idle Animation")]
        [SerializeField] private float idleScaleMultiplier = 1.05f;
        [SerializeField] private float idleDuration = 0.6f;
        [SerializeField] private Ease idleEase = Ease.InOutSine;

        private CancellationTokenSource _loadCancellation;
        private Tween _idleTween;
        private CardViewData _cardEntity;
        private string _cardId;
        private bool _dragCallbacksRegistered;
        private Vector3 _baseScale = Vector3.one;
        private bool _baseScaleInitialized;

        private RectTransform AnimatedTransform =>
            enterAnimation != null ? enterAnimation.AnimatedTransform : transform as RectTransform;

        private void Awake()
        {
            if (dragAndDropWidget == null)
                dragAndDropWidget = GetComponent<DragAndDropWidget>();
            CacheBaseScale();
        }

        private void OnEnable()
        {
            RegisterDragCallbacks();
            StopIdleAnimation(resetScale: true);
            enterAnimation?.PlayEnterAnimation().Forget();
        }

        private void OnDisable()
        {
            UnregisterDragCallbacks();
            StopIdleAnimation(resetScale: true);
            CancelSpriteLoading();
            CacheBaseScale(force: false);
        }

        private void OnDestroy()
        {
            UnregisterDragCallbacks();
            StopIdleAnimation(resetScale: false);
            CancelSpriteLoading();
        }

        public async void Setup(CardViewData cardEntity, ISpriteService spriteService)
        {
            if (cardEntity == null) throw new ArgumentNullException(nameof(cardEntity));
            if (spriteService == null) throw new ArgumentNullException(nameof(spriteService));

            CancelSpriteLoading();

            _cardEntity = cardEntity;
            _cardId = cardEntity.Id;

            dragAndDropWidget?.Setup(_cardId);
            StopIdleAnimation(resetScale: true);

            ApplyCardInfo(cardEntity);
            ResetIcons();
            CacheBaseScale(force: true);

            _loadCancellation = new CancellationTokenSource();
            var token = _loadCancellation.Token;

            try
            {
                await LoadSpritesAsync(spriteService, cardEntity, token);
            }
            catch (OperationCanceledException)
            {
                // Intentionally ignored
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to load sprites for card '{_cardId}': {exception.Message}", this);
            }
        }

        public void EnableGrouping()
        {
            if (layoutElement != null)
                layoutElement.enabled = true;
        }

        protected virtual string BuildDescription(CardViewData card)
        {
            return $"{card.Name}\n{card.Description}";
        }

        protected virtual void OnCardAssigned(CardViewData card)
        {
        }

        protected virtual void OnSpritesApplied(Sprite mainSprite, Sprite backgroundSprite)
        {
        }

        private void ApplyCardInfo(CardViewData card)
        {
            if (descriptionLabel != null)
                descriptionLabel.text = BuildDescription(card);

            OnCardAssigned(card);
        }

        private void ResetIcons()
        {
            if (mainIcon != null)
                mainIcon.sprite = null;

            if (backgroundIcon != null)
                backgroundIcon.sprite = null;
        }

        private async UniTask LoadSpritesAsync(ISpriteService spriteService, CardViewData card, CancellationToken token)
        {
            if (mainIcon == null || backgroundIcon == null)
                return;

            var mainSprite =
                await spriteService.LoadSpriteForObject(card.MainLayer, mainIcon.gameObject, token);
            var backgroundSprite =
                await spriteService.LoadSpriteForObject(card.BackgroundLayer, backgroundIcon.gameObject, token);

            ApplySprites(mainSprite, backgroundSprite);
        }

        private void ApplySprites(Sprite mainSprite, Sprite backgroundSprite)
        {
            if (mainIcon != null)
                mainIcon.sprite = mainSprite;

            if (backgroundIcon != null)
                backgroundIcon.sprite = backgroundSprite;

            OnSpritesApplied(mainSprite, backgroundSprite);
        }

        private void RegisterDragCallbacks()
        {
            if (dragAndDropWidget == null || _dragCallbacksRegistered)
                return;

            dragAndDropWidget.DragStarted += OnDragStarted;
            dragAndDropWidget.DragEnded += OnDragEnded;
            _dragCallbacksRegistered = true;
        }

        private void UnregisterDragCallbacks()
        {
            if (dragAndDropWidget == null || !_dragCallbacksRegistered)
                return;

            dragAndDropWidget.DragStarted -= OnDragStarted;
            dragAndDropWidget.DragEnded -= OnDragEnded;
            _dragCallbacksRegistered = false;
        }

        private void OnDragStarted(DragAndDropWidget widget)
        {
            if (widget != dragAndDropWidget)
                return;

            enterAnimation?.StopAnimation();
            ResetScaleToBase();
            StartIdleAnimation();
        }

        private void OnDragEnded(DragAndDropWidget widget)
        {
            if (widget != dragAndDropWidget)
                return;

            StopIdleAnimation(resetScale: true);
        }

        private void StartIdleAnimation()
        {
            var target = AnimatedTransform;
            if (target == null || idleScaleMultiplier <= 0f)
                return;

            StopIdleAnimation(resetScale: false);

            var baseScale = GetBaseScale();
            _idleTween = target.DOScale(baseScale * idleScaleMultiplier, idleDuration)
                .SetEase(idleEase)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(UpdateType.Normal, true)
                .SetTarget(this);
        }

        private void StopIdleAnimation(bool resetScale)
        {
            if (_idleTween != null)
            {
                _idleTween.Kill();
                _idleTween = null;
            }

            if (resetScale)
                ResetScaleToBase();
        }

        private void ResetScaleToBase()
        {
            var target = AnimatedTransform;
            if (target == null)
                return;

            target.localScale = GetBaseScale();
        }

        private void CancelSpriteLoading()
        {
            if (_loadCancellation == null)
                return;

            _loadCancellation.Cancel();
            _loadCancellation.Dispose();
            _loadCancellation = null;
        }

        private void CacheBaseScale(bool force = false)
        {
            if (!force && _baseScaleInitialized)
                return;

            if (enterAnimation != null)
            {
                _baseScale = enterAnimation.TargetScale;
                _baseScaleInitialized = true;
                return;
            }

            var target = AnimatedTransform;
            if (target != null)
            {
                _baseScale = target.localScale;
                _baseScaleInitialized = true;
            }
        }

        private Vector3 GetBaseScale()
        {
            if (enterAnimation != null)
                return enterAnimation.TargetScale;

            if (!_baseScaleInitialized)
                CacheBaseScale(force: true);

            return _baseScale;
        }
    }
}
