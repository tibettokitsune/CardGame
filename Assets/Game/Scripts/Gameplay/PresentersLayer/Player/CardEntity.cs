using System;
using System.Collections.Generic;
using Game.Scripts.Gameplay.DataLayer.Models;
using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.Gameplay.PresentersLayer.Player
{
    public class CardEntity
    {
        protected readonly IBaseCard Card;

        public string ID => Card.ID;
        public string Name => Card.Name;
        public string Description => Card.Description;
        public string MainLayer => Card.MainLayer;
        public string BackgroundLayer => Card.BackgroundLayer;

        public CardEntity(IBaseCard card)
        {
            Card = card;
        }
    }
    
    public class EquipmentCardEntity : CardEntity
    {
        public string EquipmentDescription => Card.MetaData[MetaDataKeys.Stats]
            .Replace("type=", "", StringComparison.OrdinalIgnoreCase)
            .Replace("value=", "+", StringComparison.OrdinalIgnoreCase)
            .Replace(";", "\n");
        public EquipmentCardEntity(IBaseCard card) : base(card)
        {
        }
    }
}