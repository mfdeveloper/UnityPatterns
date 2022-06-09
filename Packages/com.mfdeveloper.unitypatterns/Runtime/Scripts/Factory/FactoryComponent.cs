using System;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityPatterns.Util;

namespace UnityPatterns.Factory
{

    /// <summary>
    /// Factory method implementation that's retrieves
    /// a specific instance in the scene
    /// </summary>
    public class FactoryComponent
    {
        // TODO: Add a FactoryComponent.Cleanup() method to remove instances that doesn't exists in the scene anymore
        protected static Dictionary<Type, object> componentsInstances = new Dictionary<Type, object>();

        public static IReadOnlyDictionary<Type, object> ComponentsInstances => componentsInstances;

        /// <summary>
        /// Resources folder that are the .asset files to load
        /// <see cref="ScriptableObject"/> singletons
        /// </summary>
        public static string ResourcesAssetsFolder { get; set; } = "ScriptableObjects";

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
        ///
        ///         // Optionally, you can get a ScriptableObject from an interface, as well
        ///         // By default, looking for a .asset stored in: "Resources/ScriptableObjects"
        ///         
        ///         var myScriptable = FactoryComponent.Get<IMyScriptable>();
        ///         myScriptable
        ///     }
        /// }
        /// </code>
        /// </example>
        public static T Get<T>(bool includeInactive = false)
        {
            var genericsType = typeof(T);

            var instancePair = componentsInstances.FirstOrDefault(instance => genericsType.IsInstanceOfType(instance.Value));

            if (instancePair.Value != null)
            {
                return (T) instancePair.Value;
            }

            if (!componentsInstances.ContainsKey(genericsType))
            {
                var instance = GetAll<T>(includeInactive).FirstOrDefault();

                // TODO: [Refactor] Consider move this verification to the "GetAll<>()" method
                instance = FetchScriptableObject(genericsType, instance);

                if (instance != null)
                {
                    VerifyFactoryCleaner(genericsType, instance);

                    componentsInstances.Add(genericsType, instance);
                }

            }

            var resultInstance = componentsInstances[genericsType];
            return (T) resultInstance;            
        }

        public static bool ContainsInstance(object instance) => componentsInstances.ContainsValue(instance);

        public static void Cleanup(object instance = null)
        {
            if (instance is null)
            {
                componentsInstances.Clear();
            }
            else
            {
                var component = componentsInstances.FirstOrDefault(instanceReference =>
                {
                    if (instance is Component instanceComp && instanceReference.Value is Component component)
                    {
                        return instanceComp.gameObject == component.gameObject;
                    }
                    else
                    {
                        return instance == instanceReference.Value;
                    }

                });

                if (component.Value != null)
                {
                    componentsInstances.Remove(component.Key);
                }
            }
        }

        protected static T FetchScriptableObject<T>(Type genericsType, T instance)
        {
            if (instance is null)
            {
                if (genericsType.IsInterface && genericsType.Name.StartsWith("I", false, CultureInfo.CurrentCulture))
                {
                    // Looking for a ScriptableObject .asset
                    // Follow the convention for interfaces, that needs to starts with "I" and the ScriptableObject name
                    // should be the same without the "I" (e.g interface "IMyScriptable" and class should be "MyScriptable")
                    // IAudioManager => FMODAudioManager
                    string path = genericsType.Name;
                    
                    path = path.Substring(1);
                    instance = (T)(object)Resources.Load(path, genericsType);

                    if (instance is null)
                    {
                        var allAssets = Resources.LoadAll(ResourcesAssetsFolder, genericsType);
                        instance = (T)(object)allAssets.FirstOrDefault();

                        if (instance is null)
                        {
                            // A ScriptableObject instance that isn't bound to a .asset file
                            instance = (T)(object)ScriptableObject.CreateInstance(path);
                        }
                    }
                }

                var bindingFlags = BindingFlags.Instance
                                    | BindingFlags.FlattenHierarchy
                                    | BindingFlags.NonPublic
                                    | BindingFlags.Public;

                var initMethod = instance?.GetType().GetMethod("Init", bindingFlags);
                if (initMethod != null)
                {
                    // Throws internal exceptions inside of Init() method
                    try
                    {
                        initMethod.Invoke(instance, null);
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException;
                    }
                }

            }

            return instance;
        }

        protected static void VerifyFactoryCleaner<T>(Type genericsType, T instance)
        {
            Type factoryAttributeType = typeof(FactoryReferenceAttribute);

            bool containsFactoryAttribute = Attribute.IsDefined(instance.GetType(), factoryAttributeType);
            if (!containsFactoryAttribute)
            {
                containsFactoryAttribute = Attribute.IsDefined(genericsType, factoryAttributeType);
            }

            if (containsFactoryAttribute
                && instance is Component component
                && component.gameObject != null)
            {
                if (component.gameObject.GetComponent<FactoryCleaner>() is null)
                {
                    component.gameObject.AddComponent<FactoryCleaner>();
                }
            }
        }
    }
}
