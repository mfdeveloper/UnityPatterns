using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityPatterns.Extensions;

namespace UnityPatterns.Editor
{
    public class UnityObjectExtsTest
    {

        MyBehaviour myObj;

        internal class MyBehaviour : MonoBehaviour
        {
            
        }

        internal class MyScriptable : ScriptableObject
        {
            
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            myObj = new GameObject().AddComponent<MyBehaviour>();
        }
        
        [Test, Description("Test if get a TaskCompletion attached to a MonoBehaviour")]
        public async Task TestGetTaskCompletion()
        {
            var myScriptable = ScriptableObject.CreateInstance<MyScriptable>();
            
            var taskCompletion = myObj.GetTaskCompletion<MyScriptable>();
            var otherCompletion = myObj.GetTaskCompletion<MyScriptable>();
            
            // Change the TaskCompletionSource status.
            // The both instances should be exactly the same
            taskCompletion.TrySetResult(myScriptable);

            // Await for value inside of "TaskCompletionSource"
            var completedValue = await taskCompletion.Task;
             
            Assert.IsInstanceOf<TaskCompletionSource<MyScriptable>>(taskCompletion);
            Assert.AreSame(taskCompletion, otherCompletion);
            Assert.AreSame(completedValue, myScriptable);
            
            Assert.Greater(myObj.GetAllTaskCompletions<MyScriptable>().Count, 0);
            Assert.True(myObj.GetAllTaskCompletions<MyScriptable>().Any(pair => pair.Value == taskCompletion));
        }
    }
}
