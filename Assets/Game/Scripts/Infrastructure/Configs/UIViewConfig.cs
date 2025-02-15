using System;
using UnityEngine;

namespace Game.Scripts.Infrastructure.Configs
{
    [Serializable]
    [CreateAssetMenu(fileName = "UIConfig", menuName = "Configs/UIConfig", order = 1)]
    public class UIViewConfig : ScriptableObject
    {
        public string Id;
        public string PrefabPath;
        public UILayer Layer;
    }
}