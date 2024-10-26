using UnityEngine;
using Zenject;

namespace Game.Scripts.Infrastructure
{
    public class GameManager : IInitializable
    {
        private readonly ISceneManagementService _sceneManagementService;

        public GameManager(ISceneManagementService sceneManagementService)
        {
            _sceneManagementService = sceneManagementService;
        }

        public void Initialize()
        {
            Debug.Log("Init");
        }
    }
}
