using System.Threading.Tasks;
using Game.Scripts.Gameplay.PresentersLayer.GameStates;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.Infrastructure;
using Game.Scripts.UI;
using Game.Scripts.UIContracts;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer.TakeEventCardStage
{
    public class TakeEventCardStageViewModel : MonoBehaviour
    {
        [SerializeField] private PlayableDirector cinematic;
        [SerializeField] private Camera overlayCamera;
        [Inject] private IPlayerPresenter _playerPresenter;
        [Inject] private IUIService _uiService;
        [Inject] private IFinishTakeEventCardStateUseCase _finishTakeEventCardStateUseCase;

        private void Start()
        {
            ProcessStage();
        }

        private async void ProcessStage()
        {
            cinematic.Play();
            await cinematic.AwaitPlayableEnd();
            var baseCameraData = Camera.main.GetUniversalAdditionalCameraData();
            if (!baseCameraData.cameraStack.Contains(overlayCamera))
            {
                baseCameraData.cameraStack.Add(overlayCamera);
            }

            var screen = await _uiService.ShowAsync<IOpenDoorScreen>();
            await screen.ShowDoorCard(_playerPresenter.CurrentDoor.Value);
            baseCameraData.cameraStack.Remove(overlayCamera);
            await Task.Delay(3000);
            await _uiService.ClearAsync();
            _finishTakeEventCardStateUseCase.Execute();
        }
    }
}
