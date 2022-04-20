using NUnit.Framework;
using UnityPatterns;

public class SingletonSimpleTest
{

    internal class MySingleton : Singleton<MySingleton>
    {

    }

    // A Test behaves as an ordinary method
    [Test]
    public void TestIfSingletonInstanceIsCreated()
    {
        Assert.NotNull(MySingleton.Instance);
        Assert.IsInstanceOf<MySingleton>(MySingleton.Instance);
    }
}
