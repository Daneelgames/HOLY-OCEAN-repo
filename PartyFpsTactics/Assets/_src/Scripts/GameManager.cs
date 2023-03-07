using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts;
using FishNet.Component.Spawning;
using FishNet.Managing;
using Fraktalia.VoxelGen.SaveSystem.Modules;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Units;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace MrPink
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public float playerSleepTimeScale = 10;
        public string portableObjectTag = "PortableObject";
        [Header("THIS LIST IS USED WHEN MAKING A ROAD PRE RUNTIME")]
        public List<Collider> terrainAndIslandsColliders = new List<Collider>();
        public LayerMask AllSolidsMask;
        private bool cursorVisible = true;
        [SerializeField] private GameObject tileNavMeshObstaclePrefab;
        public ExplosionController DefaultFragExplosion;

        public Material rockDefaultMaterial;

        public float CurrentTimeScale = 1;

        [Serializable]
        public enum LevelType
        {
            Lobby, Game
        }

        [SerializeField] private LevelType _levelType = LevelType.Lobby;
        public LevelType GetLevelType => _levelType;
    
        private void Awake()
        {
            if (Instance != null)
            {
                StartCoroutine(Instance.SetLevelType(_levelType));
                Destroy(gameObject);
                return;
            }
            Application.targetFrameRate = 50;
            Instance = this;
            StartCoroutine(Instance.SetLevelType(_levelType));
            Random.InitState((int)DateTime.Now.Ticks);
            DontDestroyOnLoad(gameObject);
            Physics.autoSyncTransforms = false;
        }

        public IEnumerator SetLevelType(LevelType levelType)
        {
            _levelType = levelType;

            while (Game.LocalPlayer == null)
            {
                yield return null;
            }
            Game.LocalPlayer.SetLevelType(_levelType);
        }

        public void SetPlayerSleepTimeScale(bool sleep)
        {
            if (sleep)
                SetCurrentTimeScale(playerSleepTimeScale);
            else
                SetCurrentTimeScale(1);
        }
        
        void SetCurrentTimeScale(float t)
        {
             return;
            CurrentTimeScale = t;
            CurrentTimeScale = Mathf.Clamp(CurrentTimeScale, 0.1f, 100);
        }
        
    
        private void Update()
        {
            if (Game._instance == null || Game.LocalPlayer == null)
            {
                cursorVisible = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }
            
            if (PlayerInventoryUI.Instance.IsActive || MojoCustomization.Instance.IsShowing || Shop.Instance.IsActive || SettingsGameWrapper.Instance.IsOpened)
            {
                cursorVisible = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                cursorVisible = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
                
            
            
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
            {
                if (SettingsGameWrapper.Instance.IsOpened)
                {
                    SettingsGameWrapper.Instance.CloseMenu();
                    PlayerInventoryUI.Instance.HideInventory();
                }
                else
                {
                    SettingsGameWrapper.Instance.OpenMenu();
                    PlayerInventoryUI.Instance.ShowInventory();
                }
            }
        
            if (Game.LocalPlayer.Health.IsDead /*&& Input.GetKeyDown(KeyCode.R)*/)
            {
                // player died
                // restart at different place
                RespawnPlayer();
            }
            // CHEATS
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.Z))
            {
                if (Input.GetKeyDown(KeyCode.R))
                    RespawnPlayer();
                if (Input.GetKeyDown(KeyCode.F))
                    ScoringSystem.Instance.IncreaseMojoLevel();
                if (Input.GetKeyDown(KeyCode.K))
                    KillPlayer();
                if (Input.GetKeyDown(KeyCode.X))
                    ScoringSystem.Instance.AddGold(-ScoringSystem.Instance.CurrentGold);
                if (Input.GetKeyDown(KeyCode.D))
                    UnitsManager.Instance.KillAllMobs();
            }
        }

        public void KillPlayer()
        {
            Game.LocalPlayer.Health.Damage(10000000, DamageSource.Environment);
        }

        
        public void RespawnPlayer()
        {
            // player died and he'll be dead until: someone pick him up, level restarted or level completed
        }

        public void SpawnTileNavObstacle(Transform _transform)
        {
            var newObst = Instantiate(tileNavMeshObstaclePrefab, _transform.position, _transform.rotation);
            newObst.transform.parent = _transform.parent;
        }


        public bool IsPositionInPlayerFov(Vector3 pos)
        {
            bool inFov = false;
            var viewportPoint = Game.LocalPlayer.MainCamera.WorldToViewportPoint(pos);
            if (viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1)
                inFov = true;
            return inFov;
        }
        
        public string UppercaseRandomly(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            for (int i = 0; i < a.Length; i++)
            {
                if (Random.value >= 0.5f)
                    a[i] = char.ToUpper(a[i]);
                else
                    a[i] = char.ToLower(a[i]);
            }
            return new string(a);
        }
        public string RemoveRandomLetters(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            for (int i = 0; i < a.Length; i++)
            {
                if (Random.value >= 0.5f)
                    a[i] = Char.Parse(" ");
                else
                    a[i] = char.ToLower(a[i]);
            }
            return new string(a);
        }
    }
}