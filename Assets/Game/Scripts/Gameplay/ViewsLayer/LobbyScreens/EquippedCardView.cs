using System.Threading;
using Game.Scripts.UIContracts;
using Game.Scripts.Infrastructure.AsyncAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyScreens
{
    public class EquippedCardView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cardName;
        [SerializeField] private TextMeshProUGUI cardBonus;
        [SerializeField] private Image mainIcon;
        [SerializeField] private Image bgIcon;
        private CancellationTokenSource _cancellationTokenSource;
        public async void Setup(EquipmentCardViewData cardEntity, ISpriteService spriteService)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            cardName.text = cardEntity.Name;
            cardBonus.text = $"{cardEntity.EquipmentDescription}";
            var mainLayerIcon = 
                await spriteService.LoadSpriteForObject(cardEntity.MainLayer, mainIcon.gameObject, token);
            mainIcon.sprite = mainLayerIcon;
            var bgLayerIcon = 
                await spriteService.LoadSpriteForObject(cardEntity.BackgroundLayer, bgIcon.gameObject, token);
            bgIcon.sprite = bgLayerIcon;
        }
    }
}
