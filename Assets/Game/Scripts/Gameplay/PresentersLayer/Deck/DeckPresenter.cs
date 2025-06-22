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
        }
        
        private void FillCards()
        {
            for (var i = 0; i < 10; i++)
            {
                var cardConfig = _configService.Get<CardDataConfig>($"card_{(i % 4) + 1}");
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