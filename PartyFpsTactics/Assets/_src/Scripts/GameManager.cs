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
                if (!cursorVisible)
                {
                    cursorVisible = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    AudioListener.pause = false;
                }
                return;
            }
            
            if (Input.GetKeyDown(KeyCode.O))
                AudioListener.volume = Mathf.Clamp(AudioListener.volume - 0.1f, 0, 1);
            if (Input.GetKeyDown(KeyCode.P))
                AudioListener.volume = Mathf.Clamp(AudioListener.volume + 0.1f, 0, 1);

            if (Input.GetKeyDown(KeyCode.Alpha0))
                Game.LocalPlayer.LookAround._mouseSensitivity = Mathf.Clamp(Game.LocalPlayer.LookAround._mouseSensitivity + 50, 5, 1000);
            if (Input.GetKeyDown(KeyCode.Alpha9))
                Game.LocalPlayer.LookAround._mouseSensitivity = Mathf.Clamp(Game.LocalPlayer.LookAround._mouseSensitivity - 50, 5, 1000);
            
            
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
            {
                if (cursorVisible)
                {
                    cursorVisible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    //SetCurrentTimeScale(1);
                    //AudioListener.pause = false;
                    SettingsGameWrapper.Instance.CloseMenu();
                    PlayerInventoryUI.Instance.HideInventory();
                }
                else
                {
                    cursorVisible = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    //SetCurrentTimeScale(0);
                    //AudioListener.pause = true;
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
                    ScoringSystem.Instance.AddScore(1000);
                if (Input.GetKeyDown(KeyCode.K))
                    KillPlayer();
                if (Input.GetKeyDown(KeyCode.X))
                    ScoringSystem.Instance.AddScore(-ScoringSystem.Instance.CurrentScore);
            
                /*
                if (Input.GetKeyDown(KeyCode.D))
                {
                    if (BuildingGenerator.Instance.spawnedBuildingLevels[0].tilesTop.Count <= 0)
                        return;

                    for (int i = BuildingGenerator.Instance.spawnedBuildingLevels[0].tilesTop.Count - 1; i >= 0; i--)
                    {
                        var tile = BuildingGenerator.Instance.spawnedBuildingLevels[0].tilesTop[i];
                        if (tile)
                            tile.Damage(1000, DamageSource.Environment);
                    }
                }*/
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
        
        

        
        public void StartLobbyScene()
        {
            _levelType = LevelType.Lobby;
            SceneManager.LoadScene(0);
        }

        public void StartLevel(LevelType levelType)
        {
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