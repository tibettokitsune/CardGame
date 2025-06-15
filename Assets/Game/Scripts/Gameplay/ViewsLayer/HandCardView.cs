using Game.Scripts.Gameplay.Lobby.Deck;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.Infrastructure.Configs.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.ViewsLayer
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
        private IEquipCardUseCase _equipCardUseCase;
        
        private void Start()
        {
            dragAndDropWidget.OnDrop += OnAttemptToEquip;
        }

        private void OnAttemptToEquip()
        {
            _equipCardUseCase.Execute(_cardId);
        }

        private void OnValidate()
        {
            if (dragAndDropWidget == null) 
                dragAndDropWidget = GetComponent<DragAndDropWidget>();
        }

        public void Setup(string cardName, string cardDescription, CardLayerDataConfig mainLayer,
            CardLayerDataConfig backgroundLayer, string cardId, IEquipCardUseCase equipCardUseCase)
        {
            _cardId = cardId;
            _equipCardUseCase = equipCardUseCase;
            descriptionLbl.text = $"{cardName}\n{cardDescription}";
            mainIcon.Setup(mainLayer);
            bgIcon.Setup(backgroundLayer);
        }

        public void EnableGrouping()
        {
            layoutElement.enabled = true;
        }

        private async void OnEnable()
        {
            await animation.PlayEnterAnimation();
        }

        private void OnDestroy()
        {
            dragAndDropWidget.OnDrop -= OnAttemptToEquip;
        }
    }
}