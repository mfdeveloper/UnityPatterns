using System.Linq;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityPatterns.Singleton;
using UnityPatterns.Examples;

namespace UnityPatterns
{
    public class MySingletonPersist : SingletonPersistent<MySingletonPersist>
    {

    }

    public class SingletonTest
    {
        internal class MySingleton : Singleton<MySingleton>
        {

        }

        private const string SCENES_FOLDER = "Scenes";
        private const string NEW_SCENE_NAME = "TestScene";
        private const string PERSISTENT_SCENE_NAME = "SingletonPersistentScene";

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest, Description("Test if can get a singleton component in the scene")]
        public IEnumerator TestIfSingletonInstanceExistsInTheScene()
        {
            var _ = new GameObject().AddComponent<MySingleton>();
            var singletonInScene = Object.FindObjectOfType<MySingleton>();

            Assert.IsInstanceOf<MySingleton>(singletonInScene);
            Assert.AreSame(singletonInScene, MySingleton.Instance);

            yield return null;
        }


        [UnityTest, Description("Test if a singleton remains active after loads a new scene")]
        public IEnumerator TestSingletonShouldPersistAmongScenes()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.

            MySingletonPersist singleton = new GameObject().AddComponent<MySingletonPersist>();

            var asyncOp = SceneManager.LoadSceneAsync($"{SCENES_FOLDER}/{NEW_SCENE_NAME}");
            yield return new WaitUntil(() => asyncOp.isDone);

            var currentScene = SceneManager.GetActiveScene();

            Assert.AreEqual(currentScene.name, NEW_SCENE_NAME);

            var persistentObj = Object.FindObjectOfType<MySingletonPersist>(true);

            Assert.NotNull(persistentObj);
            Assert.IsInstanceOf<MySingletonPersist>(persistentObj);
            Assert.AreSame(singleton, persistentObj);

            Object.Destroy(persistentObj.gameObject);
        }

        [UnityTest, Description("Test if a singleton remains UNIQUE after loads a new scene")]
        public IEnumerator TestSingletonShouldPersistUnique()
        {

            MySingletonUniquePersistent.Instance.MyMethod();

            var asyncOp = SceneManager.LoadSceneAsync($"{SCENES_FOLDER}/{PERSISTENT_SCENE_NAME}");
            yield return new WaitUntil(() => asyncOp.isDone);

            var currentScene = SceneManager.GetActiveScene();

            Assert.AreEqual(currentScene.name, PERSISTENT_SCENE_NAME);

            var persistentObjs = Object.FindObjectsOfType<MySingletonUniquePersistent>();

            Assert.AreEqual(persistentObjs.Length, 1);
            Assert.IsInstanceOf<MySingletonUniquePersistent>(persistentObjs.FirstOrDefault());
            Assert.AreSame(MySingletonUniquePersistent.Instance, persistentObjs.FirstOrDefault());

            Assert.True(persistentObjs.FirstOrDefault().GameObjReference != null);

            Object.Destroy(persistentObjs.FirstOrDefault().gameObject);
        }
    }
}
