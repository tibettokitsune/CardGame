using Game.Scripts.Infrastructure.Menu;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Scripts.UI
{
    public class MenuScreen : UIScreen
    {
        [Inject] private IStartGameUseCase startGameUseCase;
        [Inject] private IShowSettingsUseCase showSettingsUseCase;
        [Inject] private IExitGameUseCase exitGameUseCase;
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        private void Start()
        {
            startButton.onClick.AddListener(OnStartClick);
            settingsButton.onClick.AddListener(OnSettingsClick);
            exitButton.onClick.AddListener(OnExitClick);
        }

        private void OnExitClick()
        {
            exitGameUseCase.ExitGame();
        }

        private void OnSettingsClick()
        {
            showSettingsUseCase.ShowSettings();
        }

        private void OnStartClick()
        {
            startGameUseCase.StartLobby();
        }
    }
}