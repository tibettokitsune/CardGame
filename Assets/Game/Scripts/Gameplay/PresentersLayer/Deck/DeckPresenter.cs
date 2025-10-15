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

        private readonly Queue<BaseCard> _treasuresCards = new();
        private readonly Queue<BaseCard> _doorsCards = new();

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

                var card = new BaseCard(config);
                _cardsCollection[card.ID] = card;

                switch (card.Kind)
                {
                    case CardKind.Treasure:
                        _treasuresCards.Enqueue(card);
                        break;
                    case CardKind.Door:
                        _doorsCards.Enqueue(card);
                        break;
                    case CardKind.Event:
                        // Event cards are queued elsewhere when such gameplay is added.
                        break;
                    default:
                        Debug.LogWarning($"Unhandled card kind: {card.Kind} for card {card.ID}");
                        break;
                }
            }

            Shuffle(_treasuresCards);
            Shuffle(_doorsCards);
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

        public async Task<string> TakeTreasureCard()
        {
            return DequeueCard(_treasuresCards, "treasure");
        }

        public async Task<string> TakeDoorCard()
        {
            return DequeueCard(_doorsCards, "door");
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
