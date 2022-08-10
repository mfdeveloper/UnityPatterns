using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityPatterns.Singleton;
using UnityPatterns.Factory;
using UnityPatterns.Util;

namespace UnityPatterns
{

    public class FactoryComponentTest
    {

        MyScriptPersistent persistentObj;
        DontDestroyOnLoadManager dontDestroyOnLoadObj;

        internal interface IMyComponent
        {

        }

        internal class MyScriptPersistent : SingletonPersistent<MyScriptPersistent>, IMyComponent
        {

        }

        [SetUp]
        public void SetUp()
        {
            persistentObj = new GameObject().AddComponent<MyScriptPersistent>();
            dontDestroyOnLoadObj = new GameObject().AddComponent<DontDestroyOnLoadManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(persistentObj.gameObject);
            Object.Destroy(dontDestroyOnLoadObj.gameObject);
        }

        [UnityTest, Description("Test get all gameObjects from all scenes, including DontDestroyOnLoad Unity scene")]
        public IEnumerator TestIfComponentIsOnDontDestroyOnloadScene()
        {

            var rootGameObjects = FactoryComponent.GetAllRootObjects();

            Assert.Greater(rootGameObjects.Count, 0);
            Assert.Contains(persistentObj.gameObject, rootGameObjects);

            yield return null;
        }

        [UnityTest, Description("Test if get a component from a gameObject marked with Object.DontDestroyOnLoad")]
        public IEnumerator TestGetScriptComponentMarkedAsPersistent()
        {

            var myComponent = FactoryComponent.Get<IMyComponent>();

            Assert.NotNull(myComponent);
            Assert.IsInstanceOf<MyScriptPersistent>(myComponent);

            yield return null;
        }
    }
}
