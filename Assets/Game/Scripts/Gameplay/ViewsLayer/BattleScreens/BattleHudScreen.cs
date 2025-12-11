using System;
using Game.Scripts.Gameplay.DataLayer.Battle;
using Game.Scripts.Infrastructure.UI;
using Game.Scripts.UI;
using Game.Scripts.UIContracts;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer.BattleScreens
{
    [UIScreen("BattleHud", ContractTypes = new[] { typeof(IBattleHudScreen) })]
    public class BattleHudScreen : UIScreen, IBattleHudScreen
    {
        [Inject] private IBattleDataProvider _battleDataProvider;

        [SerializeField] private TextMeshProUGUI playerHpLabel;
        [SerializeField] private TextMeshProUGUI monsterHpLabel;
        [SerializeField] private TextMeshProUGUI monsterNameLabel;

        private readonly CompositeDisposable _disposables = new();

        private void OnEnable()
        {
            if (_battleDataProvider == null)
                return;

            Bind();
        }

        private void OnDisable()
        {
            _disposables.Clear();
        }

        private void Bind()
        {
            _disposables.Clear();

            _battleDataProvider.PlayerHealth
                .CombineLatest(_battleDataProvider.PlayerMaxHealth, FormatHp)
                .Subscribe(value => playerHpLabel.text = value)
                .AddTo(_disposables);

            _battleDataProvider.MonsterHealth
                .CombineLatest(_battleDataProvider.MonsterMaxHealth, FormatHp)
                .Subscribe(value => monsterHpLabel.text = value)
                .AddTo(_disposables);

            _battleDataProvider.CurrentMonster
                .Subscribe(card => monsterNameLabel.text = card?.Name ?? "???")
                .AddTo(_disposables);
        }

        private string FormatHp(float current, float max)
        {
            max = Mathf.Max(max, 1f);
            var currentInt = Math.Max(0, Mathf.CeilToInt(current));
            var maxInt = Mathf.CeilToInt(max);
            return $"{currentInt}/{maxInt}";
        }
    }
}
