using System.Collections.Generic;

namespace ConfigGeneration.Configs
{
    public interface IGenerateConfig
    {
        string ConfigName { get; }
        string ConfigNamespace { get; }
        IEnumerable<ConfigEntry> GetConfigEntries();
    }
}
