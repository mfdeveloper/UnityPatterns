using System;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityPatterns.Util;
using UnityPatterns.Extensions;
using UnityPatterns.Factory.Attributes;
using UnityPatterns.ScriptableObjects;

namespace UnityPatterns.Factory
{
    /// <summary>
    /// Factory method implementation that's retrieves
    /// a specific instance in the scene
    /// </summary>
    /// <remarks>
    /// TODO: <b>[Improvement]</b> Use <see cref="Injection.Injector"/> component class instead of this one, in order to resolve dependencies
    /// </remarks>
    public static class FactoryComponent
    {
        public const string TAG = nameof(FactoryComponent);

        private static Dictionary<Type, object> componentsInstances = new();
        private static ScriptableObject[] scriptableObjects = {};
        private static InjectorSettings injectorSettings;

        /// <summary>
        /// Resources folder that are the .asset files to load
        /// <see cref="ScriptableObject"/> singletons
        /// </summary>
        public static string ResourcesAssetsFolder { get; private set; } = "ScriptableObjects";

        static FactoryComponent()
        {
            AddressablesExts.Tag = TAG;
            
            SceneManager.sceneUnloaded += OnDestroy;
            Application.quitting += OnDestroy;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize()
        {
            if (scriptableObjects.Length == 0)
            {
                scriptableObjects = Resources.FindObjectsOfTypeAll<ScriptableObject>();
            }

            LoadSettings();
        }
        
        private static void LoadSettings()
        {
            if (injectorSettings == null)
            {
                injectorSettings = InjectorSettings.Load();
            }
            
            // Populate fields from "InjectorSettings" ScriptableObject data
            ResourcesAssetsFolder = injectorSettings.resourcesAssetsFolder;
        }

        // TODO: [Refactor] Consider move this method to a "Util" scene class 
        public static List<GameObject> GetAllRootObjects(bool dontDestroyOnly = false, bool activeSceneOnly = false)
        {
            // TODO: [Refactor] Try to remove this circular reference of "DontDestroyOnLoadManager" here
            var rootGameObjects = new List<GameObject>(DontDestroyOnLoadManager.Instance.RootGameObjects);

            void AddObjectsFromScene(Scene scene, ref List<GameObject> gameObjects)
            {
                if (scene.rootCount > 0)
                {
                    gameObjects.AddRange(scene.GetRootGameObjects());
                }
            }

            if (!dontDestroyOnly)
            {
                Scene scene;
                if (activeSceneOnly)
                {
                   scene = SceneManager.GetActiveScene();
                   AddObjectsFromScene(scene, ref rootGameObjects);
                }
                else
                {
                    for (int i = 0; i < SceneManager.sceneCount; i++)
                    {
                       scene = SceneManager.GetSceneAt(i); 
                       AddObjectsFromScene(scene, ref rootGameObjects);
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
        ///         IMyComponent myComponent = FactoryComponent.GetAll&lt;IMyComponent&gt;()
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
            var gameObjects = GetAllRootObjects();

            IEnumerable<T> result = gameObjects.Select(gameObj =>
            {
                try
                {
                    return gameObj.GetComponentInChildren<T>(includeInactive);
                }
                catch (ArgumentException)
                {
                    return default;
                }
            })
            .Where(comp => comp != null);

            if (result.Any() && !genericsType.IsInterface && Debug.isDebugBuild)
            {
                string warnMsg = $"[{TAG}] The generic type \"{genericsType.Name}\" isn't an interface. " +
                    $"Prefer use GetComponent() or GetComponentInChildren() to improve performance " +
                    $"to lookup by script class.";

                Debug.LogWarning(warnMsg);
            }

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
        /// <br/><br/>
        /// <b> WARNING:</b>
        /// <br/>
        /// Prefer use the async loading method <see cref="GetAsync{T}"/>. Use this one only if you REALLY need
        /// loading dependencies <i>synchronously</i>.
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
        ///         IMyComponent myComponent = FactoryComponent.Get&lt;IMyComponent&gt;();
        ///
        ///         // Check if the result is a instance of the script
        ///         Debug.Log(myComponent.GetType().Name); // Output: MyScriptComponent
        ///
        ///         // Optionally, you can get a ScriptableObject from an interface, as well
        ///         // By default, looking for a .asset stored in: "Resources/ScriptableObjects"
        ///         
        ///         var myScriptable = FactoryComponent.Get&lt;IMyScriptable&gt;();
        ///         myScriptable
        ///     }
        /// }
        /// </code>
        /// </example>
        public static T Get<T>(bool includeInactive = false)
        {
            Type genericsType = typeof(T);

            // Lookup the interface/class instance in memory (by key or value)
            var instancePair = componentsInstances.FirstOrDefault(instance =>
            {
                return instance.Key == genericsType || genericsType.IsInstanceOfType(instance.Value);
            });

            if (instancePair.Value != null)
            {
                return (T) instancePair.Value;
            }

            if (!componentsInstances.ContainsKey(genericsType))
            {
                // TODO: [Refactor] Consider move this verification to the "GetAll<>()" method
                T instance = GetAll<T>(includeInactive).FirstOrDefault();

                if (instance == null)
                {
                    instance = FetchScriptableObject<T>(genericsType);
                }

                if (instance != null)
                {
                    VerifyFactoryCleaner(genericsType, instance);
                    componentsInstances.TryAdd(genericsType, instance);
                }
            }

            if (componentsInstances.TryGetValue(genericsType, out var resultInstance))
            {
                return (T) resultInstance;
            }
            
            return (T)(object) null;
        }
        
        public static async Task<T> GetTask<T>(
            string addressableAddress = "", 
            bool strictInstance = true, 
            bool includeInactive = false,
            CancellationToken cancellationToken = default,
            TaskCompletionSource<T> taskCompletionSource = default)
        {
            Type genericsType = typeof(T);

            // Lookup the interface/class instance in memory (by key or value)
            var instancePair = componentsInstances.FirstOrDefault(instance =>
            {
                return instance.Key == genericsType || genericsType.IsInstanceOfType(instance.Value);
            });

            if (instancePair.Value != null)
            {
                taskCompletionSource?.SetResult((T) instancePair.Value);
                return (T) instancePair.Value;
            }

            if (!componentsInstances.ContainsKey(genericsType))
            {
                // TODO: [Refactor] Consider move this verification to the "GetAll<>()" method
                T instance = GetAll<T>(includeInactive).FirstOrDefault();

                if (instance == null)
                {
                    instance = await FetchScriptableTask<T>(
                        addressableAddress, 
                        strictInstance, 
                        genericsType,
                        cancellationToken
                    );
                    
                    if (cancellationToken.IsCanceled(logTag: TAG))
                    {
                        return instance;
                    }
                }

                if (instance != null)
                {
                    VerifyFactoryCleaner(genericsType, instance);
                    componentsInstances.TryAdd(genericsType, instance);
                    
                    taskCompletionSource?.SetResult(instance);

                    return instance;
                }
            }
            else if (componentsInstances.TryGetValue(genericsType, out var resultInstance))
            {
                taskCompletionSource?.SetResult((T) resultInstance);
                return (T) resultInstance;
            }

            taskCompletionSource?.SetResult(default);
            return (T)(object) null;
        }
        
        public static AsyncOperationHandle<T> GetAsync<T>(
            string addressableAddress = "", 
            bool strictInstance = true, 
            bool includeInactive = false)
        {
            Type genericsType = typeof(T);
            AsyncOperationHandle<T> instanceOp = default;

            // Lookup the interface/class instance in memory (by key or value)
            var instancePair = componentsInstances.FirstOrDefault(instance =>
            {
                return instance.Key == genericsType || genericsType.IsInstanceOfType(instance.Value);
            });

            if (instancePair.Value != null)
            {
                instanceOp = Addressables.ResourceManager.CreateCompletedOperation((T)instancePair.Value, string.Empty);
            }
            else
            {
                if (!componentsInstances.ContainsKey(genericsType))
                {
                    // TODO: [Refactor] Consider move this verification to the "GetAll<>()" method
                    T instance = GetAll<T>(includeInactive).FirstOrDefault();

                    if (instance == null)
                    {
                        instanceOp = FetchScriptableAsync<T>(
                            addressableAddress, 
                            strictInstance, 
                            genericsType
                        );

                        instanceOp.Completed += opHandle =>
                        {
                            if (opHandle is { IsDone: true, Status: AsyncOperationStatus.Succeeded })
                            {
                                VerifyFactoryCleaner(genericsType, opHandle.Result);
                                componentsInstances.TryAdd(genericsType, opHandle.Result);
                            }
                        };
                    }
                }
                else if (componentsInstances.TryGetValue(genericsType, out var resultInstance))
                {
                    instanceOp = Addressables.ResourceManager.CreateCompletedOperation((T)resultInstance, string.Empty);
                }
            }

            AddressablesExts.AsyncOperations.Add(instanceOp);
            
            return instanceOp;
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

                    return instance == instanceReference.Value;

                });

                if (component.Value != null)
                {
                    componentsInstances.Remove(component.Key);
                }
            }
        }

        /// <summary>
        /// Find and load a <see cref="ScriptableObject"/> synchronously using <see cref="Resources"/> class
        /// </summary>
        /// <remarks>
        /// <b> WARNING:</b>
        /// <br/>
        /// Prefer use <see cref="FetchScriptableAsync{T}"/> method. Use this one only if you REALLY need
        /// loading dependencies <i>synchronously</i>.
        /// </remarks>
        /// <param name="genericsType"></param>
        /// <param name="strictInstance"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static T FetchScriptableObject<T>(Type genericsType, bool strictInstance = true)
        {
            T instance = default;
            string nameOrPath = string.Empty;

            if (genericsType.IsInterface &&
                genericsType.Name.StartsWith("I", false, CultureInfo.CurrentCulture)
                || genericsType.IsAbstract &&
                genericsType.Name.StartsWith("Base", false, CultureInfo.CurrentCulture))
            {
                // Looking for a ScriptableObject .asset
                // Follow the convention for interfaces, that needs to starts with "I" and the ScriptableObject name
                // should be the same without the "I" (e.g interface "IMyScriptable" and class should be "MyScriptable")
                // IAudioManager => FMODAudioManager
                    
                nameOrPath = genericsType.Name[1..];
            } else if (genericsType.IsClass && typeof(ScriptableObject).IsAssignableFrom(genericsType))
            {
                nameOrPath = genericsType.Name;
            }

            if (!string.IsNullOrWhiteSpace(nameOrPath))
            {
                instance = (T)(object)Resources.Load($@"{ResourcesAssetsFolder}/{nameOrPath}", genericsType);

                if (instance is null && scriptableObjects.Any())
                {
                    instance = (T)(object) scriptableObjects.FirstOrDefault(scriptable => scriptable.name.EndsWith(nameOrPath));
                }
                
                if (instance is null)
                {
                    try
                    {
                        // PS: This will be called only as a fallback only when "Resources.FindObjectsOfTypeAll<ScriptableObject>()"
                        // fails for any reason.
                        var allAssets = Resources.LoadAll(ResourcesAssetsFolder, genericsType);
                        instance = (T)(object)allAssets.FirstOrDefault();
                    }
                    catch (Exception exception)
                    {
                        if (Debug.isDebugBuild)
                        {
                            Debug.LogWarning($"[{TAG}] {exception.GetType().Name} => {exception.Message}");
                        }
                    }
                }
            }
            
            if (instance is null && strictInstance)
            {
                // A ScriptableObject instance that isn't bound to a .asset file
                instance = (T)(object)ScriptableObject.CreateInstance(nameOrPath);
            }

            const BindingFlags bindingFlags = BindingFlags.Instance
                                              | BindingFlags.FlattenHierarchy
                                              | BindingFlags.NonPublic
                                              | BindingFlags.Public;

            MethodInfo initMethod = instance?.GetType().GetMethod("Init", bindingFlags);
            if (initMethod != null)
            {
                // Throws internal exceptions inside of Init() method
                try
                {
                    initMethod.Invoke(instance, null);
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException != null)
                    {
                        throw ex.InnerException;
                    }
                }
            }

            return instance;
        }

        /// <summary>
        /// Load a <see cref="ScriptableObject"/> .asset file using <see cref="Addressables"/> API
        /// from "com.unity.addressables" UPM package
        /// </summary>
        /// <remarks>
        /// <b> References </b>
        /// <br/>
        /// <ul>
        ///     <li>
        ///         <a href="https://forum.unity.com/threads/unityengine-addressableassets-invalidkeyexception-was-thrown.796908">
        ///         'UnityEngine.AddressableAssets.InvalidKeyException' was thrown
        ///         </a>
        ///     </li>
        ///     <li>
        ///         <a href="https://medium.com/@alexandre.malavasi/why-exceptions-in-async-methods-are-dangerous-in-c-fda7d382b0ff">
        ///         Why exceptions in async methods are “dangerous” in C#
        ///         </a>
        ///     </li>
        /// </ul>
        /// </remarks>
        /// <param name="addressableAddress"></param>
        /// <param name="strictInstance"></param>
        /// <param name="genericsType"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="TypeLoadException"></exception>
        /// <exception cref="Exception"></exception>
        [SuppressMessage("ReSharper", "UseNullPropagation")]
        [SuppressMessage("ReSharper", "CommentTypo")]
        public static async Task<T> FetchScriptableTask<T>(
            string addressableAddress = "", 
            bool strictInstance = true, 
            Type genericsType = null,
            CancellationToken cancellationToken = default
        )
        {
            T instance = default;
            genericsType ??= typeof(T);
            var assetTypeLookup = typeof(ScriptableObject);

            var addressExists = false;
            if (!string.IsNullOrWhiteSpace(addressableAddress))
            {
                addressExists = await AddressablesExts.IsKeyExistsTask(
                    addressableAddress, 
                    typeLookup: assetTypeLookup, 
                    cancellationToken
                );
                
                if (cancellationToken.IsCanceled(logTag: TAG))
                {
                    return instance;
                }
            }

            try
            {
                if (addressExists)
                {
                    
                    // Load by string address (URL)
                    var assetOp = Addressables.LoadAssetAsync<T>(addressableAddress);

                    instance = await assetOp.Task;
                    
                    if (cancellationToken.IsCanceled(logTag: TAG))
                    {
                        return instance;
                    }
                    
                    assetOp.Completed += handle =>
                    {
                        if (handle.Status == AsyncOperationStatus.Succeeded)
                        {
                            // Gets called for every loaded asset
                            AddressablesExts.OnLoadAddressable(handle.Result);
                        }
                    };

                    if (assetOp.Status == AsyncOperationStatus.Failed)
                    {
                        var exception = new TypeLoadException(
                            $"[{TAG}] Addressable async load error: '{addressableAddress}'",
                            assetOp.OperationException
                        );
                        throw exception;
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(addressableAddress))
                    {
                        // Load By labels (label/tag)
                        instance = await AddressablesExts.LoadAssetByLabelTask(
                            addressableLabel: addressableAddress,
                            typeLookup: assetTypeLookup,
                            onLoad: AddressablesExts.OnLoadAddressable<T>(), // Gets called for every loaded asset
                            cancellationToken
                        );
                        
                        if (cancellationToken.IsCanceled(logTag: TAG))
                        {
                            return instance;
                        }
                    }

                    if (instance is null)
                    {
                        var nameOrPath = string.Empty;
                        
                        if (genericsType.IsInterface &&
                            genericsType.Name.StartsWith("I", false, CultureInfo.CurrentCulture)
                            || genericsType.IsAbstract &&
                            genericsType.Name.StartsWith("Base", false, CultureInfo.CurrentCulture)
                           )
                        {
                            // Looking for a ScriptableObject .asset
                            // Follow the convention for interfaces, that needs to starts with "I" and the ScriptableObject name
                            // should be the same without the "I" (e.g interface "IMyScriptable" and class should be "MyScriptable")
                            // IMyScriptable => MyScriptable

                            // TODO: [Feature] Add a conversion from "camelCase" (MyScriptable) to "kebabCase" (my-scriptable).
                            //       That case accomplish more properly labels/tags formats.
                            //       Maybe use this nuget library?? : https://github.com/vad3x/case-extensions
                            nameOrPath = genericsType.Name[1..];
                        }
                        else if (genericsType.IsClass && typeof(ScriptableObject).IsAssignableFrom(genericsType))
                        {
                            nameOrPath = genericsType.Name;
                        }

                        if (!string.IsNullOrWhiteSpace(nameOrPath))
                        {
                            
                            // Load by name from interface (label/tag)
                            instance = await AddressablesExts.LoadAssetByLabelTask(
                                addressableLabel: nameOrPath,
                                typeLookup: assetTypeLookup,
                                onLoad: AddressablesExts.OnLoadAddressable<T>(), // Gets called for every loaded asset
                                cancellationToken
                            );
                            
                            if (cancellationToken.IsCanceled(logTag: TAG))
                            {
                                return (T)(object) null;
                            }
                        }

                        if (instance is null)
                        {
                            // Fallback here to "Resources.Load()"
                            // TODO: [Feature] Replace that fallback to a Resources.LoadAsync<>() Coroutine call
                            instance = FetchScriptableObject<T>(genericsType, strictInstance);
                        }
                    }
                }
            }
            catch (AggregateException aggregateException)
            {
                if (Debug.isDebugBuild && aggregateException.InnerExceptions.Count > 0)
                {
                    foreach (var exception in aggregateException.InnerExceptions)
                    {
                        var msgError = $"[{TAG}] Asset for \"{addressableAddress}\" failed to load.\n " +
                                       $"Exception => \"{exception.GetType().FullName} => {exception.Message}\"";
                        Debug.LogError(msgError);
                    }
                }
            }
            catch (Exception exception)
            {
                if (Debug.isDebugBuild)
                {
                    var msgError = $"[{TAG}] Asset for \"{addressableAddress}\" failed to load.\n " +
                                   $"Exception => \"{exception.GetType().FullName} => {exception.Message}\"";
                    Debug.LogError(msgError);
                }
            }

            const BindingFlags bindingFlags = BindingFlags.Instance
                                              | BindingFlags.FlattenHierarchy
                                              | BindingFlags.NonPublic
                                              | BindingFlags.Public;

            if (instance is not null)
            {
                var instanceType = instance.GetType();
                MethodInfo initMethod = instanceType.GetMethod("Init", bindingFlags);
                if (initMethod is not null)
                {
                    // Throws internal exceptions inside of Init() method
                    try
                    {
                        // Await: Test if the result of "Init()" method is a Task
                        // and await for the execution
                        var invokeResult = initMethod.Invoke(instance, null);
                        if (invokeResult is Task task)
                        {
                            await task;
                        }
                    }
                    catch (TargetInvocationException ex)
                    {
                        if (ex.InnerException != null)
                        {
                            throw ex.InnerException;
                        }
                    }
                }
                
                initMethod = instanceType.GetMethod("InitAsync", bindingFlags)
                             ?? instanceType.GetMethod("InitTask", bindingFlags);
                if (initMethod is not null)
                {
                    try
                    {
                        if (typeof(Task).IsAssignableFrom(initMethod.ReturnType))
                        {
                            var task = (Task) initMethod.Invoke(instance, null);
                            await task;
                        }
                        else if (Debug.isDebugBuild)
                        {
                            Debug.LogError($"[{TAG}] The async method '{initMethod.Name}' should return Task or Task<T>");
                        }
                    }
                    catch (TargetInvocationException ex)
                    {
                        if (ex.InnerException != null)
                        {
                            throw ex.InnerException;
                        }
                    }
                }
            }

            return instance;
        }
        
        public static AsyncOperationHandle<T> FetchScriptableAsync<T>(
            string addressableAddress = "", 
            bool strictInstance = true, 
            Type genericsType = null
        )
        {
            T instance = default;
            genericsType ??= typeof(T);
            var assetTypeLookup = typeof(ScriptableObject);
            
            // AsyncOperations
            AsyncOperationHandle<bool> keyExistsOp = default;
            AsyncOperationHandle<T> assetOp = default;
            AsyncOperationHandle<T> instanceNameLabelOp = default;
            AsyncOperationHandle<T> instanceAddressLabelOp = default;
            
            if (!string.IsNullOrWhiteSpace(addressableAddress))
            {
                keyExistsOp = AddressablesExts.IsKeyExistsAsync(addressableAddress, typeLookup: assetTypeLookup);
            }
            
            if (keyExistsOp.IsValid())
            {
                if (assetOp.IsValid())
                {
                    Addressables.Release(assetOp);
                }
                
                // Load by string address (URL)
                assetOp = Addressables.ResourceManager.CreateChainOperation(
                    dependentOp: keyExistsOp,
                    callback: dependentOp =>
                    {
                        string errorMsg = string.Empty;
                        
                        if (dependentOp.IsDone 
                            && dependentOp is { Status: AsyncOperationStatus.Succeeded, Result: true })
                        {
                            return Addressables.LoadAssetAsync<T>(addressableAddress);
                        }

                        if (dependentOp.Status == AsyncOperationStatus.Failed)
                        {
                            errorMsg = $"Failed to try load an asset from: '{addressableAddress}' address";
                        }
                        
                        return Addressables.ResourceManager.CreateCompletedOperation((T)(object) null, errorMsg);
                    }
                );
                
                assetOp.Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        // Gets called for every loaded asset
                        AddressablesExts.OnLoadAddressable(handle.Result);
                    }
                };

                if (assetOp.Status == AsyncOperationStatus.Failed)
                {
                    var exception = new TypeLoadException(
                        $"[{TAG}] Addressable async load error: '{addressableAddress}'",
                        assetOp.OperationException
                    );
                    throw exception;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(addressableAddress))
                {
                    instanceAddressLabelOp = AddressablesExts.LoadAssetByLabelAsync(
                        addressableLabel: addressableAddress,
                        typeLookup: assetTypeLookup,
                        onLoad: AddressablesExts.OnLoadAddressable<T>()
                    );
                }
                else
                {
                    instanceAddressLabelOp = Addressables.ResourceManager.CreateCompletedOperation(
                        instance, 
                        string.Empty
                    );
                }

                instanceNameLabelOp = Addressables.ResourceManager.CreateChainOperation(
                    dependentOp: instanceAddressLabelOp,
                    callback: dependentOp =>
                    {
                        if (dependentOp.IsDone
                            && dependentOp is { Status: AsyncOperationStatus.Succeeded, Result: null })
                        {
                            var nameOrPath = string.Empty;
                                
                            if (genericsType.IsInterface &&
                                genericsType.Name.StartsWith("I", false, CultureInfo.CurrentCulture)
                                || genericsType.IsAbstract &&
                                genericsType.Name.StartsWith("Base", false, CultureInfo.CurrentCulture)
                               )
                            {
                                // Looking for a ScriptableObject .asset
                                // Follow the convention for interfaces, that needs to starts with "I" and the ScriptableObject name
                                // should be the same without the "I" (e.g interface "IMyScriptable" and class should be "MyScriptable")
                                // IMyScriptable => MyScriptable

                                // TODO: [Feature] Add a conversion from "camelCase" (MyScriptable) to "kebabCase" (my-scriptable).
                                //       That case accomplish more properly labels/tags formats.
                                //       Maybe use this nuget library?? : https://github.com/vad3x/case-extensions
                                nameOrPath = genericsType.Name[1..];
                            }
                            else if (genericsType.IsClass && typeof(ScriptableObject).IsAssignableFrom(genericsType))
                            {
                                nameOrPath = genericsType.Name;
                            }

                            if (!string.IsNullOrWhiteSpace(nameOrPath))
                            {
                                return AddressablesExts.LoadAssetByLabelAsync(
                                    addressableLabel: nameOrPath,
                                    typeLookup: assetTypeLookup,
                                    onLoad: AddressablesExts.OnLoadAddressable<T>()
                                );
                            }
                        }
                        
                        return dependentOp;
                    }
                );

                assetOp = Addressables.ResourceManager.CreateChainOperation(
                    dependentOp: instanceNameLabelOp,
                    callback: dependentOp =>
                    {
                        if (dependentOp.IsDone
                            && dependentOp is { Status: AsyncOperationStatus.Succeeded, Result: not null })
                        {
                            instance = dependentOp.Result;
                        }
                        else
                        {
                            // Fallback here to Resources.Load()
                            // TODO: [Feature] Replace that fallback to a Resources.LoadAsync<>() Coroutine call
                            #pragma warning disable CS0618 // Type or member is obsolete
                            
                            instance = FetchScriptableObject<T>(genericsType, strictInstance);
                            
                            #pragma warning restore CS0618 // Type or member is obsolete
                        }
                        
                        if (instance is not null)
                        {
                            const BindingFlags bindingFlags = BindingFlags.Instance
                                                              | BindingFlags.FlattenHierarchy
                                                              | BindingFlags.NonPublic
                                                              | BindingFlags.Public;
                            
                            MethodInfo initMethod = instance.GetType().GetMethod("Init", bindingFlags);
                            if (initMethod is not null)
                            {
                                // Throws internal exceptions inside of Init() method
                                try
                                {
                                    initMethod.Invoke(instance, null);
                                }
                                catch (TargetInvocationException ex)
                                {
                                    if (ex.InnerException != null)
                                    {
                                        throw ex.InnerException;
                                    }
                                }
                            }
                        }

                        return Addressables.ResourceManager.CreateCompletedOperation(instance, string.Empty);
                    }
                );
            }

            AddressablesExts.AsyncOperations.AddRange(new AsyncOperationHandle[]
                { keyExistsOp, 
                    assetOp, 
                    instanceNameLabelOp, 
                    instanceAddressLabelOp 
                }
            );
            
            return assetOp;
        }

        private static void VerifyFactoryCleaner<T>(Type genericsType, T instance)
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
        
        private static void OnDestroy(Scene scene)
        {
            Dispose();
            SceneManager.sceneUnloaded -= OnDestroy;
        }

        private static void OnDestroy()
        {
            Dispose();
            Application.quitting -= OnDestroy;
        }

        private static void Dispose()
        {
            if (!AddressablesExts.AsyncOperations.Any())
            {
                return;
            }
            
            foreach (var opHandle in AddressablesExts.AsyncOperations)
            {
                if (!opHandle.IsValid())
                {
                    continue;
                }

                if (Debug.isDebugBuild)
                {
                    string resultLog = opHandle.Result != null ? opHandle.Result.ToString() : "null";
                    if (opHandle.Result is IEnumerable)
                    {
                        resultLog = string.Join(",", opHandle.Result);
                    }
                    
                    var operationDetail = $"{opHandle.GetType().Name} ({resultLog})";
                    Debug.Log($"[{TAG}] [Destroy] Releasing: {operationDetail}");
                }

                Addressables.Release(opHandle);
            }

            AddressablesExts.AsyncOperations.Clear();
        }
    }
}
