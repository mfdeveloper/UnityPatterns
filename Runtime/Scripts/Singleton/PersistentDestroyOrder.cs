namespace UnityPatterns.Singleton
{
    /// <summary>
    /// Identify which instance of <see cref="SingletonPersistent{T}"/> should be destroyed
    /// </summary>
    public enum PersistentDestroyOrder
    {
        PREVIOUS,
        NEXT
    }
}