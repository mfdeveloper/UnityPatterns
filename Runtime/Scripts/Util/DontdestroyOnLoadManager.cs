using UnityEngine;
using UnityPatterns.Singleton;

namespace UnityPatterns.Util
{
    /// <summary>
    /// A manager to access root objects of the scene that Unity creates automatically
    /// when the <see cref="Object.DontDestroyOnLoad(Object)"/> is called
    /// </summary>
    /// <remarks>
    /// Also, you can access some information from the <b>"DontDestroyOnLoad"</b>scene,
    /// such as: (<seealso cref="UnityEngine.SceneManagement.Scene.isLoaded"/>, <seealso cref="UnityEngine.SceneManagement.Scene.rootCount"/>)
    /// <br/>
    /// <br/>
    /// <b>PS:</b> This is a trick to use this class as "spy"
    /// to get all the roots from DontdestroyOnLoad from the "inside" :)
    /// <br/>
    /// <br/>
    /// <b>Reference:</b>
    /// <a href="https://forum.unity.com/threads/editor-script-how-to-access-objects-under-dontdestroyonload-while-in-play-mode.442014/#post-5187008">How to access objects under DontDestroyOnLoad while in Play mode</a>
    /// </remarks>
    public class DontDestroyOnLoadManager : SingletonPersistent<DontDestroyOnLoadManager>
    {
        protected new bool destroyPreviousInstance = true;

        public bool IsSceneLoaded => gameObject.scene.isLoaded;
        public int RootCount => gameObject.scene.rootCount;
        public GameObject[] RootGameObjects => gameObject.scene.GetRootGameObjects();

        protected override void Awake()
        {
            base.Awake();

            // Force replace the default gameObject name from "New Game Object"
            gameObject.name = GetType().Name;
        }
    }
}
