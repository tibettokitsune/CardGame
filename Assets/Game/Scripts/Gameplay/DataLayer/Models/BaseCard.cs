using System.Collections.Generic;
using Game.Scripts.Infrastructure.Configs.Configs;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Scripts.Gameplay.DataLayer.Models
{
    public static class MetaDataKeys
    {
        public const string Equipment = "Equipment";
        public const string Stats = "Stats";
        public const string Name = "Name";
        public const string Description = "Description";
        public const string EquipmentReference = "EquipmentReference";
    }
    
    public class BaseCard : IBaseCard
    {
        private readonly CardDataConfig _config;

        public string ID => _config.Id;
        public string Name => MetaData[MetaDataKeys.Name];
        public string Description => MetaData[MetaDataKeys.Description];
        public string MainLayer { get; }
        public string BackgroundLayer { get; }

        public Dictionary<string, string> MetaData { get; }

        public BaseCard(CardDataConfig config)
        {
            _config = config;
            MainLayer = config.MainLayerId;
            BackgroundLayer = config.BackgroundLayerId;
            MetaData = JsonConvert.DeserializeObject<Dictionary<string, string>>(_config.MetaData);
        }
    }
}