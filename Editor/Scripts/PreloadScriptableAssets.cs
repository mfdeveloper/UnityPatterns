using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityPatterns.Extensions;
using UnityPatterns.ScriptableObjects;

using Object = UnityEngine.Object;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace UnityPatterns.Editor
{
    public class PreloadScriptableAssets : ScriptableWizard
    {
        
        [SerializeField]
        private bool useResourcesFolder = true;
        
        [Header("Resources paths")]
        [SerializeField]
        private string resourcesAssetsFolder = string.Empty;
        
        private static PackageInfo packageInfo;
        private static InjectorSettings injectorSettings;
        
        [MenuItem("Assets/UnityPatterns/Add Preload Assets")]
        public static void CreateWizard()
        {
            DisplayWizard<PreloadScriptableAssets>($"Preload assets", "Verify");
        }
        
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            AddToPreload();
        }

        private void Awake()
        {
            InitFields();
        }

        private void OnDisable()
        {
            SaveChanges();
        }

        public void OnWizardCreate()
        {
            packageInfo = PackageInfo.FindForAssembly(typeof(PreloadScriptableAssets).Assembly);
            
            var selectedFolder = this.GetSelectedPath();
            AddToPreload(scriptablesFolder: selectedFolder, displayNothingToDoDialog: true);
        }
        
        public void OnWizardUpdate()
        {
            UpdateFields();
        }

        public override void SaveChanges()
        {
            base.SaveChanges();
            
            if (injectorSettings != null && AssetDatabase.Contains(injectorSettings))
            {
                AssetDatabase.SaveAssets();
            }
            else
            {
                if (!Directory.Exists(InjectorSettings.SettingsDataPath))
                {
                    var directoryInfo = Directory.CreateDirectory(InjectorSettings.SettingsDataPath);
                    if (directoryInfo.Exists && Debug.isDebugBuild)
                    {
                        Debug.Log($"Created directory: '{directoryInfo.FullName}'");
                    }
                }
                
                var assetPath = $"{InjectorSettings.SettingsDataPath}/{injectorSettings.GetType().Name}.asset";
                AssetDatabase.CreateAsset(injectorSettings, assetPath);
            }
        }
        
        private void InitFields()
        {

            if (injectorSettings == null)
            {
                injectorSettings = InjectorSettings.Load();
            }

            if (injectorSettings != null && AssetDatabase.Contains(injectorSettings))
            {
                useResourcesFolder = injectorSettings.useResourcesFolder;
                resourcesAssetsFolder = injectorSettings.resourcesAssetsFolder;
            }
        }

        private void UpdateFields()
        { 
            if (string.IsNullOrWhiteSpace(resourcesAssetsFolder))
            {
                resourcesAssetsFolder = injectorSettings.resourcesAssetsFolder;
            } else if (!string.IsNullOrWhiteSpace(resourcesAssetsFolder) && injectorSettings.resourcesAssetsFolder != resourcesAssetsFolder)
            {
                injectorSettings.resourcesAssetsFolder = resourcesAssetsFolder;
            }

            if (injectorSettings != null && injectorSettings.useResourcesFolder != useResourcesFolder)
            {
                injectorSettings.useResourcesFolder = useResourcesFolder;
            }
            
            isValid = useResourcesFolder;

            if (!useResourcesFolder)
            {
                errorString = $"\"Use Resources Folder\" flag is required in order to Preload ScriptableObjects " +
                              $".asset files from \"{injectorSettings.resourcesAssetsFolder}\" folder";
            }
            else
            {
                errorString = string.Empty;
            }
        }

        private static void AddToPreload(string scriptablesFolder = null, bool displayNothingToDoDialog = false)
        {
            if (injectorSettings == null)
            {
                injectorSettings = InjectorSettings.Load();
            }
            
            scriptablesFolder ??= $"{InjectorSettings.BASE_RESOURCES_PATH}/{injectorSettings.resourcesAssetsFolder}";
                
            if (!injectorSettings.useResourcesFolder)
            {
                return;
            }
            
            var assetsFiles = Directory.GetFiles(scriptablesFolder).Where(filePath => !filePath.EndsWith(".meta")).ToArray();
            if (!assetsFiles.Any())
            {
                return;
            }

            // Add the config asset to the build
            var assetsToAdd = new HashSet<Object>();
            var preloadedAssets = PlayerSettings.GetPreloadedAssets()
                .Where(preloadedAsset => preloadedAsset is ScriptableObject)
                .ToList();

            foreach (var filePath in assetsFiles)
            {
                var asset = preloadedAssets.Find(preloadedAsset =>
                    Path.GetFileNameWithoutExtension(filePath) == preloadedAsset.name);
                if (asset != null)
                {
                    continue;
                }

                var loadedAsset = AssetDatabase.LoadAssetAtPath<Object>(filePath);
                assetsToAdd.Add(loadedAsset);
            }

            if (assetsToAdd.Any())
            {
                var questionMessage =
                    $"[{packageInfo?.name}] The ScriptableObject assets: \n\n - {string.Join(",\n - ", assetsToAdd)}. \n\n Aren't added " +
                    "to PlayerSettings => Optimization => Preload Assets.\n\n" +
                    "Would you like to add?";

                var confirmAddAssets = EditorUtility.DisplayDialog(
                    $"[{packageInfo?.displayName}] Preload assets",
                    questionMessage,
                    "YES",
                    "NO",
                    DialogOptOutDecisionType.ForThisSession,
                    "UnityPatterns.ShowPreloadAssetsDialog"
                );

                if (confirmAddAssets)
                {
                    preloadedAssets.AddRange(assetsToAdd);
                    PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
                    
                    EditorUtility.DisplayDialog(
                        $"[{packageInfo?.displayName}] Files added",
                        "Files were added successfully",
                        "OK"
                    );
                }
            }
            else if (displayNothingToDoDialog)
            {
                EditorUtility.DisplayDialog(
                    $"[{packageInfo?.displayName}] Files added",
                    $"All assets from: \"{scriptablesFolder}\" are already into PlayerSettings => Optimization => Preload Assets",
                    "OK"
                );
            }
        }
    }
}
