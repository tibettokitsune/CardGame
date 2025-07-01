using System.Threading.Tasks;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.Gameplay.ViewsLayer.LobbyScreens;
using Game.Scripts.Infrastructure.AsyncAssets;
using Game.Scripts.UI;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer
{
    public class OpenDoorScreen : UIScreen
    {
        [Inject] private ISpriteService _spriteService;
        [SerializeField] private HandCardView cardView;

        public async Task ShowDoorCard(CardEntity cardEntity)
        {
            cardView.gameObject.SetActive(true);
            cardView.Setup(cardEntity, _spriteService);
        }
    }
}