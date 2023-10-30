using UnityEngine;

namespace UnityPatterns.Tests.Examples.ScriptableObjects
{
    public interface IScriptableData
    {
        
    }
    
    [CreateAssetMenu(fileName = "ScriptableData", menuName = "UnityPatterns/Assets/Test/ScriptableData")]
    public class ScriptableData : ScriptableObject, IScriptableData
    {
        [SerializeField]
        private bool enable;
        
        [SerializeField]
        private string value;

        public bool Enable => enable;
        public string Value => value;
    }
}
