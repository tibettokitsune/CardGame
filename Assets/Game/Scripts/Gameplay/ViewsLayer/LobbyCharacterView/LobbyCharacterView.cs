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
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterCustomizer customizer;
         private void Start()
        {
            _playerPresenter.PlayerEquipment.ObserveAdd()
                .Subscribe(e => OnItemAdded(e.Value)).AddTo(this);
        }

        private void OnItemAdded(EquipmentCardEntity entity)
        {
            animator.SetTrigger(Equipment);
            animator.SetFloat("LobbyState", Random.Range(0, 3));
            foreach (var overrideData in entity.Overrides)
            {
                customizer.EnableItem(overrideData.Item, overrideData.Index);   
            }
        }
    }
}
