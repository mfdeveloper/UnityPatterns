using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityPatterns.Factory;
using UnityPatterns.ScriptableObjects;
using UnityPatterns.Extensions;

namespace UnityPatterns.Samples
{
    [AddComponentMenu("InjectScriptableObject")]
    public class InjectScriptableObjectExample : MonoBehaviour
    {
        private const string TAG = nameof(InjectScriptableObjectExample);

        [SerializeField]
        private Text[] textsDebug = {};
        
        // An optional property, to avoid the "verbosity"
        // of several calls to "GetCompletionValue<>()"
        public Task<ScriptableStrategy> Strategy => this.GetCompletionValue<ScriptableStrategy>();

        private async void Awake()
        {
            InitComponents();

            await TryGetScriptable();
        }

        private async void OnEnable()
        {
            // Or you can call "this.GetCompletionValue<ScriptableStrategy>()" directly,
            // if you prefer
            var strategy = await Strategy;
            Debug.Log($"[{TAG}] [OnEnable] => {nameof(strategy.Value)} : {strategy.Value}");
        }
        
        private void InitComponents()
        {
            if (textsDebug.Length == 0)
            {
                textsDebug = GetComponentsInChildren<Text>();
            }
        }

        private async Task TryGetScriptable()
        {
            var scriptable = await FactoryComponent.GetTask(
                cancellationToken: destroyCancellationToken,
                taskCompletionSource: this.GetTaskCompletion<ScriptableStrategy>()
            );

            if (scriptable is not null)
            {
                var values = JsonUtility.ToJson(scriptable);
                LoadDebug(scriptable, values);

                Debug.Log($"[{TAG}] {scriptable.name} {values}");
            }
        }

        private void LoadDebug(Object scriptable, string values)
        {
            if (!textsDebug.Any())
            {
                return;
            }
            
            var labelStatusText = textsDebug.First();
            var valuesText = textsDebug.Last();
            
            if (valuesText is not null)
            {
                labelStatusText.text = "[LOADED]";
                valuesText.text = $"{scriptable.name} {values}";
            } 
            else if (labelStatusText.text.Contains("{0}"))
            {
                labelStatusText.text = $"[LOADED] {scriptable.name} {values}";
            }
        }
    }
}
