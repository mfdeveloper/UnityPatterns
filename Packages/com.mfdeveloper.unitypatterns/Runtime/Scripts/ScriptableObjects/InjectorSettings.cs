using UnityEngine;

namespace UnityPatterns.ScriptableObjects
{
    [CreateAssetMenu(fileName = "InjectorSettings", menuName = "UnityPatterns/Assets/InjectorSettings")]
    public class InjectorSettings : ScriptableObject
    {
        public const string BASE_RESOURCES_PATH = "Assets/Resources";
        
        /// <summary>
        /// Fixed path: Avoid change the folder where this ScriptableObject should be saved
        /// That's can cause an error or an unexpected behavior, when try call: <see cref="UnityEditor.AssetDatabase.LoadAssetAtPath{T}"/>
        /// in order to load this <i>.asset</i> for the first time
        /// </summary>
        public const string SETTINGS_DATA_FOLDER = "ScriptableObjects/Data";
        
        [SerializeField]
        public bool useResourcesFolder;
        
        [Header("Resources paths")]
        [SerializeField]
        public string resourcesAssetsFolder = "ScriptableObjects";

        public static string SettingsDataPath => $"{BASE_RESOURCES_PATH}/{SETTINGS_DATA_FOLDER}";
        
        public static InjectorSettings Load()
        {
            InjectorSettings settings;
            
            #if UNITY_EDITOR
                        
            settings = UnityEditor.AssetDatabase.LoadAssetAtPath<InjectorSettings>($"{SettingsDataPath}/{nameof(InjectorSettings)}.asset");

            #endif
            
            if (settings == null)
            {
                settings = CreateInstance<InjectorSettings>();
            }

            return settings;
        }
    }
}
