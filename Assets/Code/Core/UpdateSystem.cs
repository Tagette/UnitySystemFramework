namespace UnitySystemFramework.Core
{
    public abstract class UpdateSystem : BaseSystem, ISystem
    {
        /// <summary>
        /// Called when the game's update occurs.
        /// </summary>
        protected abstract void OnUpdate();

        void ISystem.OnStart()
        {
            Game.AddUpdate(OnUpdate);
            OnStart();
        }

        void ISystem.OnEnd()
        {
            Game.RemoveUpdate(OnUpdate);
            OnEnd();
        }
    }
}
