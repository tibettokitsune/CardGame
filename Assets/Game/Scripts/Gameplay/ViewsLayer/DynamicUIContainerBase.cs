using UnityEngine;

namespace Game.Scripts.Gameplay.ViewsLayer
{
    public abstract class DynamicUIContainerBase : MonoBehaviour
    {
        public abstract void Initialize();
        public abstract void Clear();
    }
}