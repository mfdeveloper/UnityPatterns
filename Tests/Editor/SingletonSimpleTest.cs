using NUnit.Framework;
using UnityPatterns.Singleton;

namespace UnityPatterns.Editor
{

    /// <summary>
    /// A basic unit test under Unity Editor.
    /// You can use this class as a base for super fast unit tests
    /// </summary>
    /// <remarks>
    /// <b>Reference:</b>
    /// <a href="https://www.raywenderlich.com/9454-introduction-to-unity-unit-testing">Introduction To Unity Unit Testing</a>
    /// </remarks>
    [TestFixture]
    public class SingletonSimpleTest
    {

        internal class MySingleton : Singleton<MySingleton>
        {

        }

        // A Test behaves as an ordinary method
        [Test, Description("Test if a singleton instance is accessed from the static property 'Instance'")]
        public void TestIfSingletonInstanceIsCreated()
        {
            Assert.NotNull(MySingleton.Instance);
            Assert.IsInstanceOf<MySingleton>(MySingleton.Instance);
        }
    }
}
