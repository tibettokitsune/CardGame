using System.Collections.Generic;
using Game.Scripts.Infrastructure.Loading;
using Game.Scripts.Infrastructure.Menu;
using Game.Scripts.UI;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Infrastructure
{
    public class ServiceInitializer : IInitializable
    {
        [Inject] private LoadingScreen _screen;
        [Inject] private IMenuPresenter _menuPresenter;
        private readonly IEnumerable<IAsyncInitializable> _services;

        public ServiceInitializer(IEnumerable<IAsyncInitializable> services)
        {
            _services = services;
        }

        public async void Initialize()
        {
            Debug.Log("Start services initialization");
            using var loading = _screen.Show();
            foreach (var service in _services)
            {
                await service.InitializeAsync();
            }
            Debug.Log("Finish services initialization");

            await _menuPresenter.LoadMenu();
        }
    }
}