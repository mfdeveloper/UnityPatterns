using NUnit.Framework;
using UnityEngine;

namespace UnityPatterns.Editor
{
    public class FactoryComponentTest
    {

        internal interface IMyComponent
        {

        }

        internal interface IMyAnotherComponent
        {

        }

        internal class MyScript : MonoBehaviour, IMyComponent, IMyAnotherComponent
        {

        }

        // A Test behaves as an ordinary method
        [Test]
        public void TestGetScriptComponent()
        {
            var addedGameObj = new GameObject().AddComponent<MyScript>();

            // Lookup by an C# Interface
            var myComponentFromInterface = FactoryComponent.Get<IMyComponent>();

            // Lookup the same component, but now by a class.
            // The only difference here is that a warning is showed in the Unity console
            // to prefer use Object.GetComponent() methods
            var myComponentFromClass = FactoryComponent.Get<MyScript>();

            Assert.NotNull(myComponentFromInterface);
            Assert.IsInstanceOf<MyScript>(myComponentFromInterface);

            Assert.NotNull(myComponentFromClass);
            Assert.IsInstanceOf<MyScript>(myComponentFromClass);

            // Make sure that "cache" references are stored
            Assert.AreEqual(1, FactoryComponent.ComponentsInstances.Count);
            Assert.True(FactoryComponent.ContainsInstance(myComponentFromInterface));

            Object.DestroyImmediate(addedGameObj.gameObject);
        }

        [Test]
        public void TestTryGetScriptComponentFromDistinctInterface()
        {
            var addedGameObj = new GameObject().AddComponent<MyScript>();

            // Lookup by an C# Interface
            var myComponent = FactoryComponent.Get<IMyComponent>();
            var myAnotherComponent = FactoryComponent.Get<IMyAnotherComponent>();

            Assert.NotNull(myComponent);
            Assert.IsInstanceOf<MyScript>(myComponent);

            Assert.AreEqual(1, FactoryComponent.ComponentsInstances.Count);
            Assert.True(FactoryComponent.ContainsInstance(myAnotherComponent));

            Object.DestroyImmediate(addedGameObj.gameObject);
        }
    }
}
