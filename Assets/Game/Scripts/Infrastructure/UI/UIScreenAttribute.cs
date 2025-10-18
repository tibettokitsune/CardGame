using System;

namespace Game.Scripts.Infrastructure.UI
{
    /// <summary>
    /// Marks UIScreen implementations with the config identifier that should be used to load metadata.
    /// Allows the UIService to resolve screens without relying on string literals at call sites.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class UIScreenAttribute : Attribute
    {
        public UIScreenAttribute(string configId = null)
        {
            ConfigId = configId;
        }

        public string ConfigId { get; }

        /// <summary>
        /// Optional contracts that this screen implements. Allows layers to interact with screens via shared interfaces.
        /// </summary>
        public Type[] ContractTypes { get; set; } = Array.Empty<Type>();
    }
}
