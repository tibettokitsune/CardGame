using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyScreens
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class DragAndDropWidget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public event Action OnDrop;

        [Header("Drag Settings")] [SerializeField]
        private float dragAlpha = 0.6f;

        [SerializeField] private bool returnIfNotDropped = true;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Canvas parentCanvas;
        private Transform originalParent;
        private LayoutElement layoutElement;
        private int originalSiblingIndex;

        // Для работы с Layout Group
        private LayoutGroup originalLayoutGroup;
        private bool wasInLayoutGroup;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            parentCanvas = GetComponentInParent<Canvas>();
            layoutElement = GetComponent<LayoutElement>();

            // Запоминаем оригинальный parent и проверяем Layout Group
            originalParent = transform.parent;
            originalLayoutGroup = originalParent.GetComponent<LayoutGroup>();
            wasInLayoutGroup = originalLayoutGroup != null;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Запоминаем оригинальный порядок в hierarchy
            originalSiblingIndex = transform.GetSiblingIndex();

            // Делаем элемент полупрозрачным при перетаскивании
            canvasGroup.alpha = dragAlpha;

            // Отключаем блокировку raycast, чтобы другие элементы могли реагировать на курсор
            canvasGroup.blocksRaycasts = false;

            // Если элемент был в Layout Group - временно вынимаем его
            if (wasInLayoutGroup)
            {
                // Перемещаем элемент на верхний уровень Canvas для свободного перемещения
                transform.SetParent(parentCanvas.transform, true);

                // Отключаем Layout Element, если он есть
                if (layoutElement != null)
                {
                    layoutElement.ignoreLayout = true;
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Перемещаем элемент вместе с курсором
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.transform as RectTransform,
                    eventData.position,
                    parentCanvas.worldCamera,
                    out Vector2 localPoint))
            {
                rectTransform.localPosition = localPoint;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Возвращаем прозрачность
            canvasGroup.alpha = 1f;

            // Включаем блокировку raycast обратно
            canvasGroup.blocksRaycasts = true;

            // Проверяем, был ли элемент сброшен в зону
            bool wasDropped = false;
            foreach (var hovered in eventData.hovered)
            {
                if (hovered.TryGetComponent<DropZone>(out var dropZone))
                {
                    wasDropped = true;
                    dropZone.HandleDrop(rectTransform);
                    break;
                }
            }

            ReturnToOriginalParent();
            if (wasDropped) OnDrop?.Invoke();
        }

        private void ReturnToOriginalParent()
        {
            // Возвращаем в оригинальный parent
            transform.SetParent(originalParent, false);

            // Восстанавливаем оригинальный порядок в hierarchy
            transform.SetSiblingIndex(originalSiblingIndex);

            // Если был в Layout Group - включаем обратно правила layout
            if (wasInLayoutGroup && layoutElement != null)
            {
                layoutElement.ignoreLayout = false;

                // Принудительно обновляем layout
                LayoutRebuilder.ForceRebuildLayoutImmediate(originalParent as RectTransform);
            }
        }

        // Метод для принудительного возврата (можно вызывать извне)
        public void ResetPosition()
        {
            ReturnToOriginalParent();
        }
    }
}