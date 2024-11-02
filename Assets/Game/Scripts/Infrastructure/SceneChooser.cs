using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;


namespace Game.Scripts.Infrastructure
{
    public class SceneChooser : MonoBehaviour
    {
        private ISceneManagementService _sceneManagementService;
        private SceneConfig _sceneConfig;
        private string[] _gameplayScenes, _mainMenuScenes;

        [Inject]
        public void Construct (SceneConfig sceneConfig, ISceneManagementService sceneManagementService)
        {
            _sceneManagementService = sceneManagementService;
            _sceneConfig = sceneConfig;
            _gameplayScenes = _sceneConfig.gameplayScenes;
            _mainMenuScenes = _sceneConfig.mainMenuScenes;
        }

        [ValueDropdown(nameof(_mainMenuScenes))]
        [SerializeField]
        private string _mainMenuSelectedScene;

        [ValueDropdown(nameof(_gameplayScenes))]
        [SerializeField]
        private string _gameplaySelectedScene;

        [Button]
        public void LoadGameplaySceneGroup() => _sceneManagementService.LoadScenes(_gameplayScenes);

        [Button]
        public void LoadMenuSceneGroup() => _sceneManagementService.LoadScenes(_mainMenuScenes);

        [Button]
        public void SetGameplayActiveScene() => _sceneManagementService.SelectActiveScene(_gameplaySelectedScene);

        [Button]
        public void SetMainMenuActiveScene() => _sceneManagementService.SelectActiveScene(_mainMenuSelectedScene);


    }

}
