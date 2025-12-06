using System;
using System.Threading.Tasks;
using Game.Scripts.Infrastructure.SceneManagment;
using UnityEngine;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    [EffectOrder(-10)]
    public class LoadBattleSceneEffect : IStateEnterEffect<BattleState>
    {
        private readonly ISceneManagerService _sceneManagerService;

        public LoadBattleSceneEffect(ISceneManagerService sceneManagerService)
        {
            _sceneManagerService = sceneManagerService;
        }

        public async Task OnEnterAsync()
        {
            try
            {
                await _sceneManagerService.LoadScene("Battle", SceneLayer.GameplayElement, false);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }

    [EffectOrder(0)]
    public class UnloadBattleSceneEffect : IStateExitRequestEffect<BattleState>
    {
        private readonly ISceneManagerService _sceneManagerService;

        public UnloadBattleSceneEffect(ISceneManagerService sceneManagerService)
        {
            _sceneManagerService = sceneManagerService;
        }

        public async Task OnExitRequestAsync()
        {
            try
            {
                await _sceneManagerService.UnloadScene("Battle", SceneLayer.GameplayElement);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
