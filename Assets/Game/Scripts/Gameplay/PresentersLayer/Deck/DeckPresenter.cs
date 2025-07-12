using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.DataLayer.Models;
using Game.Scripts.Infrastructure.Configs;
using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.Gameplay.PresentersLayer.Deck
{
    public class DeckPresenter : IDeckPresenter
    {
        private readonly IConfigService _configService;

        private readonly Queue<BaseCard> _treasuresCards = new();
        private readonly Queue<BaseCard> _doorsCards = new();

        private Dictionary<string, BaseCard> _cardsCollection = new();
        public DeckPresenter(IConfigService configService)
        {
            _configService = configService;
            FillTreasures();
            FillDoors();
            Shuffle(_treasuresCards);
            Shuffle(_doorsCards);
        }

        private void Shuffle(Queue<BaseCard> cardsCollection)
        {
            Random rng = new Random();
            var list = new List<BaseCard>(cardsCollection);
            cardsCollection.Clear();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }

            foreach (var item in list)
            {
                cardsCollection.Enqueue(item);
            }
        }

        private void FillTreasures()
        {
            for (var i = 0; i < 3; i++)
            {
                var cardConfig = _configService.Get<CardDataConfig>($"hand{i + 1}");
                FilterAndAddCard(new BaseCard(cardConfig), _treasuresCards);
            }
            for (var i = 0; i < 5; i++)
            {
                var cardConfig = _configService.Get<CardDataConfig>($"head{i + 1}");
                FilterAndAddCard(new BaseCard(cardConfig), _treasuresCards);
            }
            for (var i = 0; i < 4; i++)
            {
                var cardConfig = _configService.Get<CardDataConfig>($"chest{i + 1}");
                FilterAndAddCard(new BaseCard(cardConfig), _treasuresCards);
            }
            for (var i = 0; i < 5; i++)
            {
                var cardConfig = _configService.Get<CardDataConfig>($"boot{i + 1}");
                FilterAndAddCard(new BaseCard(cardConfig), _treasuresCards);
            }
        }

        private void FillDoors()
        {
            for (var i = 0; i < 5; i++)
            {
                var cardConfig = _configService.Get<CardDataConfig>($"npc{i + 1}");
                FilterAndAddCard(new BaseCard(cardConfig), _doorsCards);
            }
        }

        private void FilterAndAddCard(BaseCard card, Queue<BaseCard> cardsCollection)
        {
            cardsCollection.Enqueue(card);
            _cardsCollection.TryAdd(card.ID, card);
        }

        private string FilterAndReturnTreasure()
        {
            _treasuresCards.TryDequeue(out var card);
            return card.ID;
        }

        public async Task<string> TakeTreasureCard()
        {
            return FilterAndReturnTreasure();
        }

        public async Task<string> TakeDoorCard()
        {
            _doorsCards.TryDequeue(out var card);
            return card.ID;
        }

        public BaseCard GetCardById(string cardId) => _cardsCollection[cardId];
    }
}