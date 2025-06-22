using System;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyCharacterView
{
    public class LobbyCharacterView : MonoBehaviour
    {
        [Inject] private IPlayerPresenter _playerPresenter;
        [SerializeField] private Animator animator;

        private void Start()
        {
            _playerPresenter.PlayerEquipment.ObserveAdd()
                .Subscribe(e => OnItemAdded(e.Value, e.Index)).AddTo(this);
        }

        private void OnItemAdded(object value, object index)
        {
            animator.SetTrigger("Equipment");
        }
    }
}