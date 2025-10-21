using Game.Scripts.Gameplay.PresentersLayer.Player;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyCharacterView
{
    public class LobbyCharacterView : MonoBehaviour
    {
        private static readonly int Equipment = Animator.StringToHash("Equipment");
        [Inject] private IPlayerPresenter _playerPresenter;
        [SerializeField] private CharacterCustomizer customizer;
        private Animator _animator;

        private void Start()
        {
            customizer.CharacterRebuilt += HandleCharacterRebuilt;
            if (customizer.Animator != null)
            {
                _animator = customizer.Animator;
            }
            _playerPresenter.PlayerEquipment.ObserveAdd()
                .Subscribe(e => OnItemAdded(e.Value)).AddTo(this);
            _playerPresenter.PlayerEquipment.ObserveRemove()
                .Subscribe(e => OnItemRemoved(e.Value)).AddTo(this);
        }

        private void OnDestroy()
        {
            customizer.CharacterRebuilt -= HandleCharacterRebuilt;
        }

        private void OnItemAdded(EquipmentCardEntity entity)
        {
            if (_animator != null)
            {
                _animator.SetTrigger(Equipment);
                _animator.SetFloat("LobbyState", Random.Range(0, 3));
            }

            foreach (var overrideData in entity.Overrides)
            {
                customizer.EnableItem(overrideData.Item, overrideData.Index);   
            }
        }

        private void OnItemRemoved(EquipmentCardEntity entity)
        {
            foreach (var overrideData in entity.Overrides)
            {
                customizer.DisableItem(overrideData.Item);
            }
        }

        private void HandleCharacterRebuilt(Animator animatorInstance)
        {
            _animator = animatorInstance;
        }
    }
}
