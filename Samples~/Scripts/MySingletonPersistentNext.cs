using UnityEngine;
using UnityPatterns.Singleton.Attributes;
using UnityPatterns.Singleton;

namespace UnityPatterns.Samples
{
    /// <summary>
    /// An usage example of <see cref="SingletonPersistent{T}"/> implementation
    /// </summary>
    [SingletonSettings(CopyFieldsValues = true)]
    public class MySingletonPersistentNext : SingletonPersistent<MySingletonPersistentNext>
    {

        [SerializeField]
        protected GameObject gameObjReference;

        [SerializeField]
        protected OptionExample optionsEnumExample = OptionExample.NONE;

        public GameObject GameObjReference => gameObjReference;
        public OptionExample Options => optionsEnumExample;

        /// <summary>
        /// Duplication of <see cref="MySingletonPersistentPrevious.Awake"/> here to avoid
        /// a deeper inheritance of <seealso cref="SingletonPersistent{T}"/>
        /// </summary>
        protected override void Awake()
        {
            // Simulate a gameObject reference from the current scene
            // This is used for unit testing verifications
            AddGameObjectToScene();

            optionsEnumExample = OptionExample.ONE;

            base.Awake();
        }

        public void MyMethod()
        {
            Debug.Log($"Called from: \"{GetType().Name}\" singleton");
        }
        private void AddGameObjectToScene()
        {
            if (gameObjReference != null)
            {
                return;
            }
            
            var obj = new GameObject
            {
                name = nameof(gameObjReference)
            };
                
            gameObjReference = obj;
        }
    }
}
