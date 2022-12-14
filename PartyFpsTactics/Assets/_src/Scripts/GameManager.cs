using System;
using System.Collections.Generic;
using _src.Scripts;
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
        private bool cursorVisible = false;

        public Material rockDefaultMaterial;

        public float currentTimeScale = 1;

        [Serializable]
        public enum LevelType
        {
            Intermission, Building, Road, Train, Bridge, Stealth
        }

        [SerializeField] private LevelType _levelType = LevelType.Building;
        public LevelType GetLevelType => _levelType;
    
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
        
            Instance = this;
            Random.InitState((int)DateTime.Now.Ticks);
            DontDestroyOnLoad(gameObject);
            Physics.autoSyncTransforms = false;
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
            currentTimeScale = t;
            currentTimeScale = Mathf.Clamp(currentTimeScale, 0.1f, 100);
            Time.timeScale = currentTimeScale;
        }
        
    
        private void Update()
        {
            if (Game._instance == null || Game.Player == null)
            {
                if (cursorVisible == false)
                {
                    cursorVisible = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    Time.timeScale = 1;
                    AudioListener.pause = false;
                }
                return;
            }
            
            if (Input.GetKeyDown(KeyCode.O))
                AudioListener.volume = Mathf.Clamp(AudioListener.volume - 0.1f, 0, 1);
            if (Input.GetKeyDown(KeyCode.P))
                AudioListener.volume = Mathf.Clamp(AudioListener.volume + 0.1f, 0, 1);

            if (Input.GetKeyDown(KeyCode.Alpha0))
                Game.Player.LookAround._mouseSensitivity = Mathf.Clamp(Game.Player.LookAround._mouseSensitivity + 50, 5, 1000);
            if (Input.GetKeyDown(KeyCode.Alpha9))
                Game.Player.LookAround._mouseSensitivity = Mathf.Clamp(Game.Player.LookAround._mouseSensitivity - 50, 5, 1000);
            
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (cursorVisible)
                {
                    cursorVisible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    Time.timeScale = currentTimeScale;
                    AudioListener.pause = false;
                }
                else
                {
                    cursorVisible = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    Time.timeScale = 0.1f;
                    AudioListener.pause = true;
                }
            }
        
            if (Game.Player.Health.health <= 0 && Input.GetKeyDown(KeyCode.R))
            {
                // player died
                // restart at different place
                RespawnPlayer();
            }
            // CHEATS
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.Z))
            {
                if (Input.GetKeyDown(KeyCode.T))
                    ContentPlacer.Instance.SpawnRedUnitAroundPlayer();
                if (Input.GetKeyDown(KeyCode.R))
                    RespawnPlayer();
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
            Game.Player.Health.Damage(10000000, DamageSource.Environment);
        }
        public void RespawnPlayer()
        {
            // change player's position
            switch (_levelType)
            {
                case LevelType.Building:
                    StartBuildingScene();
                    break;
                case LevelType.Road:
                    StartRoadScene();
                    break;
                case LevelType.Train:
                    StartTrainScene();
                    break;
                case LevelType.Bridge:
                    StartBridgeScene();
                    break;
                case LevelType.Stealth:
                    StartStealthScene();
                    break;
            }
        }

        public void LevelCompleted()
        {
            if (ProgressionManager.Instance.currentLevelIndex >= ProgressionManager.Instance.levelDatas.Count)
                ProgressionManager.Instance.SetCurrentLevel(Random.Range(0, ProgressionManager.Instance.levelDatas.Count));
            else
                ProgressionManager.Instance.SetCurrentLevel(ProgressionManager.Instance.currentLevelIndex + 1);

            StartIntermissionScene();
            return;
            switch (_levelType)
            {
                case LevelType.Building:
                    int r = Random.Range(1, SceneManager.sceneCountInBuildSettings);
                    switch (r)
                    {
                        case 1: StartRoadScene(); break;
                        case 2: StartTrainScene(); break;
                        case 3: StartBridgeScene(); break;
                        //case 4: StartStealthScene(); break;
                        
                        default: StartTrainScene(); break;
                    }
                    break;
                default:
                    StartBuildingScene();
                    break;
            }
        }
        
        public void StartIntermissionScene()
        {
            _levelType = LevelType.Intermission;
            SceneManager.LoadScene(0);
        }

        public void StartLevel(LevelType levelType)
        {
            switch (levelType)
            {
                case LevelType.Building:
                    StartBuildingScene();
                    break;
                case LevelType.Road:
                    StartRoadScene();
                    break;
                case LevelType.Train:
                    StartTrainScene();
                    break;
                case LevelType.Stealth:
                    StartStealthScene();
                    break;
                case LevelType.Bridge:
                    StartBridgeScene();
                    break;
            }
        }
        public void StartBuildingScene()
        {
            _levelType = LevelType.Building;
            SceneManager.LoadScene(1);
        }
        public void StartRoadScene()
        {
            _levelType = LevelType.Road;
            SceneManager.LoadScene(2);
        } 
        public void StartTrainScene()
        {
            _levelType = LevelType.Train;
            SceneManager.LoadScene(3);
        } 
        public void StartBridgeScene()
        {
            _levelType = LevelType.Bridge;
            SceneManager.LoadScene(4);
        }
        public void StartStealthScene()
        {
            StartBuildingScene();
            return;
            
            _levelType = LevelType.Stealth;
            SceneManager.LoadScene(5);
        }
    
        public void StartFlatScene()
        {
            return;
        }


        public bool IsPositionInPlayerFov(Vector3 pos)
        {
            bool inFov = false;
            var viewportPoint = Game.Player.MainCamera.WorldToViewportPoint(pos);
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