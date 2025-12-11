using System;
using Game.Scripts.Gameplay.DataLayer.Battle;
using Game.Scripts.Gameplay.PresentersLayer.Battle;
using Game.Scripts.Infrastructure.TimeManagement;
using Game.Scripts.Infrastructure.UI;
using Game.Scripts.UI;
using Game.Scripts.UIContracts;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer.BattleScreens
{
    [UIScreen("BattleStage", ContractTypes = new[] { typeof(IBattleStageScreen) })]
    public class BattleStageScreen : UIScreen, IBattleStageScreen
    {
        [Inject] private IBattleDataProvider _battleDataProvider;
        [Inject] private IBattlePreparationCoordinator _battlePreparationCoordinator;
        [Inject] private ITimerService _timerService;

        [SerializeField] private TextMeshProUGUI stageLabel;
        [SerializeField] private TextMeshProUGUI timerLabel;
        [SerializeField] private Button readyButton;

        private readonly CompositeDisposable _disposables = new();

        private void OnEnable()
        {
            if (_battleDataProvider == null || _battlePreparationCoordinator == null)
                return;

            readyButton?.onClick.AddListener(OnReadyClicked);
            Bind();
        }

        private void OnDisable()
        {
            readyButton?.onClick.RemoveListener(OnReadyClicked);
            _disposables.Clear();
        }

        private void Update()
        {
            if (_battlePreparationCoordinator == null || _timerService == null)
                return;

            UpdateTimerLabel();
        }

        private void Bind()
        {
            _disposables.Clear();

            _battleDataProvider.Phase
                .Subscribe(phase => stageLabel.text = phase.ToString())
                .AddTo(_disposables);
        }

        private void UpdateTimerLabel()
        {
            if (_timerService.HasTimer(_battlePreparationCoordinator.TimerId))
            {
                var timer = _timerService.GetTimer(_battlePreparationCoordinator.TimerId);
                timerLabel.text = timer.TimeLeft.ToString(@"mm\:ss");
                return;
            }

            timerLabel.text = TimeSpan.Zero.ToString(@"mm\:ss");
        }

        private void OnReadyClicked()
        {
            _battlePreparationCoordinator.MarkReady();
        }
    }
}
