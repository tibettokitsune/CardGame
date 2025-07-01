using System.Threading.Tasks;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.Gameplay.ViewsLayer;
using Game.Scripts.Infrastructure;
using Game.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using Zenject;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    public class TakeCardStageController : MonoBehaviour
    {
        [SerializeField] private PlayableDirector cinematic;
        [Inject] private IPlayerPresenter _playerPresenter;
        [Inject] private IUIService _uiService;
        [SerializeField] private ParticleSystem effect;
        [SerializeField] private Camera overlayCamera;
        private void Start()
        {
            ProcessStage();
        }

        private async void ProcessStage()
        {
            cinematic.Play();
            await cinematic.AwaitPlayableEnd();
            effect.Play(true);
            var baseCameraData  = Camera.main.GetUniversalAdditionalCameraData();
            if (!baseCameraData.cameraStack.Contains(overlayCamera))
            {
                baseCameraData.cameraStack.Add(overlayCamera);
            }
            var screen = await _uiService.ShowScreen("OpenDoorScreen") as OpenDoorScreen;
            await screen.ShowDoorCard(_playerPresenter.CurrentDoor.Value);
            await Task.Delay(2000);
            baseCameraData.cameraStack.Remove(overlayCamera);
            var fadescreen = await _uiService.ShowScreen("FadeScreen") as FadeScreen;
        }
    }
}