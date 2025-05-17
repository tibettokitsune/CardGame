using System;
using UnityEngine;

namespace Game.Scripts.Gameplay.Lobby.Deck
{
    [Serializable]
    public class CardIconLayer
    {
        public Sprite sprite;
        public Vector3 position;
        public Vector3 rotation;
        public Vector2 size;
    }

    [CreateAssetMenu(fileName = "MultipleLayerPreset", menuName = "Game/Deck/MultipleLayerPreset", order = 0)]
    public class MultipleLayerPreset : ScriptableObject
    {
        public CardIconLayer[] layers;
    }
}