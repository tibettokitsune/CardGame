using System;
using System.Collections.Generic;
using System.ComponentModel;
using Game.Scripts.Gameplay.DataLayer.Models;
using Game.Scripts.Infrastructure.Configs.Configs;
using Newtonsoft.Json;
using UnityEngine;

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

        public Dictionary<string, int> EquipmentReference;
        public EquipmentCardEntity(IBaseCard card) : base(card)
        {
            var res = Card.MetaData.TryGetValue(MetaDataKeys.EquipmentReference, out var data);
            if (!res)
            {
                EquipmentReference = new Dictionary<string, int>();
                Debug.LogWarning($"EquipmentReference for card {card.ID} not found");
                return;
            }
            EquipmentReference = JsonConvert.DeserializeObject<Dictionary<string, int>>(data);
        }
    }
    
    public class DoorCardEntity : CardEntity
    {
        public DoorCardEntity(IBaseCard card) : base(card)
        {
        }
    }
}