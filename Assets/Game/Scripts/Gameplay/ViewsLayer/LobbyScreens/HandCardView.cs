using System.Threading;
using Game.Scripts.Gameplay.Lobby.Deck;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.Infrastructure.AsyncAssets;
using Game.Scripts.Infrastructure.Configs.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyScreens
{
    public class HandCardView : MonoBehaviour
    {
        [SerializeField] private DragAndDropWidget dragAndDropWidget;
        [SerializeField] private TextMeshProUGUI descriptionLbl;
        [SerializeField] private HandCardAnimation animation;

        [SerializeField] private Image mainIcon;
        [SerializeField] private Image bgIcon;
        [SerializeField] private LayoutElement layoutElement;

        private string _cardId;

        private CancellationTokenSource _cancellationTokenSource;

        private void OnValidate()
        {
            if (dragAndDropWidget == null)
                dragAndDropWidget = GetComponent<DragAndDropWidget>();
        }

        public async void Setup(CardEntity cardEntity, ISpriteService spriteService)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _cardId = cardEntity.ID;
            descriptionLbl.text = $"{cardEntity.Name}\n{cardEntity.Description}";
            var mainLayerIcon =
                await spriteService.LoadSpriteForObject(cardEntity.MainLayer, mainIcon.gameObject, token);
            mainIcon.sprite = mainLayerIcon;
            var bgLayerIcon =
                await spriteService.LoadSpriteForObject(cardEntity.BackgroundLayer, bgIcon.gameObject, token);
            bgIcon.sprite = bgLayerIcon;
            
            dragAndDropWidget?.Setup(_cardId);
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