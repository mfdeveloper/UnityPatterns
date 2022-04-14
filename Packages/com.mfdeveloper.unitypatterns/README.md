# Unity Patterns

Unity Design Patterns implementations, to shared across projects as an [UPM](https://docs.unity3d.com/Manual/cus-layout.html) package. Below you can see which patterns are implemented until here

## Unity: Singleton

Unity Singleton Monobehaviour component, that can be attached to Game Objects. You can use `SingletonPersistent` to persist the instance among scenes
or just `Singleton` class to use the same instance on the only one scene.

### Main use cases

- Managers that should use the same instance among scripts (e.g GameManager, ScoreManager, InputManager...)
- When you need use any component that depends of a Game object in the scene (e.g access `AudioSource` inside of an singleton)

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

## References

- [mstevenson/MonoBehaviourSingleton.cs](https://gist.github.com/mstevenson/4325117)

> The implementation here was based in this gist above!!

- [Design Pattern: Singletons in Unity](https://www.youtube.com/watch?v=Ova7l0UB26U)