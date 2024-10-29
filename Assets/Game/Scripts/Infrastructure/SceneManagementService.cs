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
    }
}