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
        private void LateUpdate()
        {
            if (_localPlayer == null) return;
            if (_localPlayer.VehicleControls.controlledMachine == null)
            {
                cameraParent.transform.position = Vector3.Lerp(cameraParent.transform.position,
                    _localPlayer.LookAround.HeadPos, camMoveSmooth * Time.fixedUnscaledDeltaTime);
                cameraParent.transform.rotation = Quaternion.Slerp(cameraParent.transform.rotation, _localPlayer.LookAround.HeadRot, camRotSmooth * Time.fixedUnscaledDeltaTime);
            }
            else
            {
                //_gameCamera.transform.position = _localPlayer.VehicleControls.controlledMachine.CameraTransform.position;
                
                cameraParent.transform.position = Vector3.Lerp(cameraParent.transform.position,
                    _localPlayer.VehicleControls.controlledMachine.CameraTransform.position,
                    camMoveSmoothVehicle * Time.unscaledDeltaTime);
                
                cameraParent.transform.rotation = Quaternion.Slerp(cameraParent.transform.rotation, _localPlayer.LookAround.HeadRot, camRotSmoothVehicle * Time.fixedUnscaledDeltaTime);
            }
        }
        
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
        
        public void RespawnAllPlayers()
        {
            // called on server? seems not
            if (playersesInGame.Count > 0)
            {
                for (var index = playersesInGame.Count - 1; index >= 0; index--)
                {
                    var player = playersesInGame[index];
                    if (player != null)
                        player.Respawn();
                }
            }

            UnitsManager.Instance.HealAllUnits();
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