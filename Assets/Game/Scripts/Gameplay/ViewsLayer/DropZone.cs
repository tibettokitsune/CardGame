using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.ViewsLayer
{
    public abstract class DropZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
    
        public virtual void HandleDrop(string cardId)
        {
            if (isHighlighted && image != null)
            {
                image.color = originalColor;
                isHighlighted = false;
            }
        }
    }
}