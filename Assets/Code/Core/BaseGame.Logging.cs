using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Debug = UnityEngine.Debug;

namespace UnitySystemFramework.Core
{
    public partial interface IGame
    {
        /// <summary>
        /// Logs a message to the console and log file.
        /// </summary>
        void Log(object message);

        /// <summary>
        /// Logs a warning message to the console and log file.
        /// </summary>
        void LogWarning(object message);

        /// <summary>
        /// Logs an error message to the console and log file.
        /// </summary>
        void LogError(object message);

        /// <summary>
        /// Logs an exception to the console and log file.
        /// </summary>
        void LogException(Exception ex);
    }

    public static partial class Game
    {
        /// <inheritdoc cref="IGame.Log(object)"/>
        public static void Log(object message)
        {
            if (_currentGame != null)
                _currentGame.Log(message);
            else
                Debug.Log(message);
        }

        /// <inheritdoc cref="IGame.LogWarning(object)"/>
        public static void LogWarning(object message)
        {
            if (_currentGame != null)
                _currentGame.LogWarning(message);
            else
                Debug.LogWarning(message);
        }

        /// <inheritdoc cref="IGame.LogError(object)"/>
        public static void LogError(object message)
        {
            if (_currentGame != null)
                _currentGame.LogError(message);
            else
                Debug.LogError(message);
        }

        /// <inheritdoc cref="IGame.LogException(Exception)"/>
        public static void LogException(Exception ex)
        {
            if (_currentGame != null)
                _currentGame.LogException(ex);
            else
                Debug.LogException(ex);
        }
    }

    public abstract partial class BaseGame
    {
        /// <inheritdoc cref="IGame.Log(object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public void Log(object message)
        {
            Debug.Log(message);
        }

        /// <inheritdoc cref="IGame.LogWarning(object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public void LogWarning(object message)
        {
            Debug.LogWarning(message);
        }

        /// <inheritdoc cref="IGame.LogError(object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public void LogError(object message)
        {
            Debug.LogError(message);
        }

        /// <inheritdoc cref="IGame.LogException(Exception)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public void LogException(Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}
