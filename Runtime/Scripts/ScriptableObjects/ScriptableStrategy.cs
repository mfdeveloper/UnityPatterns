using UnityEngine;

namespace UnityPatterns.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ScriptableStrategy", menuName = "UnityPatterns/Assets/ScriptableStrategy")]
    public class ScriptableStrategy : ScriptableObject
    {
        [SerializeField]
        private string value = string.Empty;

        public string Value => value;
    }
}
