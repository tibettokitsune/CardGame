using Game.Scripts.Gameplay.Lobby.Deck;
using Game.Scripts.Infrastructure.Configs.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.ViewsLayer
{
    public class HandCardView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI attackLbl;
        [SerializeField] private TextMeshProUGUI hpLbl;
        [SerializeField] private TextMeshProUGUI descriptionLbl;
        [SerializeField] private HandCardAnimation animation;

        [SerializeField] private MultipleLayerImageWidget mainIcon;
        [SerializeField] private MultipleLayerImageWidget bgIcon;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private CardController controller;
        
        public void Setup(string cardName, string cardDescription, CardLayerDataConfig mainLayer, CardLayerDataConfig backgroundLayer)
        {
            descriptionLbl.text = $"{cardName}\n{cardDescription}";
            
            mainIcon.Setup(mainLayer);
            bgIcon.Setup(backgroundLayer);
        }

        public void EnableGrouping()
        {
            layoutElement.enabled = true;
            controller = gameObject.AddComponent<CardController>();
        }

        private async void OnEnable()
        {
            await animation.PlayEnterAnimation();
        }
    }
}