using UnityEngine;

namespace UnityPatterns
{
    public interface IOtherScriptable
    {

    }

    // Have to follow the class name convention with same name
    // of the interface, without the "I" prefix
    [CreateAssetMenu(fileName = "OtherScriptable", menuName = "Data/Samples/OtherScriptable")]
    public class OtherScriptable : ScriptableObject, IOtherScriptable
    {
        
    }
}
