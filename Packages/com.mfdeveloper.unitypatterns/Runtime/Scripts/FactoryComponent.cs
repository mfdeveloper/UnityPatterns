using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityPatterns.Util;

namespace UnityPatterns
{

    /// <summary>
    /// Factory method implementation that's retrieves
    /// a specific instance in the scene
    /// </summary>
    public class FactoryComponent
    {

        protected static Dictionary<Type, object> componentsInstances = new Dictionary<Type, object>();

        public static IReadOnlyDictionary<Type, object> ComponentsInstances => componentsInstances;

        // TODO: [Refactor] Consider move this method to a "Util" scene class 
        public static List<GameObject> GetAllRootObjects(bool onlyDontDestroy = false)
        {
            // TODO: [Refactor] Try to remove this circular reference of "DontDestroyOnLoadManager" here
            var rootGameObjects = new List<GameObject>(DontDestroyOnLoadManager.Instance.RootGameObjects);

            if (!onlyDontDestroy)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (scene.rootCount > 0)
                    {
                        rootGameObjects.AddRange(scene.GetRootGameObjects());
                    }
                }
            }

            return rootGameObjects;
        }

        /// <summary>
        /// Similar to <see cref="Get{T}(bool)"/>, use this if you wish apply
        /// <seealso cref="System.Linq"/> filters before retrieve the component
        /// </summary>
        /// <example>
        /// <code>
        /// using System.Linq;
        /// using System.Collections.Generic;
        /// using UnityEngine;
        /// using UnityPatterns;
        ///
        /// // Create an interface
        /// public interface IMyComponent
        /// {
        ///
        /// }
        ///
        /// // Create a script that implements an interface
        /// public class MyScriptComponent : MonoBehaviour, IMyComponent
        /// {
        ///
        /// }
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///
        ///     // Get the component that implements a specific interface
        ///     void Start()
        ///     {
        ///         IMyComponent myComponent = FactoryComponent.GetAll<IMyComponent>()
        ///                                                    .Where(comp => comp.name == "MyManager")
        ///                                                    .FirstOrDefault();
        ///
        ///         // Check if the result is the filtered gameObject
        ///         Debug.Log(myComponent.gameObject.name); // Output: MyManager
        ///     }
        /// }
        /// </code>
        /// </example>
        [SuppressMessage("Type Safety", "UNT0014:Invalid type for call to GetComponent", Justification = "<Ignored>")]
        public static IEnumerable<T> GetAll<T>(bool includeInactive = false)
        {
            var genericsType = typeof(T);

            if (!genericsType.IsInterface && Debug.isDebugBuild)
            {
                string warnMsg = $"The generic type {genericsType.Name} isn't an interface. Prefer use GetComponent() or GetComponentInChildren() to improve " +
                    "performance to lookup by script class.";
                Debug.LogWarning(warnMsg);
            }

            var gameObjects = GetAllRootObjects();

            IEnumerable<T> result = gameObjects.Select(gameObj =>
            {
                return gameObj.GetComponentInChildren<T>(includeInactive);
            })
            .Where(comp => comp != null);

            return result;
        }

        public static List<T> GetList<T>(bool includeInactive = false) => GetAll<T>(includeInactive).ToList();

        /// <summary>
        /// Find a gameObject in the scene with an attached script
        /// that implements an <b>C#</b> interface
        /// </summary>
        /// <remarks>
        /// The component instances are stored in a "cache" to improve lookup performance,
        /// and don't perform a new search in the scene for each call of <see cref="Get{T}(bool)"/> 
        /// </remarks>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using UnityPatterns;
        ///
        /// // Create an interface
        /// public interface IMyComponent
        /// {
        ///
        /// }
        ///
        /// // Create a script that implements an interface
        /// public class MyScriptComponent : MonoBehaviour, IMyComponent
        /// {
        ///
        /// }
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///
        ///     // Get the component that implements a specific interface
        ///     void Start()
        ///     {
        ///         IMyComponent myComponent = FactoryComponent.Get<IMyComponent>();
        ///
        ///         // Check if the result is a instance of the script
        ///         Debug.Log(myComponent.GetType().Name); // Output: MyScriptComponent
        ///     }
        /// }
        /// </code>
        /// </example>
        public static T Get<T>(bool includeInactive = false)
        {
            var genericsType = typeof(T);

            var instance = componentsInstances.FirstOrDefault(instance => genericsType.IsInstanceOfType(instance.Value));
            if (instance.Value != null)
            {
                return (T) instance.Value;
            }

            if (!componentsInstances.ContainsKey(genericsType))
            {
                componentsInstances.Add(genericsType, GetAll<T>(includeInactive).FirstOrDefault());
            }

            return (T) componentsInstances[genericsType];
        }

        public static bool ContainsInstance(object instance) => componentsInstances.ContainsValue(instance);
    }
}
