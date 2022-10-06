using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.Profiling;

namespace UnitySystemFramework.Core
{
    public struct DisposableSample : IDisposable
    {
        public void Dispose()
        {
            Profiler.EndSample();
        }
    }

    public partial interface IGame
    {
        /// <summary>
        /// Begins a profiler sample. The return value can be used in a using statement where EndSample will be called when it's disposed.
        /// </summary>
        DisposableSample BeginSample(string name);

        /// <summary>
        /// Ends a previously started profiler sample.
        /// </summary>
        void EndSample();
    }

    public static partial class Game
    {
    }

    public abstract partial class BaseGame
    {
        /// <inheritdoc cref="IGame.BeginSample(string)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public DisposableSample BeginSample(string name)
        {
            Profiler.BeginSample(name ?? "NULL");

            return default;
        }

        /// <inheritdoc cref="IGame.EndSample()"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public void EndSample()
        {
            Profiler.EndSample();
        }
    }
}
