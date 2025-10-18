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
    [UIScreen("CharacterStats", ContractTypes = new[] { typeof(ICharacterStatsScreen) })]
    public class CharacterStatsScreen : UIScreen, ICharacterStatsScreen
    {
        [Inject] private IPlayerPresenter _playerPresenter;
        [Inject] private ISpriteService _spriteService;
        [SerializeField] private StatsContainer container;

        private void Start()
        {
            container.Bind(
                _playerPresenter.PlayerStats,
                (statValue, uiElement) => uiElement.Setup(statValue, _spriteService),
                element => Debug.Log("Created: " + element.name)
            );
        }
    }
}
