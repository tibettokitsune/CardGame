using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.Gameplay.ViewsLayer.LobbyScreens;
using Game.Scripts.Infrastructure.AsyncAssets;
using Game.Scripts.Infrastructure.UI;
using Game.Scripts.UI;
using Game.Scripts.UIContracts;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer.BattleScreens
{
    [UIScreen("BattleHand", ContractTypes = new[] { typeof(IBattleHandScreen) })]
    public class BattleHandScreen : UIScreen, IBattleHandScreen
    {
        [SerializeField] private HandCardView cardPrefab;
        [SerializeField] private HandCardsContainer _container;
        [Inject] private IPlayerPresenter _playerPresenter;
        [Inject] private ISpriteService _spriteService;

        private void Start()
        {
            _container.Bind(
                _playerPresenter.BattleHand,
                (data, element) =>
                {
                    element.Setup(data, _spriteService);
                    return Disposable.Empty;
                },
                element => { }
            );
        }
    }
}
