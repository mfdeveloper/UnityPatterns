using NUnit.Framework;
using UnityEngine;

namespace UnityPatterns.Editor
{
    public class FactoryComponentTest
    {

        internal interface IMyComponent
        {

        }

        internal class MyScript : MonoBehaviour, IMyComponent
        {

        }

        // A Test behaves as an ordinary method
        [Test]
        public void TestGetScriptComponentByInterface()
        {
            var addedGameObj = new GameObject().AddComponent<MyScript>();
            var myComponent = FactoryComponent.Get<IMyComponent>();

            Assert.NotNull(myComponent);
            Assert.IsInstanceOf<MyScript>(myComponent);

            Object.DestroyImmediate(addedGameObj.gameObject);
        }
    }
}
