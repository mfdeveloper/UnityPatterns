using System.Collections;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;
using UnityPatterns.Util;
using UnityPatterns.Singleton;
using UnityPatterns.Factory;
using UnityPatterns.Tests.Examples.ScriptableObjects;
using UnityPatterns.YieldInstructions;

namespace UnityPatterns.Tests
{

    [TestFixture]
    public class FactoryComponentTest
    {

        MyScriptPersistent persistentObj;
        DontDestroyOnLoadManager dontDestroyOnLoadObj;

        private interface IMyComponent
        {

        }

        internal class MyScriptPersistent : SingletonPersistent<MyScriptPersistent>, IMyComponent
        {

        }

        [SetUp]
        public void SetUp()
        {
            FactoryComponent.Cleanup();
            
            persistentObj = new GameObject().AddComponent<MyScriptPersistent>();
            dontDestroyOnLoadObj = new GameObject().AddComponent<DontDestroyOnLoadManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(persistentObj.gameObject);
            Object.Destroy(dontDestroyOnLoadObj.gameObject);
            
            FactoryComponent.Cleanup();
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
        
        /// <summary>
        /// <b>PS:</b> Async <see cref="Task"/> don't use <see cref="UnityTestAttribute"/>, but should run on PlayMode
        /// </summary>
        /// <param name="labelOrAddress">Addressable path (e.g MyAddressableAssets/MyAsset) or a label/tag (my-asset)</param>
        [Test, Description("Test if fetch a ScriptableObject .asset using Addressables API that returns a async Task")]
        public async Task TestFetchScriptableObjectWithTask()
        {
            var myScriptableData = await FactoryComponent.FetchScriptableTask<IScriptableData>(strictInstance: false);

            Assert.NotNull(myScriptableData);
            Assert.IsInstanceOf<ScriptableObject>(myScriptableData);
            Assert.IsInstanceOf<ScriptableData>(myScriptableData);
            
            var msgLogRegex = new Regex($@"^\[{nameof(FactoryComponent)}\] Loaded addressable asset");
            LogAssert.Expect(LogType.Log, msgLogRegex);
        }
        
        [UnityTest, Description("Test if fetch a ScriptableObject .asset using Addressables API that returns an IEnumerator coroutine")]
        public IEnumerator TestFetchScriptableObjectWithCoroutines()
        {
            var assetOp = FactoryComponent.FetchScriptableAsync<IScriptableData>(strictInstance: false);
            
            // yielding when already done still waits until the next frame
            // so don't yield if done.
            if (!assetOp.IsDone)
            {
                yield return assetOp;
            }

            Assert.AreEqual(assetOp.Status, AsyncOperationStatus.Succeeded);
            Assert.IsInstanceOf<ScriptableObject>(assetOp.Result);
            Assert.IsInstanceOf<ScriptableData>(assetOp.Result);
            
            var msgLogRegex = new Regex($@"^\[{nameof(FactoryComponent)}\] Loaded addressable asset");
            LogAssert.Expect(LogType.Log, msgLogRegex);
        }
        
        /// <summary>
        /// <b>PS:</b> Async <see cref="Task"/> don't use <see cref="UnityTestAttribute"/>, but should run on PlayMode
        /// </summary>
        /// <param name="labelOrAddress">Addressable path (e.g MyAddressableAssets/MyAsset) or a label/tag (my-asset)</param>
        [Test, Description("Test if get/load async ScriptableObject .asset using Addressables API by address")]
        [TestCase("AddressableTests/Data")]
        [TestCase("scriptable-data")]
        public async Task TestGetScriptableObjectAssetByAddressWithTask(string labelOrAddress)
        {
            var myScriptableData = await FactoryComponent.GetTask<ScriptableData>(addressableAddress: labelOrAddress, strictInstance: false);

            Assert.NotNull(myScriptableData);
            Assert.IsInstanceOf<ScriptableObject>(myScriptableData);
            Assert.IsInstanceOf<ScriptableData>(myScriptableData);
            Assert.IsTrue(myScriptableData.Enable);
            Assert.AreEqual("test", myScriptableData.Value);
            
            var msgLogRegex = new Regex($@"^\[{nameof(FactoryComponent)}\] Loaded addressable asset");
            LogAssert.Expect(LogType.Log, msgLogRegex);
        }
        
        [UnityTest, Description("Test if get/load a ScriptableObject .asset using Addressables API that returns an IEnumerator coroutine")]
        public IEnumerator TestGetScriptableObjectAssetByTypeWithCoroutines()
        {
            var assetOp = FactoryComponent.GetAsync<ScriptableData>(strictInstance: false);
            
            // yielding when already done still waits until the next frame
            // so don't yield if done.
            if (!assetOp.IsDone)
            {
                yield return assetOp;
            }

            Assert.AreEqual(assetOp.Status, AsyncOperationStatus.Succeeded);
            Assert.IsInstanceOf<ScriptableObject>(assetOp.Result);
            Assert.IsInstanceOf<ScriptableData>(assetOp.Result);
            
            var msgLogRegex = new Regex($@"^\[{nameof(FactoryComponent)}\] Loaded addressable asset");
            LogAssert.Expect(LogType.Log, msgLogRegex);
        }
        
        [UnityTest, Description("Test WaitAsyncOperation custom yield instruction")]
        public IEnumerator TestWaitAsyncOperationCoroutine()
        {
            var assetOp = FactoryComponent.GetAsync<ScriptableData>(strictInstance: false);

            var waitAsyncOp = new WaitAsyncOperation<ScriptableData>
            {
                OpHandle = assetOp
            };

            yield return waitAsyncOp;

            Assert.AreEqual(waitAsyncOp.OpHandle.Status, AsyncOperationStatus.Succeeded);
            Assert.IsInstanceOf<ScriptableObject>(waitAsyncOp.OpHandle.Result);
            Assert.IsInstanceOf<ScriptableData>(waitAsyncOp.OpHandle.Result);
        }
    }
}
