using System;

namespace UnitySystemFramework.Core
{
    public partial interface IGame
    {
        /// <summary>
        /// The version of the game.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Determines if the game has finished initialization.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Determines if the game is currently initializing.
        /// </summary>
        bool IsInitializing { get; }

        /// <summary>
        /// Determines if the game is currently starting.
        /// </summary>
        bool IsStarting { get; }

        /// <summary>
        /// Determines if the game has started.
        /// </summary>
        bool IsStarted { get; }

        /// <summary>
        /// Determines if the game is currently ending.
        /// </summary>
        bool IsEnding { get; }

        /// <summary>
        /// Determines if the game is currently exiting.
        /// </summary>
        bool IsExiting { get; }

        /// <summary>
        /// Initializes the game, finds and preps all events types, and calls init on each system added.
        /// Also calls BaseGame.OnInit().
        /// </summary>
        void Init();

        /// <summary>
        /// Calls OnStart() on each system.
        /// </summary>
        void Start();

        /// <summary>
        /// Calls OnUpdate() on each system.
        /// </summary>
        void Update();

        /// <summary>
        /// Calls OnFixedUpdate() on each system.
        /// </summary>
        void FixedUpdate();

        /// <summary>
        /// Calls OnLateUpdate() on each system.
        /// </summary>
        void LateUpdate();

        /// <summary>
        /// Calls OnEnd() on each system and resets the game to be used again.
        /// Also ends the current state and it's systems.
        /// Calling this will begin the ending process. The game-end will be finalized after all updates have completed.
        /// See <see cref="IsEnding"/> to determine when the game has finished it's ending process. 
        /// </summary>
        void End();

        /// <summary>
        /// Ends the game and closes the application. Also stops the editor from playing.
        /// </summary>
        void Exit();
    }
}
