using System.IO;
using System.Linq;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityPatterns.Samples;
using UnityPatterns.Singleton;

namespace UnityPatterns.Tests
{
    public class MySingletonPersist : SingletonPersistent<MySingletonPersist>
    {

    }

    [TestFixture]
    public class SingletonTest
    {
        internal class MySingleton : Singleton<MySingleton>
        {

        }

        private const string SAMPLES_FOLDER = "Samples";
        private const string PACKAGE_PATH = "Packages/com.mfdeveloper.unitypatterns";
        private const string NEW_SCENE_NAME = "Test";
        private const string PERSISTENT_SCENE_NAME = "SingletonPersistent";

        private string samplesScenesFolder = string.Empty;
        private string persistentScenePath = string.Empty;
        private string newScenePath = string.Empty;

        [OneTimeSetUp]
        public void SetUp()
        {
            SetupPaths();
        }
        
        private void SetupPaths()
        {
            /*
             * PS: When running tests on mobile platforms (e.g Android), should load scenes
             *      by name only (e.g `SceneManager.LoadSceneAsync("SceneName")`);
             */
            if (Application.isMobilePlatform)
            {
                persistentScenePath = PERSISTENT_SCENE_NAME;
                newScenePath = NEW_SCENE_NAME;
            }
            else
            {
                if (Directory.Exists($"{PACKAGE_PATH}/{SAMPLES_FOLDER}"))
                {
                    samplesScenesFolder = $"{PACKAGE_PATH}/{SAMPLES_FOLDER}/{PERSISTENT_SCENE_NAME}";
                }
                else if (Directory.Exists($"{PACKAGE_PATH}/{SAMPLES_FOLDER}~"))
                {
                    samplesScenesFolder = $"{PACKAGE_PATH}/{SAMPLES_FOLDER}~/{PERSISTENT_SCENE_NAME}";
                }

                persistentScenePath = $"{samplesScenesFolder}/{PERSISTENT_SCENE_NAME}";
                newScenePath = $"{samplesScenesFolder}/{NEW_SCENE_NAME}";
            }
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest, Description("Test if can get a singleton component in the scene")]
        public IEnumerator TestIfSingletonInstanceExistsInTheScene()
        {
            _ = new GameObject().AddComponent<MySingleton>();
            var singletonInScene = Object.FindObjectOfType<MySingleton>();

            Assert.IsInstanceOf<MySingleton>(singletonInScene);
            Assert.AreSame(singletonInScene, MySingleton.Instance);

            yield return null;
        }

        /// <summary>
        /// Test that loads a new scene asynchronously, wait and check if an object persists among scenes 
        /// </summary>
        /// <remarks>
        /// <b> References </b>
        /// <br/>
        /// <ul>
        ///     <li>
        ///         <a href="https://docs.unity3d.com/Manual/upm-assets.html">
        ///         Accessing package assets
        ///         </a>
        ///     </li>
        /// </ul>
        /// </remarks>
        /// <returns></returns>
        [UnityTest, Description("Test if a singleton remains active after loads a new scene")]
        public IEnumerator TestSingletonShouldPersistAmongScenes()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.

            var singleton = new GameObject().AddComponent<MySingletonPersist>();

            var asyncOp = SceneManager.LoadSceneAsync(newScenePath);
            yield return new WaitUntil(() => asyncOp.isDone);

            var currentScene = SceneManager.GetActiveScene();

            Assert.AreEqual(currentScene.name, NEW_SCENE_NAME);

            var persistentObj = Object.FindObjectOfType<MySingletonPersist>(true);

            Assert.NotNull(persistentObj);
            Assert.IsInstanceOf<MySingletonPersist>(persistentObj);
            Assert.AreSame(singleton, persistentObj);

            Object.Destroy(persistentObj.gameObject);
        }

        [UnityTest]
        [Description("Test if a singleton remains UNIQUE and keep the previous serialized values after loads a new scene")]
        public IEnumerator TestSingletonShouldPersistUniqueAndDestroyPreviousInstance()
        {

            MySingletonPersistentPrevious.Instance.MyMethod();
            var previousGameObjectReference = MySingletonPersistentPrevious.Instance.GameObjReference;
            Object.DontDestroyOnLoad(previousGameObjectReference);

            var asyncOp = SceneManager.LoadSceneAsync(persistentScenePath);
            yield return new WaitUntil(() => asyncOp.isDone);

            var currentScene = SceneManager.GetActiveScene();

            Assert.AreEqual(currentScene.name, PERSISTENT_SCENE_NAME);

            var persistentObjs = Object.FindObjectsOfType<MySingletonPersistentPrevious>();
            var persistentInstance = persistentObjs.FirstOrDefault();

            Assert.AreEqual(persistentObjs.Length, 1);
            Assert.IsInstanceOf<MySingletonPersistentPrevious>(persistentInstance);
            Assert.AreSame(MySingletonPersistentPrevious.Instance, persistentInstance);

            Assert.True(persistentInstance != null && persistentInstance.GameObjReference != null);
            Assert.IsNotNull(previousGameObjectReference);
            
            Assert.AreNotSame(previousGameObjectReference, persistentInstance.GameObjReference);
            Assert.AreEqual(OptionExample.TWO, persistentInstance.Options);

            Object.Destroy(persistentInstance.gameObject);
        }

        [UnityTest]
        [Description("Test if a singleton remains UNIQUE and keep the next/current serialized values after loads a new scene")]
        public IEnumerator TestSingletonShouldPersistUniqueAndDestroyNextInstance()
        {

            MySingletonPersistentNext.Instance.MyMethod();
            var previousGameObjectReference = MySingletonPersistentNext.Instance.GameObjReference;
            Object.DontDestroyOnLoad(previousGameObjectReference);

            var asyncOp = SceneManager.LoadSceneAsync(persistentScenePath);
            yield return new WaitUntil(() => asyncOp.isDone);

            var currentScene = SceneManager.GetActiveScene();

            Assert.AreEqual(currentScene.name, PERSISTENT_SCENE_NAME);

            var persistentObjs = Object.FindObjectsOfType<MySingletonPersistentNext>();
            var persistentInstance = persistentObjs.FirstOrDefault();

            Assert.AreEqual(persistentObjs.Length, 1);
            Assert.IsInstanceOf<MySingletonPersistentNext>(persistentInstance);
            Assert.AreSame(MySingletonPersistentNext.Instance, persistentInstance);

            Assert.True(persistentInstance != null && persistentInstance.GameObjReference != null);
            Assert.True(previousGameObjectReference != null);

            Assert.AreNotSame(previousGameObjectReference, persistentInstance.GameObjReference);
            Assert.AreEqual(OptionExample.ONE, persistentInstance.Options);

            Object.Destroy(persistentInstance.gameObject);
        }
    }
}
