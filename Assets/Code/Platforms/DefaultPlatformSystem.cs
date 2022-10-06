using UnitySystemFramework.Core;

namespace UnitySystemFramework.Platforms
{
    public class DefaultPlatformSystem : BaseSystem, IPlatformSystem
    {
        public PlatformSDK SDK => PlatformSDK.None;

        protected override void OnInit()
        {
        }

        protected override void OnStart()
        {
        }

        protected override void OnEnd()
        {
        }
    }
}
