

namespace UnitySystemFramework.Core
{
    public interface ISystem
    {
        /// <summary>
        /// The reference to the game.
        /// </summary>
        IGame Game { get; set; }

        /// <summary>
        /// Whether or not this system has been initialized yet or not.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Whether or not this system is enabled. Disabled systems do not get updated.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Called when the system initializes. Use this method to require/get any systems and cache them in a field.
        /// Any methods subscribed will be automatically removed when the system ends.
        /// </summary>
        void OnInit();

        /// <summary>
        /// Called after all systems have been initialized.
        /// </summary>
        void OnStart();

        /// <summary>
        /// Called when the system is enabled. Called after OnStart().
        /// </summary>
        void OnEnable();

        /// <summary>
        /// Called when the system is disabled. Called before OnEnd().
        /// </summary>
        void OnDisable();

        /// <summary>
        /// Called when the state ends. Any methods subscribed in OnInit() will be automatically removed.
        /// </summary>
        void OnEnd();
    }
}
