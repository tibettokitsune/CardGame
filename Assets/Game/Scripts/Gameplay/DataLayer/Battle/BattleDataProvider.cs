using System;
using UniRx;
using UnityEngine;

namespace Game.Scripts.Gameplay.DataLayer.Battle
{
    public enum BattlePhase
    {
        Preparation,
        Strike,
        Outcome
    }

    public sealed class BattleOpponentData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public float Damage { get; set; }
        public float Reward { get; set; }
        public float Health { get; set; }
        public string ViewId { get; set; }
    }

    public interface IBattleDataProvider
    {
        ReactiveProperty<float> PlayerHealth { get; }
        ReactiveProperty<float> PlayerMaxHealth { get; }
        ReactiveProperty<float> MonsterHealth { get; }
        ReactiveProperty<float> MonsterMaxHealth { get; }
        ReactiveProperty<BattleOpponentData> CurrentMonster { get; }
        ReactiveProperty<BattlePhase> Phase { get; }
        bool IsBattleActive { get; }

        void StartBattle(BattleOpponentData monster, float playerHealth);
        void ApplyExchange(float playerDamage, float monsterDamage);
        bool HasWinner(out bool playerWon);
        void Reset();
    }

    public class BattleDataProvider : IBattleDataProvider
    {
        public ReactiveProperty<float> PlayerHealth { get; } = new();
        public ReactiveProperty<float> PlayerMaxHealth { get; } = new();
        public ReactiveProperty<float> MonsterHealth { get; } = new();
        public ReactiveProperty<float> MonsterMaxHealth { get; } = new();
        public ReactiveProperty<BattleOpponentData> CurrentMonster { get; } = new();
        public ReactiveProperty<BattlePhase> Phase { get; } = new(BattlePhase.Preparation);

        public bool IsBattleActive { get; private set; }

        public void StartBattle(BattleOpponentData monster, float playerHealth)
        {
            if (monster == null) throw new ArgumentNullException(nameof(monster));

            CurrentMonster.Value = monster;
            PlayerMaxHealth.Value = Mathf.Max(0, playerHealth);
            PlayerHealth.Value = PlayerMaxHealth.Value;

            var monsterHealth = monster.Health;
            MonsterMaxHealth.Value = Mathf.Max(0, monsterHealth);
            MonsterHealth.Value = MonsterMaxHealth.Value;

            Phase.Value = BattlePhase.Preparation;
            IsBattleActive = true;
        }

        public void ApplyExchange(float playerDamage, float monsterDamage)
        {
            if (!IsBattleActive)
                return;

            MonsterHealth.Value = Mathf.Max(0, MonsterHealth.Value - Mathf.Max(0, playerDamage));
            PlayerHealth.Value = Mathf.Max(0, PlayerHealth.Value - Mathf.Max(0, monsterDamage));
        }

        public bool HasWinner(out bool playerWon)
        {
            playerWon = false;

            if (!IsBattleActive)
                return false;

            if (MonsterHealth.Value <= 0)
            {
                playerWon = true;
                IsBattleActive = false;
                return true;
            }

            if (PlayerHealth.Value <= 0)
            {
                playerWon = false;
                IsBattleActive = false;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            IsBattleActive = false;
            PlayerHealth.Value = 0;
            PlayerMaxHealth.Value = 0;
            MonsterHealth.Value = 0;
            MonsterMaxHealth.Value = 0;
            CurrentMonster.Value = null;
            Phase.Value = BattlePhase.Preparation;
        }
    }
}
