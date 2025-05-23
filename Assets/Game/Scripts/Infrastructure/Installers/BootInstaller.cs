using System;
using Game.Scripts.Infrastructure.Configs;
using Game.Scripts.Infrastructure.Loading;
using Game.Scripts.Infrastructure.Menu;
using Game.Scripts.Infrastructure.SceneManagment;
using Game.Scripts.Infrastructure.UI;
using Game.Scripts.UI;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Infrastructure.Installers
{
    public class BootInstaller : MonoInstaller
    {
        [SerializeField] private Transform uiRoot;
        public override void InstallBindings()
        {
            InstallUI();
            InstallService();
            Container.BindInterfacesTo<MenuController>().AsSingle();
        }

        private void InstallService()
        {
            Container.BindInterfacesAndSelfTo<SceneManagerService>().AsSingle();
            Container.BindInterfacesAndSelfTo<ConfigService>().AsSingle();
            Container.BindInterfacesAndSelfTo<ServiceInitializer>().AsSingle();
            Container.BindInterfacesAndSelfTo<UIService>().AsSingle().WithArguments(uiRoot);
        }

        private void InstallUI()
        {
            Container.Bind<LoadingScreen>().FromComponentInHierarchy(true).AsSingle();
            Container.BindFactory<UIScreen, Transform, Type, UIScreen, UIScreenFactory>()
                .FromFactory<UIScreenFactory>();

        }
    }
}