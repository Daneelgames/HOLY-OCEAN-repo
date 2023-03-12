using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using MrPink.PlayerSystem;
using MrPink.Units;
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
        [SerializeField] private Transform gameplayUi;

        public static Player LocalPlayer
            => _instance._localPlayer;

        public static GlobalFlags Flags
            => _instance._flags;

        
        [BoxGroup("CAMERA")] [SerializeField] private GameCamera _gameCamera;
        [BoxGroup("CAMERA")] [SerializeField] private Transform cameraParent;
        [BoxGroup("CAMERA")] [SerializeField] private Animator cameraAnimator;
        [BoxGroup("CAMERA")] [SerializeField] private float camMoveSmooth = 10;
        [BoxGroup("CAMERA")] [SerializeField] private float camMoveSmoothVehicle = 10;
        [BoxGroup("CAMERA")] [SerializeField] private float camRotSmooth = 10;
        [BoxGroup("CAMERA")] [SerializeField] private float camRotSmoothVehicle = 5;
        private static readonly int Screenshake = Animator.StringToHash("Screenshake");
        public Camera PlayerCamera => _gameCamera._Camera;

        [SerializeField] private AudioSource earthquakeAu;

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
            if (_localPlayer.VehicleControls.controlledMachine == null)
            {
                cameraParent.transform.position = Vector3.Lerp(cameraParent.transform.position, _localPlayer.LookAround.HeadPos, camMoveSmooth * Time.deltaTime);
                cameraParent.transform.rotation = Quaternion.Slerp(cameraParent.transform.rotation, _localPlayer.LookAround.HeadRot, camRotSmooth * Time.deltaTime);
            }
            else
            {
                //cameraParent.transform.position = Vector3.Lerp(cameraParent.transform.position, _localPlayer.VehicleControls.controlledMachine.CameraTransform.position, camMoveSmoothVehicle * Time.deltaTime);
                cameraParent.transform.position = _localPlayer.VehicleControls.controlledMachine.CameraTransform.position;
                cameraParent.transform.rotation = Quaternion.Slerp(cameraParent.transform.rotation, _localPlayer.LookAround.HeadRot, camRotSmoothVehicle * Time.deltaTime);
            }
        }

        public struct PlayerDistance
        {
            public Player Player;
            public float distance;
        }
        public PlayerDistance DistanceToClosestPlayer(Vector3 pos)
        {
            PlayerDistance closest = new PlayerDistance();
            float distance = 100000;
            foreach (var player in playersesInGame)
            {
                if (player == null)
                    continue;
                var newDist = Vector3.Distance(pos, player.transform.position);
                if (newDist < distance)
                {
                    distance = newDist;
                    closest.Player = player;
                }
            }
            closest.distance = distance;

            return closest;
        }
        
        public void RespawnAllPlayers()
        {
            ProgressionManager.Instance.RunOver();
            if (playersesInGame.Count > 0)
            {
                for (var index = playersesInGame.Count - 1; index >= 0; index--)
                {
                    var player = playersesInGame[index];
                    if (player != null)
                        player.Respawn();
                }
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
            if (playersesInGame.Contains(p) == false) return;
            
            playersesInGame.Remove(p);
        }
        
        public void SetLocalPlayer(Player p)
        {
            gameplayUi.gameObject.SetActive(true);   
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

        private bool levelGenerating = false;
        public bool IsLevelGenerating => levelGenerating;
        
        public void SetLevelGeneratingFeedback(bool active)
        {
            levelGenerating = active;
        }
        
    }
}