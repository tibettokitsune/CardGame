using System;
using System.Linq;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.Lobby.Player;
using Game.Scripts.Gameplay.PresentersLayer.Deck;
using ModestTree;
using UniRx;

namespace Game.Scripts.Gameplay.PresentersLayer.Player
{
    public class PlayerPresenter : IPlayerPresenter,
        IFillStartHandUseCase,
        IEquipCardUseCase,
        IDisposable
    {
        public ReactiveCollection<CardEntity> PlayerHand { get; } = new();
        public ReactiveCollection<EquipmentCardEntity> PlayerEquipment { get; } = new();
        public ReactiveCollection<StatEntity> PlayerStats { get; } = new();

        private readonly IDeckPresenter _deckPresenter;
        private readonly IPlayerDataProvider _playerDataProvider;
        private readonly CompositeDisposable _disposables = new();
        private const int StartCardsLimit = 8;

        public PlayerPresenter(IDeckPresenter deckPresenter,
            IPlayerDataProvider playerDataProvider)
        {
            _deckPresenter = deckPresenter;
            _playerDataProvider = playerDataProvider;
            _playerDataProvider.PlayersHand.ObserveAdd().Subscribe(OnHandChange).AddTo(_disposables);
            _playerDataProvider.PlayersHand.ObserveRemove().Subscribe(OnHandChange).AddTo(_disposables);
            _playerDataProvider.PlayersEquipment.ObserveAdd().Subscribe(OnEquipmentChange).AddTo(_disposables);
            _playerDataProvider.PlayersEquipment.ObserveRemove().Subscribe(OnEquipmentChange).AddTo(_disposables);
            _playerDataProvider.PlayersStats.ObserveReplace().Subscribe(OnStatReplace).AddTo(_disposables);
            _playerDataProvider.PlayersStats.ObserveAdd().Subscribe(OnStatChanged);
            _playerDataProvider.PlayersStats.ObserveReplace().Subscribe(OnStatChanged);
            _playerDataProvider.PlayersStats.ObserveRemove().Subscribe(OnStatRemoved);
            SyncStatEntities();
        }

        private void OnStatChanged(DictionaryAddEvent<PlayerStat, float> evt)
        {
            var existing = PlayerStats.FirstOrDefault(x => x.Stat == evt.Key);
            if (existing != null)
            {
                existing.Value = evt.Value;
            }
            else
            {
                PlayerStats.Add(new StatEntity {Stat = evt.Key, Value = evt.Value});
            }
        }

        private void OnStatChanged(DictionaryReplaceEvent<PlayerStat, float> evt)
        {
            var existing = PlayerStats.FirstOrDefault(x => x.Stat == evt.Key);
            if (existing != null)
            {
                existing.Value = evt.NewValue;
            }
        }

        private void OnStatRemoved(DictionaryRemoveEvent<PlayerStat, float> evt)
        {
            var stat = PlayerStats.FirstOrDefault(x => x.Stat == evt.Key);
            if (stat != null)
            {
                PlayerStats.Remove(stat);
            }
        }

        private void SyncStatEntities()
        {
            PlayerStats.Clear();
            foreach (var kvp in _playerDataProvider.PlayersStats)
            {
                PlayerStats.Add(new StatEntity
                {
                    Stat = kvp.Key,
                    Value = kvp.Value
                });
            }
        }

        private void OnStatReplace(DictionaryReplaceEvent<PlayerStat, float> replaceEvent)
        {
            var index = 0;
            for (var i = 0; i < PlayerStats.Count; i++)
            {
                if (PlayerStats[i].Stat == replaceEvent.Key)
                {
                    index = i;
                    break;
                }
            }

            var target = PlayerStats[index];
            target.Value = replaceEvent.NewValue;
            PlayerStats.RemoveAt(index);
            PlayerStats.Insert(index, target);
        }

        private void OnHandChange(CollectionAddEvent<string> collectionAddEvent)
        {
            PlayerHand.Add(new CardEntity(_deckPresenter.GetCardById(collectionAddEvent.Value)));
        }

        private void OnHandChange(CollectionRemoveEvent<string> collectionRemoveEvent)
        {
            PlayerHand.Remove(PlayerHand.First(x => x.ID == collectionRemoveEvent.Value));
        }

        private void OnEquipmentChange(CollectionAddEvent<string> collectionAddEvent)
        {
            PlayerEquipment.Add(new EquipmentCardEntity(_deckPresenter.GetCardById(collectionAddEvent.Value)));
        }

        private void OnEquipmentChange(CollectionRemoveEvent<string> collectionRemoveEvent)
        {
            PlayerEquipment.Remove(PlayerEquipment.First(x => x.ID == collectionRemoveEvent.Value));
        }


        #region usecases

        async Task<EquipCardResult> IEquipCardUseCase.Execute(string cardId)
        {
            if (_playerDataProvider.IsPlayerCanEquipCard(cardId))
            {
                await _playerDataProvider.EquipCard(cardId);
                return EquipCardResult.Equipped;
            }

            return EquipCardResult.Failed;
        }

        async Task IFillStartHandUseCase.Execute()
        {
            for (var i = 0; i < StartCardsLimit; i++)
            {
                await AddRandomCardByType();
            }
        }

        private async Task AddRandomCardByType()
        {
            var cardId = await _deckPresenter.ClaimRandomCardFromDeck();
            await _playerDataProvider.ClaimCard(cardId);
        }

        #endregion

        public void Dispose()
        {
            PlayerHand?.Dispose();
            PlayerEquipment?.Dispose();
        }
    }
}