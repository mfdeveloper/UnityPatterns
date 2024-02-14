using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityPatterns.Extensions
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public static class AddressablesExts
    {
        public static string Tag { get; set; } = nameof(AddressablesExts);
        public static HashSet<AsyncOperationHandle> AsyncOperations { get; set; } = new();
        
        public static async Task<T> LoadAssetByLabelTask<T>(
            string addressableLabel,
            Type typeLookup = null,
            Action<T> onLoad = default,
            CancellationToken cancellationToken = default
        )
        {
            T instance = default;
            typeLookup ??= typeof(ScriptableObject);
            var labels = new List<string> { addressableLabel };
            const Addressables.MergeMode mergeMode = Addressables.MergeMode.Union;

            var labelsExists = await IsKeyExistsTask(labels, mergeMode, typeLookup, cancellationToken);
            if (!labelsExists)
            {
                return instance;
            }
            
            var listAssetsOp = Addressables.LoadAssetsAsync(
                labels,
                onLoad,
                Addressables.MergeMode.Union,
                true
            );

            if (cancellationToken.IsCancellationRequested)
            {
                if (Debug.isDebugBuild)
                {
                    Debug.Log($"[{Tag}] {nameof(LoadAssetByLabelTask)} was CANCELED");
                }
                return instance;
            }
            
            var assetsCollection = await listAssetsOp.Task;
            
            if (assetsCollection.Count > 0)
            {
                instance = assetsCollection.FirstOrDefault();
            } else if (listAssetsOp.Status == AsyncOperationStatus.Failed)
            {
                var exception = new TypeLoadException(
                    $"[{Tag}] Addressable async load error: '{string.Join(",", labels)}'", 
                    listAssetsOp.OperationException
                );
                throw exception;
            }

            AsyncOperations.Add(listAssetsOp);

            return instance;
        }
        
        public static AsyncOperationHandle<T> LoadAssetByLabelAsync<T>(
            string addressableLabel,
            Type typeLookup = null,
            Action<T> onLoad = default
        )
        {
            T instance = default;
            typeLookup ??= typeof(ScriptableObject);
            var labels = new List<string> { addressableLabel };
            const Addressables.MergeMode mergeMode = Addressables.MergeMode.Union;
            
            var keyExistsOp = IsKeyExistsAsync(labels, mergeMode, typeLookup);
            var listAssetsOp = Addressables.ResourceManager.CreateChainOperation(
                dependentOp: keyExistsOp,
                callback: dependentOp =>
                {
                    string errorMsg = string.Empty;
                    
                    if (dependentOp.IsDone 
                        && dependentOp is { Status: AsyncOperationStatus.Succeeded, Result: true })
                    {
                        return Addressables.LoadAssetsAsync(
                            labels,
                            onLoad,
                            Addressables.MergeMode.Union,
                            true
                        );
                    }

                    if (dependentOp.Status == AsyncOperationStatus.Failed)
                    {
                        errorMsg = $"Failed to load assets List from: '{string.Join(",", labels)}' labels";
                    }

                    return Addressables.ResourceManager.CreateCompletedOperation<IList<T>>(new List<T>(), errorMsg);
                }
            );
            
            var instanceOp = Addressables.ResourceManager.CreateChainOperation(
                dependentOp: listAssetsOp,
                callback: dependentOp =>
                {
                    TypeLoadException listLoadException = null;
                    
                    if (dependentOp.IsDone 
                        && dependentOp is { Status: AsyncOperationStatus.Succeeded, Result: { Count: > 0 } })
                    {
                        instance = dependentOp.Result.FirstOrDefault();
                    }
                    else if (dependentOp.Status == AsyncOperationStatus.Failed)
                    {
                        listLoadException = new TypeLoadException(
                            $"[{Tag}] Addressable async load error: '{string.Join(",", labels)}'", 
                            dependentOp.OperationException
                        );
                    }
                    
                    return Addressables.ResourceManager.CreateCompletedOperationWithException(instance, listLoadException);
                }
            );

            AsyncOperations.Add(keyExistsOp);
            AsyncOperations.Add(listAssetsOp);
            AsyncOperations.Add(instanceOp);
            
            return instanceOp;
        }
        
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task<bool> IsKeyExistsTask(object key, Type typeLookup = null, CancellationToken cancellationToken = default)
        {
            var locationsAsyncOp = Addressables.LoadResourceLocationsAsync(key, typeLookup);
            if (cancellationToken.IsCancellationRequested)
            {
                if (Debug.isDebugBuild)
                {
                    Debug.Log($"[{Tag}] {nameof(IsKeyExistsTask)} was CANCELED");
                }
                return false;
            }
            
            var resourceLocations = await locationsAsyncOp.Task;

            if (locationsAsyncOp.Status == AsyncOperationStatus.Failed)
            {
                return false;
            }

            AsyncOperations.Add(locationsAsyncOp);
            
            return IsKeyExists(resourceLocations, key);
        }
        
        public static AsyncOperationHandle<bool> IsKeyExistsAsync(
            IEnumerable key, 
            Addressables.MergeMode mode = Addressables.MergeMode.Union, 
            Type typeLookup = null
        )
        {
            var locationsAsyncOp = Addressables.LoadResourceLocationsAsync(key, mode, typeLookup);
            
            var mapOp = Addressables.ResourceManager.CreateChainOperation(
                dependentOp: locationsAsyncOp,
                callback: (dependentOp) =>
                {
                    var keyExists = false; 
                    string errorMsg = string.Empty;
                    
                    if (dependentOp.IsDone && dependentOp.Status == AsyncOperationStatus.Succeeded)
                    {
                        keyExists = IsKeyExists(dependentOp.Result, key);
                    } else if (dependentOp.Status == AsyncOperationStatus.Failed)
                    {
                        var keyLog = key.ToString();
                        if (key is IList)
                        {
                            keyLog = string.Join(",", key);
                        }
                        errorMsg = $"Failed to check if the key '{keyLog}' exists";
                    }
                    
                    return Addressables.ResourceManager.CreateCompletedOperation(keyExists, errorMsg);
                },
                releaseDependenciesOnFailure: true
            );
            
            AsyncOperations.Add(locationsAsyncOp);
            AsyncOperations.Add(mapOp);
            
            return mapOp;
        }

        /// <summary>
        /// TODO: [Feature] Consider create Addressable custom operation inheriting from <see cref="AsyncOperationBase{TObject}"/> <br/><br/>
        /// 
        /// <b>See:</b>
        /// <a href="https://docs.unity3d.com/Packages/com.unity.addressables@1.17/manual/AddressableAssetsCustomOperation.html">
        /// Custom operations
        /// </a>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="mode"></param>
        /// <param name="typeLookup"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> IsKeyExistsTask(
            IEnumerable key, 
            Addressables.MergeMode mode = Addressables.MergeMode.Union, 
            Type typeLookup = null,
            CancellationToken cancellationToken = default
        )
        {
            var locationsAsyncOp = Addressables.LoadResourceLocationsAsync(key, mode, typeLookup);
            if (cancellationToken.IsCancellationRequested)
            {
                if (Debug.isDebugBuild)
                {
                    Debug.Log($"[{Tag}] {nameof(IsKeyExistsTask)} was CANCELED");
                }
                return false;
            }
            
            var resourceLocations = await locationsAsyncOp.Task;
            
            if (locationsAsyncOp.Status == AsyncOperationStatus.Failed)
            {
                return false;
            }
            
            AsyncOperations.Add(locationsAsyncOp);
            
            return IsKeyExists(resourceLocations, key);
        }
        
        /// <summary>
        /// Should be called by `Addressables.LoadAssets()` for every loaded assetAddressables.LoadAssets
        /// </summary>
        /// <param name="asset"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Action<T> OnLoadAddressable<T>(T asset = default)
        {
            Action<T> action = addressable =>
            {
                
                if (addressable is not null && Debug.isDebugBuild)
                {
                    Debug.Log($"[{Tag}] Loaded addressable asset: \"{addressable}\"");
                }
            };

            if (asset is not null)
            {
                action.Invoke(asset);
            }

            return action;
        }
        
        private static bool IsKeyExists(IList<IResourceLocation> resourceLocations, object key)
        {
            try
            {
                return resourceLocations is not null && resourceLocations.Count > 0;
            }
            catch (Exception e)
            {
                if (Debug.isDebugBuild)
                {
                    var warnMsg = $"[${Tag}] The Addressable key \"{key}\" doesn't exists.\n" +
                                  $"Exception: \"{e.GetType().FullName} => {e.Message}\"";
                    Debug.LogWarning(warnMsg);
                }

                return false;
            }
        }
    }
}
