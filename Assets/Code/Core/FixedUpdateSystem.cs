namespace UnitySystemFramework.Core
{
    public abstract class FixedUpdateSystem : BaseSystem, ISystem
    {
        /// <summary>
        /// Called when the game's fixed update occurs.
        /// </summary>
        protected abstract void OnFixedUpdate();

        void ISystem.OnStart()
        {
            Game.AddFixedUpdate(OnFixedUpdate);
            OnStart();
        }

        void ISystem.OnEnd()
        {
            Game.RemoveFixedUpdate(OnFixedUpdate);
            OnEnd();
        }
    }
}
