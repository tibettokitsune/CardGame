using Game.Scripts.Infrastructure;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Scripts.UI
{
    public class MenuScreen : UIScreen
    {
        [Inject] private IGameManager gameManager;
        [SerializeField] private Button startButton;

        private void Awake()
        {
            startButton.onClick.AddListener(OnStartBtnClicked);
        }

        private void OnStartBtnClicked()
        {
            gameManager.GoToGame();
        }
    }
}