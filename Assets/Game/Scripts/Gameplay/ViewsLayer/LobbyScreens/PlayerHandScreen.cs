using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.UI;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyScreens
{
    public class PlayerHandScreen : UIScreen
    {
        [SerializeField] private HandCardView cardPrefab;
        [Inject] private IPlayerPresenter _playerPresenter;
        [SerializeField] private HandCardsContainer _container;

        private void Start()
        {
            _container.Bind(
                _playerPresenter.PlayerHand,
                (data, element) =>
                {
                    element.Setup(data);
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