using UnitySystemFramework.Core;

namespace UnitySystemFramework.Platforms
{
    public interface IPlatformSystem : ISystem
    {
        PlatformSDK SDK { get; }
    }
}
