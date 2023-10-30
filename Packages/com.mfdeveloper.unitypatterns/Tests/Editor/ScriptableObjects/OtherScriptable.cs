using UnityEngine;

namespace UnityPatterns
{
    public interface IOtherScriptable
    {
        public string MyProperty { get; set; }
        void Init();
    }

    // Have to follow the class name convention with same name
    // of the interface, without the "I" prefix
    [CreateAssetMenu(fileName = "OtherScriptable", menuName = "UnityPatterns/Assets/Test/OtherScriptable")]
    public class OtherScriptable : ScriptableObject, IOtherScriptable
    {
        public string MyProperty { get; set; }

        public void Init()
        {
            MyProperty = "value";
        }
    }
}
