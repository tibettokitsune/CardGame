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

        private readonly Queue<BaseCard> _cards = new();

        private Dictionary<string, BaseCard> _cardsCollection = new();
        public DeckPresenter(IConfigService configService)
        {
            _configService = configService;
            FillCards();
            Shuffle();
        }

        private void Shuffle()
        {
            Random rng = new Random();
            var list = new List<BaseCard>(_cards);
            _cards.Clear();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }

            foreach (var item in list)
            {
                _cards.Enqueue(item);
            }
        }

        private void FillCards()
        {
            for (var i = 0; i < 3; i++)
            {
                var cardConfig = _configService.Get<CardDataConfig>($"hand{i + 1}");
                FilterAndAddCard(new BaseCard(cardConfig));
            }
            for (var i = 0; i < 5; i++)
            {
                var cardConfig = _configService.Get<CardDataConfig>($"head{i + 1}");
                FilterAndAddCard(new BaseCard(cardConfig));
            }
            for (var i = 0; i < 4; i++)
            {
                var cardConfig = _configService.Get<CardDataConfig>($"chest{i + 1}");
                FilterAndAddCard(new BaseCard(cardConfig));
            }
            for (var i = 0; i < 5; i++)
            {
                var cardConfig = _configService.Get<CardDataConfig>($"boot{i + 1}");
                FilterAndAddCard(new BaseCard(cardConfig));
            }
        }

        private void FilterAndAddCard(BaseCard card)
        {
            _cards.Enqueue(card);
            _cardsCollection.TryAdd(card.ID, card);
        }

        private string FilterAndReturnCard()
        {
            _cards.TryDequeue(out var card);
            return card.ID;
        }

        public async Task<string> ClaimRandomCardFromDeck()
        {
            return FilterAndReturnCard();
        }

        public BaseCard GetCardById(string cardId) => _cardsCollection[cardId];
    }
}