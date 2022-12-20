using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink
{
    public class Game : MonoBehaviour
    {
        public static Game _instance;
        [SerializeField]
        private GlobalFlags _flags = new GlobalFlags();
        
        [SerializeField]
        private Player _localPlayer;

        [ReadOnly] [SerializeField] private List<Player> playersInGame = new List<Player>();
        public List<Player> PlayerInGame => playersInGame;
        [SerializeField, SceneObjectsOnly]
        
        private LightManager _lightManager;

        public static LightManager LightManager => _instance._lightManager;

        public static Player LocalPlayer
            => _instance._localPlayer;

        public static GlobalFlags Flags
            => _instance._flags;


        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AddPlayer(Player p)
        {
            playersInGame.Add(p);
        }
        public void RemovePlayer(Player p)
        {
            if (playersInGame.Contains(p)) return;
            
            playersInGame.Remove(p);
        }
        
        public void SetLocalPlayer(Player p)
        {
            _localPlayer = p;
        }
    }
}