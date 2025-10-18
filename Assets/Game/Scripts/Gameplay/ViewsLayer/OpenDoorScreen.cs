using System.Threading.Tasks;
using Game.Scripts.Gameplay.PresentersLayer.Contracts.UI;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.Gameplay.ViewsLayer.LobbyScreens;
using Game.Scripts.Infrastructure.AsyncAssets;
using Game.Scripts.Infrastructure.UI;
using Game.Scripts.UI;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer
{
    [UIScreen("OpenDoorScreen", ContractTypes = new[] { typeof(IOpenDoorScreen) })]
    public class OpenDoorScreen : UIScreen, IOpenDoorScreen
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
