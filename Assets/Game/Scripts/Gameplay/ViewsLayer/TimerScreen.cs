using System;
using Game.Scripts.Infrastructure.TimeManagement;
using Game.Scripts.Infrastructure.UI;
using Game.Scripts.UI;
using Game.Scripts.UIContracts;
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
        private const string TimerId = "PrepareToRound";

        private void Update()
        {
            if (!_timerService.HasTimer(TimerId))
            {
                timerLbl.text = TimeSpan.Zero.ToString(@"mm\:ss");
                return;
            }

            var timer = _timerService.GetTimer(TimerId);
            timerLbl.text = timer.TimeLeft.ToString(@"mm\:ss");
        }
    }
}
