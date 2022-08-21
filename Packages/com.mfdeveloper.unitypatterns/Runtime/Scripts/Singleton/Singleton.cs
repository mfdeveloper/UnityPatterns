using UnityEngine;

namespace UnityPatterns.Singleton
{
    /// <summary>
    /// Generic Singleton MonoBehaviour target use with non persistent GameObjects
    /// </summary>
    /// <remarks>
    /// <b>Based in:</b><br/>
    /// <a href="https://gist.github.com/mstevenson/4325117">Generic Singleton classes for Unity</a><br/>
    /// <a href="https://www.youtube.com/watch?v=Ova7l0UB26U">Design Pattern: Singletons in Unity</a>
    /// </remarks>
    /// <typeparam name="T">Class target use as a singleton</typeparam>
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        protected static T _instance;

        public static T Instance 
        { 
            get
            {
                if (_instance == null)
                {

                    _instance = FindObjectOfType<T>(true);

                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                    
                        _instance = obj.AddComponent<T>();
                    }

                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
