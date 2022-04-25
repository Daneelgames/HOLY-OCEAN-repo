using System;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace MrPink
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public string portableObjectTag = "PortableObject";
        public List<Collider> terrainAndIslandsColliders = new List<Collider>();
        public LayerMask AllSolidsMask;
        private bool cursorVisible = false;

        public Material rockDefaultMaterial;
    
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

    
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
                AudioListener.volume = Mathf.Clamp(AudioListener.volume - 0.1f, 0, 1);
            if (Input.GetKeyDown(KeyCode.P))
                AudioListener.volume = Mathf.Clamp(AudioListener.volume + 0.1f, 0, 1);

            if (Input.GetKeyDown(KeyCode.Alpha0))
                Game.Player.LookAround._mouseSensitivity = Mathf.Clamp(Game.Player.LookAround._mouseSensitivity + 50, 5, 1000);
            if (Input.GetKeyDown(KeyCode.Alpha9))
                Game.Player.LookAround._mouseSensitivity = Mathf.Clamp(Game.Player.LookAround._mouseSensitivity - 50, 5, 1000);
        
            if (LevelGenerator.Instance != null && LevelGenerator.Instance.IsLevelReady == false)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (cursorVisible)
                {
                    cursorVisible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    Time.timeScale = 1f;
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
        
            if (LevelGenerator.Instance == null)
                return;
            
            if (Game.Player.Health.health <= 0 && LevelGenerator.Instance.levelType == LevelGenerator.LevelType.Game && Input.GetKeyDown(KeyCode.R))
                StartProcScene();
        
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.Z))
            {
                if (Input.GetKeyDown(KeyCode.R))
                    StartProcScene();
            
                if (Input.GetKeyDown(KeyCode.D))
                {
                    if (LevelGenerator.Instance.spawnedMainBuildingLevels[0].tilesTop.Count <= 0)
                        return;

                    for (int i = LevelGenerator.Instance.spawnedMainBuildingLevels[0].tilesTop.Count - 1; i >= 0; i--)
                    {
                        var tile = LevelGenerator.Instance.spawnedMainBuildingLevels[0].tilesTop[i];
                        if (tile)
                            tile.Damage(1000, DamageSource.Environment);
                    }
                }
            }
        }

        public void StartProcScene()
        {
            SceneManager.LoadScene(0);
        }
    
        public void StartFlatScene()
        {
            ScreenshotSaver.Instance.SaveScreenshot();
            SceneManager.LoadScene(1);
        }


        public bool IsPositionInPlayerFov(Vector3 pos)
        {
            bool inFov = false;
            var viewportPoint = Game.Player.MainCamera.WorldToViewportPoint(pos);
            if (viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1)
                inFov = true;
            return inFov;
        }
    }
}