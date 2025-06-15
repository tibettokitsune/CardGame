using System;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.UI;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer
{
    public class CharacterStatsScreen : UIScreen
    {
        [Inject] private IPlayerPresenter _playerPresenter;
        [SerializeField] private PlayerParameterWidget widgetPrefab;

        private void Start()
        {
        }
    }
}