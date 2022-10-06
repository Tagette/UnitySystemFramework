using System;

namespace UnitySystemFramework.Core
{
    public static partial class Game
    {
        [ThreadStatic]
        private static IGame _currentGame;

        /// <summary>
        /// The instance of the current name. Is only available for use within System code.
        /// </summary>
        public static IGame CurrentGame => _currentGame;

        /// <summary>
        /// Sets the current game and returns the one being replaced.
        /// </summary>
        public static IGame SetGame(IGame game)
        {
            var previous = _currentGame;
            _currentGame = game;
            return previous;
        }
    }
}
