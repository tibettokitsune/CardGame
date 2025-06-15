using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyScreens
{
    public class DropZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Drop Zone Settings")]
        [SerializeField] private bool highlightOnHover = true;
        [SerializeField] private Color highlightColor = new Color(0.8f, 0.9f, 1f, 0.5f);
    
        private Image image;
        private Color originalColor;
        private bool isHighlighted;
    
        private void Awake()
        {
            image = GetComponent<Image>();
            if (image != null)
            {
                originalColor = image.color;
            }
        }
    
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (highlightOnHover && image != null)
            {
                image.color = highlightColor;
                isHighlighted = true;
            }
        }
    
        public void OnPointerExit(PointerEventData eventData)
        {
            if (highlightOnHover && image != null)
            {
                image.color = originalColor;
                isHighlighted = false;
            }
        }
    
        public void HandleDrop(RectTransform droppedItem)
        {
            // Здесь можно добавить проверки, можно ли сюда бросать этот объект
        
            // Устанавливаем новый parent
            droppedItem.SetParent(transform, false);
        
            // Если есть Layout Group - включаем правила layout
            var layoutElement = droppedItem.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                layoutElement.ignoreLayout = false;
            }
        
            // Принудительно обновляем layout
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        
            Debug.Log($"Объект {droppedItem.name} сброшен в зону {name}");
        
            // Возвращаем оригинальный цвет, если был подсвечен
            if (isHighlighted && image != null)
            {
                image.color = originalColor;
                isHighlighted = false;
            }
        }
    }
}