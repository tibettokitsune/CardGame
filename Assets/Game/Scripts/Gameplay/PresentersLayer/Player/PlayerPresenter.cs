using System;
using System.Linq;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.DataLayer;
using Game.Scripts.Gameplay.Lobby.Player;
using Game.Scripts.Gameplay.PresentersLayer.Deck;
using UniRx;

namespace Game.Scripts.Gameplay.PresentersLayer.Player
{
    public interface ITakeEventCardUseCase
    {
        Task Execute();
    }
    
    public class PlayerPresenter : IPlayerPresenter,
        IFillStartHandUseCase,
        IEquipCardUseCase,
        ITakeEventCardUseCase,
        IDisposable
    {
        public ReactiveCollection<CardEntity> PlayerHand { get; } = new();
        public ReactiveCollection<EquipmentCardEntity> PlayerEquipment { get; } = new();
        public ReactiveDictionary<string, StatEntity> PlayerStats { get; } = new();
        
        public ReactiveProperty<DoorCardEntity> CurrentDoor { get; } = new();

        private readonly IDeckPresenter _deckPresenter;
        private readonly IPlayerDataProvider _playerDataProvider;
        private readonly CompositeDisposable _disposables = new();
        private const int StartCardsLimit = 8;
        private readonly CompositeDisposable _statsSyncDisposables = new();

        public PlayerPresenter(IDeckPresenter deckPresenter,
            ILobbyDataProvider lobbyDataProvider,
            IPlayerDataProvider playerDataProvider)
        {
            _deckPresenter = deckPresenter;
            _playerDataProvider = playerDataProvider;
            _playerDataProvider.PlayersHand.ObserveAdd().Subscribe(OnHandChange).AddTo(_disposables);
            _playerDataProvider.PlayersHand.ObserveRemove().Subscribe(OnHandChange).AddTo(_disposables);
            _playerDataProvider.PlayersEquipment.ObserveAdd().Subscribe(OnEquipmentChange).AddTo(_disposables);
            _playerDataProvider.PlayersEquipment.ObserveRemove().Subscribe(OnEquipmentChange).AddTo(_disposables);
            SyncPlayerStats(_playerDataProvider.PlayersStats, PlayerStats,
                keyConverter: stat => stat.ToString(),
                entityFactory: (stat, value) => new StatEntity
                {
                    Stat = stat,
                    Value = value,
                });
        }

        private void SyncPlayerStats(
            ReactiveDictionary<PlayerStat, float> playersStats,
            ReactiveDictionary<string, StatEntity> playerStats,
            Func<PlayerStat, string> keyConverter,
            Func<PlayerStat, float, StatEntity> entityFactory)
        {
            _statsSyncDisposables.Clear();

            foreach (var kvp in playersStats)
            {
                var key = keyConverter(kvp.Key);
                playerStats[key] = entityFactory(kvp.Key, kvp.Value);
            }

            playersStats
                .ObserveAdd()
                .Subscribe(e =>
                {
                    var key = keyConverter(e.Key);
                    playerStats[key] = entityFactory(e.Key, e.Value);
                })
                .AddTo(_statsSyncDisposables);

            playersStats
                .ObserveReplace()
                .Subscribe(e =>
                {
                    var key = keyConverter(e.Key);
                    playerStats[key] = entityFactory(e.Key, e.NewValue);
                })
                .AddTo(_statsSyncDisposables);

            playersStats
                .ObserveRemove()
                .Subscribe(e =>
                {
                    var key = keyConverter(e.Key);
                    playerStats.Remove(key);
                })
                .AddTo(_statsSyncDisposables);

            playersStats
                .ObserveReset()
                .Subscribe(_ =>
                {
                    playerStats.Clear();
                    foreach (var kvp in playersStats)
                    {
                        var key = keyConverter(kvp.Key);
                        playerStats[key] = entityFactory(kvp.Key, kvp.Value);
                    }
                })
                .AddTo(_statsSyncDisposables);
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

        private async void TakeEventCard()
        {
            var card =  await _deckPresenter.TakeDoorCard();
            CurrentDoor.Value = new DoorCardEntity(_deckPresenter.GetCardById(card));
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
        
        async Task ITakeEventCardUseCase.Execute()
        {
            var card =  await _deckPresenter.TakeDoorCard();
            CurrentDoor.Value = new DoorCardEntity(_deckPresenter.GetCardById(card));
        }

        private async Task AddRandomCardByType()
        {
            var cardId = await _deckPresenter.TakeTreasureCard();
            await _playerDataProvider.ClaimCard(cardId);
        }

        #endregion

        public void Dispose()
        {
            PlayerHand?.Dispose();
            PlayerEquipment?.Dispose();
            _statsSyncDisposables?.Dispose();
        }
    }
}