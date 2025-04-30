using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Scripts.Infrastructure.Configs;
using Game.Scripts.Infrastructure.Configs.Configs;
using UnityEngine;

namespace Game.Scripts.Gameplay.Lobby.Deck
{
    public enum CardType
    {
        Door,
        Treasure
    }

    public interface IBaseCard
    {
        string Name { get; }
        string Description { get; }
        public CardLayerDataConfig MainLayer { get; }
        public CardLayerDataConfig BackgroundLayer { get; }
    }

    public class BaseCard : IBaseCard
    {
        private readonly CardDataConfig _config;

        public string Name => _config.Name;
        public string Description => _config.Description;
        public CardLayerDataConfig MainLayer { get; private set; }
        public CardLayerDataConfig BackgroundLayer { get; private set; }
        public CardType CardType { get; }

        public BaseCard(CardDataConfig config, 
            CardLayerDataConfig mainLayer, 
            CardLayerDataConfig backgroundLayer,
            CardType cardType)
        {
            _config = config;
            CardType = cardType;
            MainLayer = mainLayer;
            BackgroundLayer = backgroundLayer;
        }
    }

    public interface IDeckPresenter
    {
        Task<IBaseCard> ClaimRandomCardFromDeck(CardType cardType);
    }

    public class DeckPresenter : IDeckPresenter
    {
        private readonly IConfigService<BaseConfig> _configService;

        private readonly Queue<BaseCard> _doorsQueue = new();
        private readonly Queue<BaseCard> _treasuresQueue = new();

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
        }

        private BaseCard FilterAndReturnCard(CardType cardType)
        {
            return cardType switch
            {
                CardType.Door => _doorsQueue.Dequeue(),
                CardType.Treasure => _treasuresQueue.Dequeue(),
                _ => throw new Exception("Unknown card type")
            };
        }

        public async Task<IBaseCard> ClaimRandomCardFromDeck(CardType cardType)
        {
            Debug.Log($"ClaimRandomCardFromDeck {cardType.ToString()}");

            return FilterAndReturnCard(cardType);
        }
    }
}