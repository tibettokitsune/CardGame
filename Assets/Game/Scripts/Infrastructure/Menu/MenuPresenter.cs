using System.Threading.Tasks;
using Game.Scripts.Infrastructure.Loading;
using Game.Scripts.Infrastructure.SceneManagment;
using Game.Scripts.UI;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Infrastructure.Menu
{
    public interface IMenuPresenter
    {
        Task LoadMenu();
    }

    public interface IStartGameUseCase
    {
        Task StartGame();
    }

    public interface IShowSettingsUseCase
    {
        Task ShowSettings();
    }
    
    public interface IExitGameUseCase
    {
        Task ExitGame();
    }

    public class MenuPresenter : IMenuPresenter, IStartGameUseCase, IShowSettingsUseCase, IExitGameUseCase
    {
        [Inject] private ISceneManagerService sceneManagerService;
        [Inject] private LoadingScreen _loadingScreen;
        [Inject] private IUIService<UIScreen> uiService;
        public async Task LoadMenu()
        {
            Debug.Log("Loading menu...");
            using var loadingScreen = _loadingScreen.Show();
            await uiService.ShowScreen<MenuScreen>("Menu");
            await sceneManagerService.LoadScene("Menu", SceneLayer.GameStage);
            Debug.Log("Menu loaded");
        }

        public async Task StartGame()
        {
            Debug.Log("Starting game...");
            using var loadingScreen = _loadingScreen.Show();
            await uiService.Clear();
            await sceneManagerService.LoadScene("Game", SceneLayer.GameStage);
            Debug.Log("Game started");
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