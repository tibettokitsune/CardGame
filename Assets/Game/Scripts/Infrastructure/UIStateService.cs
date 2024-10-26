using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Infrastructure
{
    public enum UIState
    {
        Loading,
        Menu
    }

    internal class UIStateEntity
    {
        public GameObject Root;
    }

    public interface IUIStateService
    {
        UniTask Open(UIState state);
        UniTask Close(UIState state);
    }

    public class UIStateService : IUIStateService
    {
        private Dictionary<UIState, UIStateEntity> _uiStateEntities = new();


        public async UniTask Open(UIState state)
        {
            if (_uiStateEntities.ContainsKey(state))
            {
                _uiStateEntities[state].Root.SetActive(true);
            }
            else
            {
                await Resources.LoadAsync<>()
            }
        }

        public UniTask Close(UIState state)
        {
            if (_uiStateEntities.ContainsKey(state))
            {
                _uiStateEntities[state].Root.SetActive(false);
            }
            else
            {
                
            }
        }
    }

    [CreateAssetMenu(menuName = "Config/UIState")]
    public class UIStateConfig : ScriptableObject
    {
    }
}