using NUnit.Framework;
using UnityEngine;
using UnityPatterns;

public class FactoryComponentTest
{

    internal interface IMyComponent
    {

    }

    internal class MyScriptComponent : MonoBehaviour, IMyComponent
    {

    }

    // A Test behaves as an ordinary method
    [Test]
    public void FactoryComponentTestSimplePasses()
    {
        var _ = new GameObject().AddComponent<MyScriptComponent>();
        var myComponent = FactoryComponent.Get<IMyComponent>();

        Assert.NotNull(myComponent);
        Assert.IsInstanceOf<MyScriptComponent>(myComponent);
    }
}
