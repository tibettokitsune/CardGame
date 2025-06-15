using System.Collections.Generic;
using Game.Scripts.Infrastructure.Configs.Configs;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Scripts.Gameplay.DataLayer.Models
{
    public static class MetaDataKeys
    {
        public const string Equipment = "Equipment";
        public const string Name = "Name";
        public const string Description = "Description";
    }
    
    public class BaseCard : IBaseCard
    {
        private readonly CardDataConfig _config;

        public string ID => _config.Id;
        public string Name => MetaData[MetaDataKeys.Name];
        public string Description => MetaData[MetaDataKeys.Description];
        public CardLayerDataConfig MainLayer { get; }
        public CardLayerDataConfig BackgroundLayer { get; }

        private Dictionary<string, string> MetaData { get; } = new();

        public BaseCard(CardDataConfig config, 
            CardLayerDataConfig mainLayer, 
            CardLayerDataConfig backgroundLayer)
        {
            _config = config;
            MainLayer = mainLayer;
            BackgroundLayer = backgroundLayer;
            MetaData = JsonConvert.DeserializeObject<Dictionary<string, string>>(_config.MetaData);

        }
    }
}