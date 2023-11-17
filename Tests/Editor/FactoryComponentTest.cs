using NUnit.Framework;
using UnityEngine;
using UnityPatterns.Factory;
using UnityPatterns.Factory.Attributes;

namespace UnityPatterns.Editor
{
    [TestFixture]
    public class FactoryComponentTest
    {

        internal interface IMyComponent
        {

        }

        internal interface IMyAnotherComponent
        {

        }

        [FactoryReference]
        public class MyScript : MonoBehaviour, IMyComponent, IMyAnotherComponent
        {

        }

        [SetUp]
        public void SetUp()
        {
            FactoryComponent.Cleanup();
        }

        [TearDown]
        public void TearDown()
        {
            FactoryComponent.Cleanup();   
        }

        // A Test behaves as an ordinary method
        [Test, Description("Test if get a component from a interface properly")]
        public void TestGetScriptComponent()
        {
            var gameObj = new GameObject();
            var addedGameObj = gameObj.AddComponent<MyScript>();

            // Lookup by an C# Interface
            var myComponentFromInterface = FactoryComponent.Get<IMyComponent>();

            // Lookup the same component, but now by a class.
            // Will be returned here from "cache" memory
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

        [Test, Description("Test if get a component that implements 2 distinct interfaces")]
        public void TestTryGetScriptComponentFromDistinctInterface()
        {
            var gameObj = new GameObject();
            var addedGameObj = gameObj.AddComponent<MyScript>();

            // Lookup by an C# Interface
            var myComponent = FactoryComponent.Get<IMyComponent>();
            var myAnotherComponent = FactoryComponent.Get<IMyAnotherComponent>();

            Assert.NotNull(myComponent);
            Assert.IsInstanceOf<MyScript>(myComponent);

            Assert.AreEqual(1, FactoryComponent.ComponentsInstances.Count);
            Assert.True(FactoryComponent.ContainsInstance(myAnotherComponent));

            Object.DestroyImmediate(addedGameObj.gameObject);
        }

        [Test, Description("Test get a ScriptableObject that implements a interface with the same name, but that starts with 'I'")]
        public void TestTryGetAScriptableObjectWithConventionInterfaceName()
        {

            var myScriptable = FactoryComponent.Get<IMyScriptable>();

            Assert.NotNull(myScriptable);
            Assert.IsInstanceOf<MyScriptable>(myScriptable);

            FactoryComponent.Cleanup(myScriptable);
        }

        [Test, Description("Test if can get a ScriptableObject from 'Resources/ScriptableObjects' convention folder")]
        public void TestTryGetAScriptableObject()
        {

            var myScriptable = FactoryComponent.Get<IMyManagerScriptable>();

            Assert.NotNull(myScriptable);
            Assert.IsInstanceOf<MyScriptable>(myScriptable);
            Assert.AreEqual("CustomManager", ((ScriptableObject) myScriptable).name);

            FactoryComponent.Cleanup(myScriptable);
        }

        [Test, Description("Test if get a ScriptableObject without a bounded .asset file, and calls Init() method")]
        public void TestTryGetAScriptableObjectWithoutAssetWithInitMethod()
        {

            var otherScriptable = FactoryComponent.Get<IOtherScriptable>();

            Assert.NotNull(otherScriptable);
            Assert.IsInstanceOf<OtherScriptable>(otherScriptable);

            // Make sure that Init() method was called
            Assert.AreEqual("value", otherScriptable.MyProperty);

            FactoryComponent.Cleanup(otherScriptable);
        }
        
        [Test, Description("Test if get a ScriptableObject by class and calls Init() method")]
        public void TestTryGetAScriptableObjectByClass()
        {

            var otherScriptable = FactoryComponent.Get<OtherScriptable>();

            Assert.NotNull(otherScriptable);
            Assert.IsInstanceOf<OtherScriptable>(otherScriptable);

            // Make sure that Init() method was called
            Assert.AreEqual("value", otherScriptable.MyProperty);

            FactoryComponent.Cleanup(otherScriptable);
        }
    }
}
