using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.DataLayer.Models;
using Game.Scripts.Infrastructure.Configs;
using Game.Scripts.Infrastructure.Configs.Configs;
using UnityEngine;
using Random = System.Random;

namespace Game.Scripts.Gameplay.PresentersLayer.Deck
{
    public class DeckPresenter : IDeckPresenter
    {
        private readonly IConfigService _configService;

        private readonly Queue<BaseCard> _equipmentCards = new();
        private readonly Queue<BaseCard> _doorCards = new();

        private readonly Dictionary<string, BaseCard> _cardsCollection = new();

        public DeckPresenter(IConfigService configService)
        {
            _configService = configService;
            InitializeDecks();
        }

        private void InitializeDecks()
        {
            foreach (var config in _configService.GetAll<CardDataConfig>() ?? Enumerable.Empty<CardDataConfig>())
            {
                if (config == null)
                    continue;

                var card = CreateCard(config);
                _cardsCollection[card.ID] = card;

                EnqueueCard(card);
            }

            Shuffle(_equipmentCards);
            Shuffle(_doorCards);
        }

        private BaseCard CreateCard(CardDataConfig config)
        {
            var typeId = CardTypeUtils.Normalize(BaseCard.ResolveTypeId(config));

            if (CardTypeUtils.IsEquipment(typeId) && config is TreasureCardConfig treasureConfig)
                return new EquipmentCard(treasureConfig);

            if (CardTypeUtils.IsEvent(typeId) && config is EventCardConfig eventConfig)
                return new EventCard(eventConfig);

            if (CardTypeUtils.IsMonster(typeId) && config is MonsterCardConfig monsterCardConfig)
                return new MonsterCard(monsterCardConfig);

            if (config is CardWithStatModifiersConfig statsConfig)
                return new CardWithStatModifiers(statsConfig);

            return new BaseCard(config);
        }

        private void EnqueueCard(BaseCard card)
        {
            var typeId = CardTypeUtils.Normalize(card.TypeId);

            if (CardTypeUtils.IsEquipment(typeId))
            {
                _equipmentCards.Enqueue(card);
                return;
            }

            if (CardTypeUtils.IsDoor(typeId) || CardTypeUtils.IsMonster(typeId) || CardTypeUtils.IsEvent(typeId))
            {
                _doorCards.Enqueue(card);
                return;
            }

            switch (card.Kind)
            {
                case CardKind.Treasure:
                    _equipmentCards.Enqueue(card);
                    break;
                case CardKind.Door:
                case CardKind.Event:
                case CardKind.Monster:
                    _doorCards.Enqueue(card);
                    break;
                default:
                    Debug.LogWarning($"Unhandled card kind: {card.Kind} for card {card.ID}");
                    break;
            }
        }

        private void Shuffle(Queue<BaseCard> cardsCollection)
        {
            var rng = new Random();
            var list = new List<BaseCard>(cardsCollection);
            cardsCollection.Clear();

            for (var i = list.Count - 1; i > 0; i--)
            {
                var swapIndex = rng.Next(i + 1);
                (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
            }

            foreach (var item in list)
                cardsCollection.Enqueue(item);
        }

        public async Task<string> TakeEquipmentCard()
        {
            return DequeueCard(_equipmentCards, "equipment");
        }

        public async Task<string> TakeDoorCard()
        {
            return DequeueCard(_doorCards, "door");
        }

        private string DequeueCard(Queue<BaseCard> source, string deckName)
        {
            if (source.TryDequeue(out var card))
                return card.ID;

            Debug.LogWarning($"Deck '{deckName}' is empty.");
            return string.Empty;
        }

        public BaseCard GetCardById(string cardId)
        {
            if (_cardsCollection.TryGetValue(cardId, out var card))
                return card;

            throw new KeyNotFoundException($"Card with id '{cardId}' was not found.");
        }
    }
}
