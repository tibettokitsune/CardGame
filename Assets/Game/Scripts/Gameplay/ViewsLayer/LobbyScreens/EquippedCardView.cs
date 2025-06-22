using Game.Scripts.Gameplay.PresentersLayer.Player;
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
        [SerializeField] private Image bgIcon;
        [SerializeField] private Image mainIcon;
        
        public async void Setup(EquipmentCardEntity cardEntity, ISpriteService spriteService)
        {
            cardName.text = cardEntity.Name;
            cardBonus.text = $"{cardEntity.EquipmentDescription}";
            var mainLayerIcon = 
                await spriteService.LoadSpriteForObject(cardEntity.MainLayer, mainIcon.gameObject);
            mainIcon.sprite = mainLayerIcon;
            var bgLayerIcon = 
                await spriteService.LoadSpriteForObject(cardEntity.BackgroundLayer, bgIcon.gameObject);
            bgIcon.sprite = bgLayerIcon;
        }
    }
}