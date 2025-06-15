using Game.Scripts.Gameplay.Lobby.Deck;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.Infrastructure.Configs.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyScreens
{
    public class HandCardView : MonoBehaviour
    {
        [SerializeField] private DragAndDropWidget dragAndDropWidget;
        [SerializeField] private TextMeshProUGUI attackLbl;
        [SerializeField] private TextMeshProUGUI hpLbl;
        [SerializeField] private TextMeshProUGUI descriptionLbl;
        [SerializeField] private HandCardAnimation animation;

        [SerializeField] private MultipleLayerImageWidget mainIcon;
        [SerializeField] private MultipleLayerImageWidget bgIcon;
        [SerializeField] private LayoutElement layoutElement;

        private string _cardId;
        

        private void OnValidate()
        {
            if (dragAndDropWidget == null) 
                dragAndDropWidget = GetComponent<DragAndDropWidget>();
        }

        public void Setup(CardEntity cardEntity)
        {
            _cardId = cardEntity.ID;
            dragAndDropWidget.Setup(_cardId);
            descriptionLbl.text = $"{cardEntity.Name}\n{cardEntity.Description}";
            mainIcon.Setup(cardEntity.MainLayer);
            bgIcon.Setup(cardEntity.BackgroundLayer);
        }

        public void EnableGrouping()
        {
            layoutElement.enabled = true;
        }

        private async void OnEnable()
        {
            await animation.PlayEnterAnimation();
        }
    }
}