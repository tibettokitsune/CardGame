using System;
using Game.Scripts.Gameplay.PresentersLayer.Contracts.UI;
using Game.Scripts.Infrastructure.TimeManagement;
using Game.Scripts.Infrastructure.UI;
using Game.Scripts.UI;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer
{
    [UIScreen("TimerScreen", ContractTypes = new[] { typeof(ITimerScreen) })]
    public class TimerScreen : UIScreen, ITimerScreen
    {
        [Inject] private ITimerService _timerService;
        [SerializeField] private TextMeshProUGUI timerLbl;

        private void Update()
        {
            var timer = _timerService.GetTimer("PrepareToRound");
            timerLbl.text = timer.TimeLeft.ToString(@"mm\:ss");
        }
    }
}
