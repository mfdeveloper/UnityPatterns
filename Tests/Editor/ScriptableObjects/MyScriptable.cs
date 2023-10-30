using UnityEngine;

namespace UnityPatterns.Editor
{

    public interface IMyScriptable
    {

    }

    public interface IMyManagerScriptable
    {

    }

    [CreateAssetMenu(fileName = "MyScriptable", menuName = "UnityPatterns/Assets/Test/MyScriptable")]
    public class MyScriptable : ScriptableObject, IMyScriptable, IMyManagerScriptable
    {
        
    }
}
