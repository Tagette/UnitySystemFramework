using UnityEngine.LowLevel;

namespace UnitySystemFramework.Utility
{
    public static class PlayerLoopUtils
    {
        public static PlayerLoopSystem FindSystem<T>(this PlayerLoopSystem playerLoop) where T : struct
        {
            if (playerLoop.type == typeof(T))
            {
                return playerLoop;
            }

            if (playerLoop.subSystemList != null)
            {
                foreach (var subSystem in playerLoop.subSystemList)
                {
                    var found = FindSystem<T>(subSystem);
                    if (found.type == typeof(T))
                        return found;
                }
            }

            return default;
        }
    }
}
