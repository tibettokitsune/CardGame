using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

namespace Game.Scripts.Infrastructure.Configs.Configs
{
    [Serializable]
    public class SublayerData
    {
        public string SpritePath { get; set; }
        public Vector3 Offset { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Rotation { get; set; }
    }

    [Serializable]
    public class CardLayerDataConfig : BaseConfig
    {
        public List<SublayerData> Layers { get; set; }
    }

    [Serializable]
    public class CardDataConfig : BaseConfig
    {
        public string MainLayerId { get; set; }
        public string BackgroundLayerId { get; set; }

        public string MetaData { get; set; }

        public Dictionary<string, string> MetaDataDictionary => _metaDataDictionary ??=
            JsonConvert.DeserializeObject<Dictionary<string, string>>(MetaData);

        private Dictionary<string, string> _metaDataDictionary;
    }
}