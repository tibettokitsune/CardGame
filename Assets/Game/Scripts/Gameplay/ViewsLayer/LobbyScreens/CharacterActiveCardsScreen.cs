using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.Infrastructure.AsyncAssets;
using Game.Scripts.UI;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyScreens
{
    public class CharacterActiveCardsScreen : UIScreen
    {
        [Inject] private IPlayerPresenter _playerPresenter;
        [Inject] private ISpriteService _spriteService;
        [SerializeField] private EquipmentCardsContainer _container;

        private void Start()
        {
            _container.Bind(
                _playerPresenter.PlayerEquipment,
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