using System.Collections.Generic;
using UnitySystemFramework.Core;
using UnitySystemFramework.Networking;
using UnitySystemFramework.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnitySystemFramework.Levels
{
    public struct SceneBeginLoadEvent : IEvent
    {
        public Scene Scene;
    }

    public struct SceneLoadEvent : IEvent
    {
        public Scene Scene;
    }

    public struct SceneUnloadEvent : IEvent
    {
        public Scene Scene;
    }

    public struct LevelBeginLoadEvent : IEvent
    {
        public Level Level;
        public AsyncOperation Async;
    }

    public struct LevelLoadEvent : IEvent
    {
        public Level Level;
    }

    public struct LevelUnloadEvent : IEvent
    {
        public Level Level;
    }

    public struct OtherLoadLevelEvent : INetworkEvent
    {
        public Level Level;

        public uint CompositeID { get; set; }

        public void OnRead(IGame game, ByteBuffer buffer)
        {
            Level = new Level()
            {
                Name = buffer.ReadString(),
                SceneName = buffer.ReadString(),
            };
        }

        public void OnWrite(IGame game, ByteBuffer buffer)
        {
            buffer.WriteString(Level.Name);
            buffer.WriteString(Level.SceneName);
        }
    }
}
