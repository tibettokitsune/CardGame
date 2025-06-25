using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.Infrastructure.AsyncAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyScreens
{
    public class StatView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cardName;
        [SerializeField] private TextMeshProUGUI cardBonus;
        [SerializeField] private Image mainIcon;
        
        public async void Setup(StatEntity cardEntity, ISpriteService spriteService)
        {
            cardName.text = cardEntity.Name;
            cardBonus.text = $"{cardEntity.Value.ToString(cardEntity.Format)}";
            var mainLayerIcon = 
                await spriteService.LoadSpriteForObject(cardEntity.Icon, mainIcon.gameObject);
            mainIcon.sprite = mainLayerIcon;
            var bgLayerIcon = 
                await spriteService.LoadSpriteForObject(cardEntity.Icon, mainIcon.gameObject);
            mainIcon.sprite = bgLayerIcon;
        }
    }
}