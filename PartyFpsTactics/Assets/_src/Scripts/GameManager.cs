using System;
using MrPink.Health;
using MrPink.PlayerSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public LayerMask AllSolidsMask;
    
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
        

    }

    void Start()
    {
        /*
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;*/
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            AudioListener.volume = Mathf.Clamp(AudioListener.volume - 0.1f, 0, 1);
        if (Input.GetKeyDown(KeyCode.P))
            AudioListener.volume = Mathf.Clamp(AudioListener.volume + 0.1f, 0, 1);

        if (Input.GetKeyDown(KeyCode.Alpha0))
            Player.LookAround._mouseSensitivity = Mathf.Clamp(Player.LookAround._mouseSensitivity + 50, 5, 1000);
        if (Input.GetKeyDown(KeyCode.Alpha9))
            Player.LookAround._mouseSensitivity = Mathf.Clamp(Player.LookAround._mouseSensitivity - 50, 5, 1000);
        
        if (LevelGenerator.Instance.levelIsReady == false)
            return;
        
        if (Player.Health.health <= 0 && LevelGenerator.Instance.levelType == LevelGenerator.LevelType.Game && Input.GetKeyDown(KeyCode.R))
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
        var viewportPoint = Player.MainCamera.WorldToViewportPoint(pos);
        if (viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1)
            inFov = true;
        return inFov;
    }
}
