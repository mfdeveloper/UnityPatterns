using System;
using UnityEngine;
using UnityPatterns.Singleton;
using UnityPatterns.ScriptableObjects;

namespace UnityPatterns.Injection
{
    /// <remarks>
    /// TODO: <b>[Improvement]</b> Call <see cref="Factory.FactoryComponent"/> methods from this <see cref="MonoBehaviour"/> as a replacement
    /// </remarks>
    public class Injector : SingletonPersistent<Injector>
    {
        [SerializeField]
        private InjectorSettings injectorSettings;

        public InjectorSettings InjectorSettings => injectorSettings;

        public T Resolve<T>(bool includeInactive = false)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(bool includeInactive = false) => Resolve<T>(includeInactive);
    }
}
