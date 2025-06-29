using System;
using Game.Scripts.Infrastructure.TimeManagement;
using Game.Scripts.UI;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer
{
    public class TimerScreen : UIScreen
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