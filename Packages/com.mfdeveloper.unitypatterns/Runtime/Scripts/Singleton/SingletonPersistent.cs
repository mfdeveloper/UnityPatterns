using System;
using UnityEngine;
using UnityPatterns.Singleton.Attributes;

namespace UnityPatterns.Singleton
{
    /// <summary>
    /// Generic Singleton MonoBehaviour persistent among scenes
    /// </summary>
    /// <remarks>
    /// <b>Based in:</b><br/>
    /// <a href="https://gist.github.com/mstevenson/4325117">Generic Singleton classes for Unity</a><br/>
    /// <a href="https://www.youtube.com/watch?v=Ova7l0UB26U">Design Pattern: Singletons in Unity</a>
    /// </remarks>
    /// <typeparam name="T">Class to use as a singleton</typeparam>
    public class SingletonPersistent<T> : Singleton<T> where T : Component
    {

        public SingletonSettingsAttribute Settings {
            get
            {
                return (SingletonSettingsAttribute) Attribute.GetCustomAttribute(
                    GetType(), 
                    typeof(SingletonSettingsAttribute)
                );
            }
        }

        protected override void Awake()
        {
            if (_instance != null && _instance != this)
            {
                if (Settings?.CopySerializedFields == true)
                {
                    // TODO: [Feature] Implement copy any [SerializeField] that isn't a gameObject scene reference
                }

                if (Settings?.DestroyGameObject == PersistentDestroyOrder.NEXT)
                {
                    // TODO: [Feature] Always try to identify the [SerializeField] gameObject scene references
                    // when keep the previous instance and remove the current/next
                    Destroy(gameObject);
                    Destroy(this);
                } else
                {
                    Destroy(_instance.gameObject);
                    Destroy(_instance);
                }

                _instance = null;
            }

            base.Awake();

            DontDestroyOnLoad(gameObject);
        }
    }
}
