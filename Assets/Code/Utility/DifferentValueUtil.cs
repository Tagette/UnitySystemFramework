using UnityEngine;
using System.Runtime.CompilerServices;

namespace UnitySystemFramework.Utility
{
    public static class DifferentValueUtil
    {
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        static public bool Different2DMapCoordinate(Vector3 a, Vector3 b)
        {
            return
                DifferentFloat(a.x, b.x)
                ||
                DifferentFloat(a.z, b.z);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        static public bool DifferentFloat(float a, float b)
        {
            return Mathf.Abs(a - b) > 0.0001f;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        static public bool DifferentRotation(Quaternion a, Quaternion b)
        {
            return Quaternion.Dot(a, b) < 0.9999f;
        }
    }
}
