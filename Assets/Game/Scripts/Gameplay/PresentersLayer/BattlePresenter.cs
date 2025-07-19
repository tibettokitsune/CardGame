using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Game.Scripts.Gameplay.PresentersLayer
{
    public class BattlePresenter : MonoBehaviour
    {
        [SerializeField] private PlayableDirector enterGameplayDirector;
        [SerializeField] private PlayableDirector fightDirector;
        [SerializeField] private PlayableDirector loseDirector;
        [SerializeField] private PlayableDirector winDirector;

        private void Start()
        {
            enterGameplayDirector.Play();
        }
    }
}