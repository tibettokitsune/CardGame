using Game.Scripts.Gameplay.Lobby.Deck;
using Game.Scripts.Gameplay.Lobby.GameStates;
using Game.Scripts.Gameplay.Lobby.Player;
using Game.Scripts.UI;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.Lobby
{
    public class LobbyInstaller : MonoInstaller
    {
        [SerializeField] private HandCardView cardViewPrefab;

        public override void InstallBindings()
        {
            Container.Bind<HandCardView>().FromComponentInNewPrefab(cardViewPrefab).AsTransient();
        
            Container.BindFactory<HandCardView, HandCardFactory>()
                .FromComponentInNewPrefab(cardViewPrefab)
                .AsTransient();
            
            Container.BindInterfacesAndSelfTo<LobbyPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<DeckPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerDataProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<FirstEnterInGameState>().AsSingle();
            Container.BindInterfacesAndSelfTo<PreparePlayerState>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerPresenter>().AsSingle();
        }
    }
}