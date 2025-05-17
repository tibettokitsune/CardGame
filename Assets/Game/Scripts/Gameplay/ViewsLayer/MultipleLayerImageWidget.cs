using Game.Scripts.Infrastructure.Configs.Configs;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.Lobby.Deck
{
    public class MultipleLayerImageWidget : MonoBehaviour
    {
        
        public void Setup(CardLayerDataConfig preset)
        {
            foreach (var layer in preset.Layers)
            {
                var image = new GameObject("Layer").AddComponent<Image>();
                image.transform.SetParent(transform);
                image.sprite = Resources.Load<Sprite>(layer.SpritePath);
                image.SetNativeSize();
                image.transform.localPosition = new Vector3(layer.Offset.X, layer.Offset.Y, layer.Offset.Z);
                image.transform.localEulerAngles = new Vector3(layer.Rotation.X, layer.Rotation.Y, layer.Rotation.Z);
                image.transform.localScale = new Vector3(layer.Scale.X, layer.Scale.Y, layer.Scale.Z);
            }
        }

        public void Clear()
        {
            
        }
    }
}