using UnityEngine;
using UnityPatterns.Singleton;
using UnityPatterns.Singleton.Attributes;

namespace UnityPatterns.Examples
{
    /// <summary>
    /// An usage example of <see cref="SingletonPersistent{T}"/> implementation
    /// </summary>
    [SingletonSettings(CopySerializedFields = true)]
    public class MySingletonUniquePersistent : SingletonPersistent<MySingletonUniquePersistent>
    {
        [SerializeField]
        protected GameObject gameObjReference;

        public GameObject GameObjReference => gameObjReference;

        protected override void Awake()
        {
            // Simulate a gameObject reference from the current scene
            // This is used for unit testing verifications
            if (gameObjReference == null)
            {
                var obj = new GameObject();
                obj.name = nameof(gameObjReference);
                gameObjReference = obj;
            }

            base.Awake();
        }

        public void MyMethod()
        {
            Debug.Log($"Called from: \"{GetType().Name}\" singleton");
        }
    }
}
