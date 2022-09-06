using UnityEngine;

namespace UnityPatterns.Factory
{
    public class FactoryCleaner : MonoBehaviour
    {
        private void OnDestroy()
        {
            FactoryComponent.Cleanup(this);
        }
    }
}
