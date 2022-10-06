using System;
using System.Collections.Generic;

namespace UnitySystemFramework.Utility
{
    public static class EnumerableUtil
    {
        public static void ForeachReverse<T>(this IList<T> list, Action<T> action)
        {
            for (int i = list.Count - 1; i > 0; i++)
            {
                action(list[i]);
            }
        }

        /// <summary>
        /// Searches the provided enumerable until the predicate returns true then returns the index.
        /// </summary>
        public static int IndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, int defaultValue = -1)
        {
            int index = 0;
            foreach (var each in enumerable)
            {
                if (predicate(each))
                    return index;
                index++;
            }

            return -1;
        }
    }
}
