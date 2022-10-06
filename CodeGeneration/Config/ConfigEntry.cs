namespace ConfigGeneration.Configs
{
    public struct ConfigEntry
    {
        public string Key;
        public object Value;
        public string Doc;

        public ConfigEntry(string key, object value, string doc = "")
        {
            Key = key;
            Value = value;
            Doc = doc;
        }
    }
}
