using System.Threading.Tasks;
using Game.Scripts.Infrastructure.Loading;
using Game.Scripts.Infrastructure.UI;
using Game.Scripts.Infrastructure.SceneManagment;
using Game.Scripts.UI;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Infrastructure.Menu
{
    public class MenuController : IMenuController, IStartGameUseCase, IShowSettingsUseCase, IExitGameUseCase
    {
        [Inject] private ISceneManagerService sceneManagerService;
        [Inject] private LoadingScreen _loadingScreen;
        [Inject] private IUIService uiService;
        public async Task LoadMenu()
        {
            Debug.Log("Loading menu...");
            using var loadingScreen = _loadingScreen.Show();
            await uiService.ShowAsync<MenuScreen>();
            await sceneManagerService.LoadScene("Menu", SceneLayer.GameStage);
            Debug.Log("Menu loaded");
        }

        public async Task StartLobby()
        {
            Debug.Log("Starting lobby...");
            using var loadingScreen = _loadingScreen.Show();
            await uiService.ClearAsync();
            await sceneManagerService.LoadScene("Lobby", SceneLayer.GameStage, true);
            Debug.Log("Lobby started");
        }

        public Task ShowSettings()
        {
            throw new System.NotImplementedException();
        }

        public Task ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
            Application.Quit();
            return Task.CompletedTask;
        }
    }
}
