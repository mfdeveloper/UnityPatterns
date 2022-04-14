# Unity Patterns

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

### Code templates

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
