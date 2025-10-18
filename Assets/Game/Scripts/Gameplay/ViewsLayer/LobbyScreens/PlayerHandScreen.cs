using Game.Scripts.Gameplay.PresentersLayer.Contracts.UI;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.Infrastructure.AsyncAssets;
using Game.Scripts.Infrastructure.UI;
using Game.Scripts.UI;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyScreens
{
    [UIScreen("PlayerHand", ContractTypes = new[] { typeof(IPlayerHandScreen) })]
    public class PlayerHandScreen : UIScreen, IPlayerHandScreen
    {
        [SerializeField] private HandCardView cardPrefab;
        [Inject] private IPlayerPresenter _playerPresenter;
        [Inject] private ISpriteService _spriteService;
        [SerializeField] private HandCardsContainer _container;

        private void Start()
        {
            _container.Bind(
                _playerPresenter.PlayerHand,
                (data, element) =>
                {
                    element.Setup(data, _spriteService);
                    return Disposable.Empty;
                },
                element =>
                {
                    // Дополнительная настройка при создании
                }
            );
        }
    }
}
