using Game.Scripts.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Game.Scripts.Infrastructure
{
    public interface ISceneManagementService
    {
        AsyncOperation LoadScene(string scene);
        AsyncOperation UnloadScene(string scene);
        void SelectScene(string scene);
    }
    
    public class SceneManagementService : ISceneManagementService
    {
        public AsyncOperation LoadScene(string scene)
        {
            return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        }

        public AsyncOperation UnloadScene(string scene)
        {
            return SceneManager.UnloadSceneAsync(scene);
        }

        public void SelectScene(string scene)
        {
            Scene targetScene = SceneManager.GetSceneByName(scene);

            if (targetScene.IsValid() && targetScene.isLoaded)
            {
                SceneManager.SetActiveScene(targetScene);
                DisableInactiveListeners();
                Debug.Log($"—цена {scene} установлена активной.");
            }
            else
            {
                Debug.LogWarning($"—цена {scene} не загружена или недействительна.");
            }
        }

        private void DisableInactiveListeners()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                bool isActiveScene = scene == SceneManager.GetActiveScene();

                foreach (GameObject rootObj in scene.GetRootGameObjects())
                {
                    var audioListener = rootObj.GetComponentInChildren<AudioListener>();
                    if (audioListener != null)
                    {
                        audioListener.enabled = isActiveScene;
                    }
                }
            }
        }
    }
}