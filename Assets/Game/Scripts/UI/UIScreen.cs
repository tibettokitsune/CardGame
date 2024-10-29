using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup cg;

        private void OnValidate()
        {
            if (!cg) cg = GetComponent<CanvasGroup>();
        }

        [Button]
        public void Open()
        {
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }

        [Button]
        public void Close()
        {
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
    }
}