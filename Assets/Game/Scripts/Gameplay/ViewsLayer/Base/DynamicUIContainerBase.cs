using UnityEngine;

namespace Game.Scripts.Gameplay.ViewsLayer.Base
{
    public abstract class DynamicUIContainerBase : MonoBehaviour
    {
        public abstract void Initialize();
        public abstract void Clear();
    }
}