using System;

namespace UnitySystemFramework.Identity
{
    public class IDAlreadyExistsException : Exception
    {
        public ID ID { get; private set; }

        public IDAlreadyExistsException(ID id)
        {
            ID = id;
        }

        public IDAlreadyExistsException(ID id, string message) : base(message)
        {
            ID = id;
        }
    }
}
