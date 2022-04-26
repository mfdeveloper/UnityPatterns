using UnityEngine;

namespace UnityPatterns
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
        // TODO: [Refactor] Consider use an C# Attribute instead of override fields
        protected bool destroyPreviousInstance = false;

        protected override void Awake()
        {
            if (destroyPreviousInstance && _instance != null)
            {
                Destroy(this);
            }

            base.Awake();

            DontDestroyOnLoad(gameObject);
        }
    }
}
