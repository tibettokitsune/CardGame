using Game.Scripts.Gameplay.PresentersLayer.Player;
using TMPro;
using UnityEngine;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyScreens
{
    public class EquippedCardView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cardName;
        [SerializeField] private TextMeshProUGUI cardBonus;
        
        public void Setup(CardEntity data)
        {
            cardName.text = data.Name;
            cardBonus.text = "WOWO";
        }
    }
}