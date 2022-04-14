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

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }
    }
}
