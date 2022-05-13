using UnityEngine;

namespace UnityPatterns.Editor
{

    public interface IMyScriptable
    {

    }

    public interface IMyManagerScriptable
    {

    }

    [CreateAssetMenu(fileName = "MyScriptable", menuName = "Data/Samples/MyScriptable")]
    public class MyScriptable : ScriptableObject, IMyScriptable, IMyManagerScriptable
    {
        
    }
}
