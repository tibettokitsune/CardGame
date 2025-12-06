using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityHFSM;

namespace Game.Scripts.Gameplay.PresentersLayer.GameStates
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EffectOrderAttribute : Attribute
    {
        public int Order { get; }

        public EffectOrderAttribute(int order)
        {
            Order = order;
        }
    }

    public interface IStateEnterEffect<TState>
    {
        Task OnEnterAsync();
    }

    public interface IStateExitEffect<TState>
    {
        Task OnExitAsync();
    }

    public interface IStateExitRequestEffect<TState>
    {
        Task OnExitRequestAsync();
    }

    public interface IPrioritizedEffect
    {
        int Order { get; }
    }

    public static class EffectOrdering
    {
        public static IReadOnlyList<T> OrderEffects<T>(IEnumerable<T> effects)
        {
            return effects
                .OrderBy(GetOrder)
                .ToList();
        }

        private static int GetOrder<T>(T effect)
        {
            if (effect is IPrioritizedEffect prioritized)
                return prioritized.Order;

            var attr = effect?.GetType().GetCustomAttribute<EffectOrderAttribute>();
            return attr?.Order ?? 0;
        }
    }

    /// <summary>
    /// Thin HFSM state that delegates all side effects to injected effect handlers.
    /// </summary>
    /// <typeparam name="TState">Type marker of the state.</typeparam>
    public abstract class EffectDrivenState<TState> : StateBase
    {
        private readonly IReadOnlyList<IStateEnterEffect<TState>> _enterEffects;
        private readonly IReadOnlyList<IStateExitEffect<TState>> _exitEffects;
        private readonly IReadOnlyList<IStateExitRequestEffect<TState>> _exitRequestEffects;
        private readonly bool _requiresExitConfirmation;
        private bool _exitInProgress;

        protected EffectDrivenState(
            IEnumerable<IStateEnterEffect<TState>> enterEffects,
            IEnumerable<IStateExitEffect<TState>> exitEffects,
            IEnumerable<IStateExitRequestEffect<TState>> exitRequestEffects,
            bool needsExitTime,
            bool isGhostState) : base(needsExitTime, isGhostState)
        {
            _enterEffects = EffectOrdering.OrderEffects(enterEffects);
            _exitEffects = EffectOrdering.OrderEffects(exitEffects);
            _exitRequestEffects = EffectOrdering.OrderEffects(exitRequestEffects);
            _requiresExitConfirmation = needsExitTime;
        }

        public override async void OnEnter()
        {
            Debug.Log($"{typeof(TState).Name} Enter");
            await RunEnterEffectsAsync();
        }

        public override async void OnExit()
        {
            Debug.Log($"{typeof(TState).Name} Exit");
            await RunExitEffectsAsync();
            _exitInProgress = false;
        }

        public override void OnExitRequest()
        {
            if (_exitInProgress)
                return;

            _exitInProgress = true;
            _ = HandleExitRequestAsync();
        }

        private async Task HandleExitRequestAsync()
        {
            await RunExitRequestEffectsAsync();

            if (_requiresExitConfirmation)
                fsm?.StateCanExit();
        }

        private async Task RunEnterEffectsAsync()
        {
            foreach (var effect in _enterEffects)
                await effect.OnEnterAsync();
        }

        private async Task RunExitEffectsAsync()
        {
            foreach (var effect in _exitEffects)
                await effect.OnExitAsync();
        }

        private async Task RunExitRequestEffectsAsync()
        {
            foreach (var effect in _exitRequestEffects)
                await effect.OnExitRequestAsync();
        }
    }
}