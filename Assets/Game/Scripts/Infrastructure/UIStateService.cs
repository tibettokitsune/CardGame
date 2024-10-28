using System;
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

    public interface IUIStateService
    {
        UniTask Open(UIState state);
        UniTask Close(UIState state);
    }

    public class UIStateService : IUIStateService, IInitializable
    {
        public UniTask Open(UIState state)
        {
            return UniTask.CompletedTask;
        }

        public UniTask Close(UIState state)
        {
            return UniTask.CompletedTask;
        }

        public async void Initialize()
        {
            var configs = await Resources.LoadAsync<UIStateConfig>("Configs");
            
        }
    }

    [CreateAssetMenu(menuName = "Config/UIState")]
    public class UIStateConfig : ScriptableObject
    {
        public GameObject prefab;
    }
    
    [CreateAssetMenu(menuName = "Config/UIState")]
    public class UIConfig : ScriptableObject
    {
        
    }
}