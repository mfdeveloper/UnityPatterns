using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Object = UnityEngine.Object;

namespace UnityPatterns.Extensions
{
    public static class UnityObjectExts
    {
        /// <summary>
        /// A weak collection <see cref="Dictionary{TKey, TValue}"/> of key => value to get/set properties to an instance. 
        /// This is a workaround to extensions properties, that isn't party of C# 8 
        /// <b>Reference:</b> <a href="https://stackoverflow.com/questions/619033/does-c-sharp-have-extension-properties">Does C# have extension properties?</a>
        /// </summary>
        private static ConditionalWeakTable<object, Dictionary<Type, object>> taskCompletions = new();
        
        public static TaskCompletionSource<T> GetTaskCompletion<T>(
            this Object unityObj, 
            object state = null, 
            TaskCreationOptions creationOptions = TaskCreationOptions.None
        )
        {
            var typeLookup = typeof(T);
            var sources = taskCompletions.GetOrCreateValue(unityObj);
            
            if (sources.TryGetValue(typeLookup, out var completionSource))
            {
                if (completionSource is TaskCompletionSource<T> value)
                {
                    return value;
                }
            }

            var newSource = CreateTaskCompletion<T>(state, creationOptions);
            
            sources.Add(typeLookup, newSource);
            taskCompletions.AddOrUpdate(unityObj, sources);
            
            return newSource;
        }
        
        public static Task<T> GetCompletionValue<T>(this Object unityObj)
        {
            var completionSource = GetTaskCompletion<T>(unityObj);
            return completionSource.Task;
        }
        
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Dictionary<Type, TaskCompletionSource<T>> GetAllTaskCompletions<T>(
            this Object unityObj)
        {
            return taskCompletions.GetOrCreateValue(unityObj)
                .Where(pair => pair.Value is TaskCompletionSource<T>)
                .ToDictionary(pair => pair.Key, pair => (TaskCompletionSource<T>) pair.Value);
        }
        

        private static TaskCompletionSource<T> CreateTaskCompletion<T>(
            object state, 
            TaskCreationOptions creationOptions
        )
        {
            TaskCompletionSource<T> completionSource;
            if (state != null && creationOptions != TaskCreationOptions.None)
            {
                completionSource = new TaskCompletionSource<T>(state, creationOptions);
            }
            else if (state != null)
            {
                completionSource = new TaskCompletionSource<T>(state);
            }
            else if (creationOptions != TaskCreationOptions.None)
            {
                completionSource = new TaskCompletionSource<T>(creationOptions);
            }
            else
            {
                completionSource = new TaskCompletionSource<T>();
            }

            return completionSource;
        }
    }
}
