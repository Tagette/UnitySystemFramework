using System;

namespace UnitySystemFramework.Commands
{
    public class AliasAttribute : Attribute
    {
        public readonly string[] Aliases;

        public AliasAttribute(params string[] aliases)
        {
            Aliases = aliases;
        }
    }
}
