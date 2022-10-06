using System;

namespace UnitySystemFramework.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute()
        {
        }
    }
}
