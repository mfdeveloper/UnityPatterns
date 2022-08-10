using System;

namespace UnityPatterns.Singleton.Attributes
{
    /// <summary>
    /// Configurations and custom parameters that might be passed to singletons
    /// <see cref="Singleton{T}"/> and <seealso cref="SingletonPersistent{T}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class SingletonSettingsAttribute : Attribute
    {
        public bool CopySerializedFields { get; set; }
        public PersistentDestroyOrder DestroyGameObject { get; set; } = PersistentDestroyOrder.PREVIOUS;

        public SingletonSettingsAttribute()
        {
        }
    }
}
