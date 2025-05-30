using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.DataLayer;
using Game.Scripts.Gameplay.DataLayer.Models;
using Game.Scripts.Infrastructure.Configs;
using Game.Scripts.Infrastructure.Configs.Configs;
using UnityEngine;

namespace Game.Scripts.Gameplay.Lobby.Deck
{
    public interface IDeckPresenter
    {
        Task<string> ClaimRandomCardFromDeck(CardType cardType);
        BaseCard GetCardById(string cardId);
    }

    public class DeckPresenter : IDeckPresenter
    {
        private readonly IConfigService<BaseConfig> _configService;

        private readonly Queue<BaseCard> _doorsQueue = new();
        private readonly Queue<BaseCard> _treasuresQueue = new();

        private Dictionary<string, BaseCard> _cardsCollection = new();
        public DeckPresenter(IConfigService<BaseConfig> configService)
        {
            _configService = configService;
            FillDeck();
        }

        private void FillDeck()
        {
            FillDoors();
            FillTreasures();
        }

        private void FillDoors()
        {
            var cardConfig = _configService.Get<CardDataConfig>("card_1");
            var mainLayerConfig = _configService.Get<CardLayerDataConfig>(cardConfig.MainLayerId);
            var backgroundLayerConfig = _configService.Get<CardLayerDataConfig>(cardConfig.BackgroundLayerId);
            
            FilterAndAddCard(new BaseCard(cardConfig, 
                mainLayerConfig,
                backgroundLayerConfig,
                CardType.Door));
            FilterAndAddCard(new BaseCard(cardConfig, 
                mainLayerConfig,
                backgroundLayerConfig,
                CardType.Door));
            FilterAndAddCard(new BaseCard(cardConfig, 
                mainLayerConfig,
                backgroundLayerConfig,
                CardType.Door));
            FilterAndAddCard(new BaseCard(cardConfig, 
                mainLayerConfig,
                backgroundLayerConfig,
                CardType.Door));
        }
        
        private void FillTreasures()
        {
            var cardConfig = _configService.Get<CardDataConfig>("card_2");
            var mainLayerConfig = _configService.Get<CardLayerDataConfig>(cardConfig.MainLayerId);
            var backgroundLayerConfig = _configService.Get<CardLayerDataConfig>(cardConfig.BackgroundLayerId);
            
            FilterAndAddCard(new BaseCard(cardConfig, 
                mainLayerConfig,
                backgroundLayerConfig,
                CardType.Treasure));
            FilterAndAddCard(new BaseCard(cardConfig, 
                mainLayerConfig,
                backgroundLayerConfig,
                CardType.Treasure));
            FilterAndAddCard(new BaseCard(cardConfig, 
                mainLayerConfig,
                backgroundLayerConfig,
                CardType.Treasure));
            FilterAndAddCard(new BaseCard(cardConfig, 
                mainLayerConfig,
                backgroundLayerConfig,
                CardType.Treasure));
        }

        private void FilterAndAddCard(BaseCard card)
        {
            switch (card.CardType)
            {
                case CardType.Door:
                    _doorsQueue.Enqueue(card);
                    break;
                case CardType.Treasure:
                    _treasuresQueue.Enqueue(card);
                    break;
                default:
                    throw new Exception("Unknown card type");
            }
            
            _cardsCollection.TryAdd(card.ID, card);
        }

        private string FilterAndReturnCard(CardType cardType)
        {
            return cardType switch
            {
                CardType.Door => _doorsQueue.Dequeue().ID,
                CardType.Treasure => _treasuresQueue.Dequeue().ID,
                _ => throw new Exception("Unknown card type")
            };
        }

        public async Task<string> ClaimRandomCardFromDeck(CardType cardType)
        {
            Debug.Log($"ClaimRandomCardFromDeck {cardType.ToString()}");

            return FilterAndReturnCard(cardType);
        }

        public BaseCard GetCardById(string cardId) => _cardsCollection[cardId];
    }
}