using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySystemFramework.Core
{
    public class GameBehaviour : MonoBehaviour
    {
        private static readonly Dictionary<IGame, GameBehaviour> _behaviours = new Dictionary<IGame, GameBehaviour>();

        /// <summary>
        /// Gets the behaviour for the specified game.
        /// </summary>
        public static GameBehaviour GetBehaviour(IGame game)
        {
            _behaviours.TryGetValue(game, out var behaviour);
            return behaviour;
        }
        
        public static void Init(string gameName, IGame game)
        {
            var go = new GameObject(gameName);
            go.SetActive(false); // Disable so awake is not called on the game behaviour until we set the game.
            var gb = go.AddComponent<GameBehaviour>();
            _behaviours[game] = gb;
            gb.SetGame(game);
            go.SetActive(true);
        }

        public string GameType = "Choose a Type";
        public bool DestroyOnSceneLoad = false;

        private IGame _game;

        void Awake()
        {
            if (_game == null)
            {
                if (string.IsNullOrWhiteSpace(GameType))
                    throw new ArgumentNullException("GameType", "A type was not specified.");

                var type = Type.GetType(GameType);

                if (type == null)
                    throw new ArgumentNullException("GameType", "Unable to find the type specified.");

                _game = (IGame) Activator.CreateInstance(type);
            }
            else
            {
                GameType = _game.GetType().AssemblyQualifiedName;
            }

            _game.Init();

            if (!DestroyOnSceneLoad)
                DontDestroyOnLoad(gameObject);

            _game.Start();
        }

        void Start()
        {
        }

        void Update()
        {
            _game.Update();
        }

        void FixedUpdate()
        {
            _game.FixedUpdate();
        }

        void LateUpdate()
        {
            _game.LateUpdate();
        }

        void OnDestroy()
        {
            _game.End();
            _behaviours.Remove(_game);
        }

        void OnApplicationQuit()
        {
            _game.Exit();
            _behaviours.Remove(_game);
        }

        public IGame GetGame()
        {
            return _game;
        }

        public void SetGame(IGame game)
        {
            if(_game != null)
                _behaviours.Remove(_game);

            _game = game;

            if (_game != null)
                _behaviours[_game] = this;
        }
    }
}
