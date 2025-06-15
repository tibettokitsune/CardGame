using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.UI;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyScreens
{
    public class CharacterStatsScreen : UIScreen
    {
        [Inject] private IPlayerPresenter _playerPresenter;
        
        private void Start()
        {
        }
    }
}