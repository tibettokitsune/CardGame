using Game.Scripts.Gameplay.Lobby.Deck;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer
{
    public class ViewsLayerInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindFactory<HandCardView, HandCardFactory>()
                .FromComponentInNewPrefabResource("Prefabs/card");
        }
    }
}