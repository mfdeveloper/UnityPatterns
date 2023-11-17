using System;
using System.IO;
using System.Diagnostics.CodeAnalysis;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

using Object = UnityEngine.Object;

namespace UnityPatterns.Extensions
{
    public static class ScriptableObjectExts
    {
        /// <summary>
        /// Retrieves selected folder on Project view.
        /// </summary>
        /// <remarks>
        /// <b> WARNING: </b> Call this method from <i>Editor</i> scripts only
        /// (e.g <see cref="Editor"/>, <see cref="EditorWindow"/>, <see cref="ScriptableWizard"/>...)
        /// <br/><br/>
        /// <b> References </b>
        /// <ul>
        ///     <li>
        ///         <a href="https://discussions.unity.com/t/how-to-get-the-current-selected-folder-of-project-window/73156/4">
        ///             How to get the current selected folder of “Project” Window
        ///         </a>
        ///     </li>
        /// </ul>
        /// </remarks>
        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public static string GetSelectedPath(this ScriptableObject scriptableObject, Type assetType = null)
        {
            #if !UNITY_EDITOR
            
            if (Debug.isDebugBuild)
            {
                Debug.LogWarning($"{nameof(GetSelectedPath)}() method should be called only from Editor scripts");
            }
            
            return null;
            
            #else
            
            string path = null;
            var assetsSelection = Selection.GetFiltered(assetType ?? typeof(Object), SelectionMode.Assets);
            
            foreach (var obj in assetsSelection)
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    continue;
                }
                
                path = Path.GetDirectoryName(path);
                break;
            }
            
            return path;
            
            #endif
        }
        
        /// <summary>
        /// Retrieves selected folder on Project view.
        /// </summary>
        /// <remarks>
        /// <b> PS: </b> Call this statically from another static method (e.g Custom Menu items, Unity Initialize methods...)   
        /// </remarks>
        public static string GetSelectedPath(Type assetType = null) => GetSelectedPath(null, assetType);

        /// <summary>
        /// Retrieves selected folder on Project view.
        /// </summary>
        /// <remarks>
        /// <b> WARNING:</b> Call this method from <i>Editor</i> scripts only
        /// (e.g <see cref="Editor"/>, <see cref="EditorWindow"/>, <see cref="ScriptableWizard"/>...)
        /// </remarks>
        public static string GetSelectedPath<T>(this ScriptableObject scriptableObject) where T : Object => GetSelectedPath(scriptableObject, typeof(T));
    }
}
