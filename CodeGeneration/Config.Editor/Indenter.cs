using System;

namespace ConfigGeneration.Configs
{
    public class Indenter : IDisposable
    {
        private readonly Action _callback;

        public Indenter(Action callback)
        {
            _callback = callback;
        }

        public void Dispose()
        {
            _callback?.Invoke();
        }
    }
}