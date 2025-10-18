using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Scripts.UI;

namespace Game.Scripts.Infrastructure.UI
{
    internal readonly struct UIScreenMetadata
    {
        public UIScreenMetadata(Type screenType, string configId, Type[] contracts)
        {
            ScreenType = screenType;
            ConfigId = configId;
            Contracts = contracts ?? Array.Empty<Type>();
        }

        public Type ScreenType { get; }
        public string ConfigId { get; }
        public Type[] Contracts { get; }

        public bool Supports(Type type)
        {
            if (type == null)
            {
                return false;
            }

            if (type == ScreenType || type.IsAssignableFrom(ScreenType))
            {
                return true;
            }

            for (var i = 0; i < Contracts.Length; i++)
            {
                if (Contracts[i] == type)
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal static class UIScreenMetadataCache
    {
        private static readonly Dictionary<Type, UIScreenMetadata> Cache = new();

        public static UIScreenMetadata For<T>()
        {
            return For(typeof(T));
        }

        public static UIScreenMetadata For(Type screenType)
        {
            if (screenType == null)
                throw new ArgumentNullException(nameof(screenType));

            if (Cache.TryGetValue(screenType, out var metadata))
            {
                return metadata;
            }

            if (typeof(UIScreen).IsAssignableFrom(screenType))
            {
                metadata = BuildMetadata(screenType);
                return metadata;
            }

            metadata = ResolveContractMetadata(screenType);
            Cache[screenType] = metadata;
            return metadata;
        }

        private static string DeriveIdFromType(Type screenType)
        {
            var name = screenType.Name;
            const string suffix = "Screen";

            if (name.EndsWith(suffix, StringComparison.Ordinal))
            {
                name = name[..^suffix.Length];
            }

            return name;
        }

        private static UIScreenMetadata BuildMetadata(Type screenType)
        {
            if (!typeof(UIScreen).IsAssignableFrom(screenType))
            {
                throw new ArgumentException(
                    $"Type {screenType.FullName} must inherit from {nameof(UIScreen)}.", nameof(screenType));
            }

            var attribute = screenType.GetCustomAttribute<UIScreenAttribute>();
            if (attribute == null)
            {
                throw new InvalidOperationException(
                    $"UIScreen {screenType.FullName} is not annotated with {nameof(UIScreenAttribute)}.");
            }

            var configId = !string.IsNullOrWhiteSpace(attribute.ConfigId)
                ? attribute.ConfigId
                : DeriveIdFromType(screenType);

            var contracts = attribute.ContractTypes ?? Array.Empty<Type>();
            var metadata = new UIScreenMetadata(screenType, configId, contracts);

            Cache[screenType] = metadata;
            foreach (var contract in contracts)
            {
                if (contract != null)
                {
                    Cache[contract] = metadata;
                }
            }

            return metadata;
        }

        private static UIScreenMetadata ResolveContractMetadata(Type type)
        {
            foreach (var metadata in Cache.Values)
            {
                if (metadata.Supports(type))
                {
                    return metadata;
                }
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types ?? Array.Empty<Type>();
                }

                foreach (var candidate in types)
                {
                    if (candidate == null || !typeof(UIScreen).IsAssignableFrom(candidate) || candidate.IsAbstract)
                    {
                        continue;
                    }

                    if (!Cache.TryGetValue(candidate, out var metadata))
                    {
                        metadata = BuildMetadata(candidate);
                    }

                    if (metadata.Supports(type))
                    {
                        Cache[type] = metadata;
                        return metadata;
                    }
                }
            }

            throw new InvalidOperationException(
                $"No UIScreen annotated for contract type {type.FullName}.");
        }
    }
}
