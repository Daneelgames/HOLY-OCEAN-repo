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

        [ReadOnly] [SerializeField] private List<Player> playersesInGame = new List<Player>();
        public List<Player> PlayersInGame => playersesInGame;
        [SerializeField, SceneObjectsOnly]
        
        private LightManager _lightManager;

        public static LightManager LightManager => _instance._lightManager;

        public static Player LocalPlayer
            => _instance._localPlayer;

        public static GlobalFlags Flags
            => _instance._flags;

        public float DistanceToClosestPlayer(Vector3 pos)
        {
            float distance = 100000;
            foreach (var player in playersesInGame)
            {
                if (player == null)
                    continue;
                var newDist = Vector3.Distance(pos, player.transform.position);
                if (newDist < distance)
                    distance = newDist;
            }

            return distance;
        }

        [Header("CAMERA")]
        [SerializeField] private GameCamera _gameCamera;
        public Camera PlayerCamera => _gameCamera._Camera;
        [SerializeField] private float camMoveSmooth = 10;
        [SerializeField] private float camRotSmooth = 10;
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

        
        //private void LateUpdate()
        private void Update()
        {
            if (_localPlayer == null) return;
            _gameCamera.transform.position =  Vector3.Lerp(_gameCamera.transform.position, _localPlayer.LookAround.HeadPos, camMoveSmooth * Time.unscaledDeltaTime);
            _gameCamera.transform.rotation = Quaternion.Slerp(_gameCamera.transform.rotation, _localPlayer.LookAround.HeadRot, camRotSmooth/* * Time.unscaledDeltaTime*/);
        }

        
        public void RespawnAllPlayers()
        {
            // called on server
            foreach (var player in playersesInGame)
            {
                player.Respawn();
            }
        }
        
        public void AddPlayer(Player p)
        {
            if (playersesInGame.Contains(p))
                return;
            
            playersesInGame.Add(p);
        }
        public void RemovePlayer(Player p)
        {
            if (playersesInGame.Contains(p)) return;
            
            playersesInGame.Remove(p);
        }
        
        public void SetLocalPlayer(Player p)
        {
            _localPlayer = p;
        }

        public bool AllPlayersLoadedGameScene()
        {
            foreach (var player in PlayersInGame)
            {
                if (player == null)
                    continue;

                if (player.GameSceneLoaded == false) return false;
            }

            return true;
        }
    }
}