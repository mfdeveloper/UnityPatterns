# Unity Patterns

**Unity version:** `2021.3.7f1`

Unity Design Patterns implementations, to shared across projects as an [UPM](https://docs.unity3d.com/Manual/cus-layout.html) package. Below you can see which patterns are implemented until here

## Unity: Singleton

Unity Singleton Monobehaviour component, that can be attached to Game Objects. You can use `SingletonPersistent` to persist the instance among scenes
or just `Singleton` class to use the same instance on the only one scene.

### Main use cases

- Managers that should use the same instance among scripts (e.g GameManager, ScoreManager, InputManager...)

- When you need use any component that depends of a Game object in the scene (e.g access `AudioSource` inside of an singleton)

- When you need a Singleton with Unity messages like `Start()`, and/or access some objects that are only available after the game starts
  > (e.g Audio Middlewares, Third Party packages/libraries...)

> Consider use a [ScriptableObject](https://docs.unity3d.com/Manual/class-ScriptableObject.html) instead

### Getting started

Create a script that inherits from `Singleton<MyScript>` passing the script class by generics:

```csharp
public class GameManager : Singleton<GameManager> {
  ...
}
```

In any other script, access the `Instance` property. The value of this property should be equal from any script:

```csharp
public class PlayerController : Monobehaviour {
  
  private void Awake() {
      // Get the singleton instance
      var gameManager = GameManager.Instance;
  }
}
```

### Persistent Singleton

If you wish a singleton that persists among scenes, you can create a class that inherit from `SingletonPersistent`:

```csharp
public class GameManager : SingletonPersistent<GameManager> {
  ...
}
```

By default, when a new scene is loaded and there is the same game object with the same script component (e.g `GameManager` above), the previous instance from the previous scene will be destroyed and will remains just one instance under **`DontDestroyOnload`** Unity scene.

### Persistent Singleton: Optional settings

Optionally, it's possible pass custom configurations to a `SingletonPersistent` script with `SingletonSettings` **_C#_** attribute:

```csharp
// Here if the same gameObject with the same script 
// exists in another scene, the next one will be
// destroyed and the GameObject reference fields
// will be copied to the previous 
[SingletonSettings(CopyFieldsValues = true, DestroyGameObject = PersistentDestroyOrder.NEXT)]
public class GameManager : SingletonPersistent<GameManager> {
  ...
}
```

> For more details, see `Tests/Runtime/Examples` scripts examples

## Unity: Factory Method

A base Factory Method implementation for Unity. The main use case here is to use this to access a gameObject in the scene that contains a script that implements an `C#` interface:

```csharp

// Create a C# interface
public interface IMyComponent
{

}

// Create a MonoBehaviour script that implements the interface above, and attach it to a gameObject in the scene
public class MyScript : MonoBehaviour, IMyComponent
{

}

// Example to access the a gameObject script that implements an interface
using System.Linq;
using UnityEngine;
using UnityPatterns;

public class ExampleScript : MonoBehaviour
{
    public GameObject[] rootsFromDontDestroyOnLoad;
    void Start()
    {
        IMyComponent myComponent = FactoryComponent.Get<IMyComponent>();

        // (Optional) You can get the all gameObjects with a script that implements an interface
        List<IMyComponent> myComponent = FactoryComponent.GetList<IMyComponent>();

        Debug.Log($"The component is: {myComponent.GetType().Name}") // Prints: MyScript
    }
}
```

Also, it's possible get a `ScriptableObject` instance from an interface or a class:

```csharp
// Create a C# interface
public interface IMyScriptable
{

}

// Create a MonoBehaviour script that implements the interface above, and attach it to a gameObject in the scene
[CreateAssetMenu(fileName = "MyScriptable", menuName = "Data/Samples/MyScriptable")]
public class MyScriptable : ScriptableObject, IMyScriptable
{

}

// Example to access the a gameObject script that implements an interface
using System.Linq;
using UnityEngine;
using UnityPatterns;

public class ExampleScript : MonoBehaviour
{
    public GameObject[] rootsFromDontDestroyOnLoad;
    void Start()
    {
        // Get a ScriptableObject instance from an interface
        IMyScriptable myScriptable = FactoryComponent.Get<IMyScriptable>();

        // Get a ScriptableObject instance from a class
        MyScriptable myScriptable = FactoryComponent.Get<MyScriptable>();


        Debug.Log($"The component is: {myScriptable.GetType().Name}") // Prints: MyScriptable
    }
}
```

### Main use cases

- Get singleton managers from all scenes active scenes (including `DontDestroyOnLoad` automatic scene created by Unity).

- Get any **gameObject** from scenes that contains a script that implements an `C#` interface.

- Get a `ScriptableObject` that implements an `C#` interface or a from class reference. The last one is great to get an instance that automatically call `Init()` method.

## Code templates

Under `Samples~` folder, this package share some code templates to easily create singleton classes from Unity Editor.

To use that, follow the steps below:

1. On Unity Editor, click on `Window` => `Package Manager`
2. On the opened window, find the package `Unity Design Patterns...` and import the sample: **ScriptTemplates**
3. Move the imported folder `ScriptTemplates` to your root `Assets` folder in your Unity game project.
4. Restart the Editor
5. Openup again, and press right click on any folder of your game project, and check if appears: `Create` => `Custom Templates` => `UnitySingleton` :)

## References

- [mstevenson/MonoBehaviourSingleton.cs](https://gist.github.com/mstevenson/4325117)

> The implementation here was based in this gist above!!

- [Design Pattern: Singletons in Unity](https://www.youtube.com/watch?v=Ova7l0UB26U)
