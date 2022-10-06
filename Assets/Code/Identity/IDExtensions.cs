using UnitySystemFramework.Core;
using UnitySystemFramework.Identity;

public static class IDExtensions
{
    /// <inheritdoc cref="IDSystem.GetID{T}(T)" />
    public static ID<T> GetID<T>(this T value)
    {
        var game = Game.CurrentGame;
        var idSystem = game.GetSystem<IDSystem>();
        return (ID<T>)idSystem.GetID(value);
    }
}
