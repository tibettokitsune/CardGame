using System.Collections.Generic;
using Game.Scripts.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Game.Scripts.Infrastructure
{
    public interface ISceneManagementService
    {
        List<AsyncOperation> LoadScene(string infraScene, string[] additiveScenes);
    }

    public class SceneManagementService : ISceneManagementService
    {
        private string _currentBoot = string.Empty;
        private string _currentInfraScene = string.Empty;
        private string[] _currentAdditiveScenes = new string[0];

        private AsyncOperation LoadScene(string scene)
        {
            return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        }

        private AsyncOperation UnloadScene(string scene)
        {
            return SceneManager.UnloadSceneAsync(scene);
        }

        public List<AsyncOperation> LoadScene(string infraScene, string[] additiveScenes)
        {
            var operations = new List<AsyncOperation>();

            if (!_currentInfraScene.Equals(infraScene))
            {
                if(!string.IsNullOrEmpty(_currentInfraScene))
                    operations.Add(UnloadScene(_currentInfraScene));
                operations.Add(LoadScene(infraScene));
                _currentInfraScene = infraScene;
            }

            if (IsNewAdditiveListChanged(additiveScenes))
            {
                
                foreach (var additive in _currentAdditiveScenes)
                {
                    operations.Add(UnloadScene(additive));
                }

                foreach (var additive in additiveScenes)
                {
                    operations.Add(LoadScene(additive));
                }
            }
            
            return operations;
        }

        private bool IsNewAdditiveListChanged(string[] additiveScenes)
        {
            for (var i = 0; i < additiveScenes.Length; i++)
            {
                if (!additiveScenes[i].Equals(_currentAdditiveScenes[i]))
                    return true;
            }

            return false;
        }
    }
}