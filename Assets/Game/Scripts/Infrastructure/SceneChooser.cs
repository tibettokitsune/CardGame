using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;


namespace Game.Scripts.Infrastructure
{
    public class SceneChooser : MonoBehaviour
    {
        [Inject] private readonly GameManager _gameManager;

        [SerializeField, FoldoutGroup("Scenes")]
        private string[] _gameplayScenes, _mainMenuScenes;

        [ValueDropdown(nameof(_mainMenuScenes))]
        [SerializeField]
        private string _mainMenuSelectedScene;

        [ValueDropdown(nameof(_gameplayScenes))]
        [SerializeField]
        private string _gameplaySelectedScene;

        [Button]
        public void LoadGameplaySceneGroup() => _gameManager.LoadScenes(_gameplayScenes);

        [Button]
        public void LoadMenuSceneGroup() => _gameManager.LoadScenes(_mainMenuScenes);

        [Button]
        public void SetGameplayActiveScene() => _gameManager.SelectActiveScene(_gameplaySelectedScene);

        [Button]
        public void SetMainMenuActiveScene() => _gameManager.SelectActiveScene(_mainMenuSelectedScene);


    }

}
