using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityPatterns
{

    /// <summary>
    /// Factory method implementation that's retrieves
    /// a specific instance in the scene
    /// </summary>
    public class FactoryComponent
    {

        public static GameObject[] RootGameObjects
        {
            get
            {
                Scene activeScene = SceneManager.GetActiveScene();
                return activeScene.GetRootGameObjects();
            }
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
            var gameObjects = RootGameObjects;

            IEnumerable<T> result = gameObjects.Select(gameObj =>
            {
                return gameObj.GetComponentInChildren<T>(includeInactive);
            })
            .Where(comp => comp != null);

            return result;
        }

        /// <summary>
        /// Find a gameObject in the scene with an attached script
        /// that implements an <b>C#</b> interface
        /// </summary>
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
        public static T Get<T>(bool includeInactive = false) => GetAll<T>(includeInactive).FirstOrDefault();
    }
}
