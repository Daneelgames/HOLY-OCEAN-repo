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

    public PhysicMaterial corpsesMaterial;
    public HealthController redTeamUnitPrefab;
    public HealthController blueTeamUnitPrefab;
    
    
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
        if (LevelGenerator.Instance.levelType == LevelGenerator.LevelType.Game && Input.GetKeyDown(KeyCode.R))
            StartProcScene();
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

    public void SpawnBlueUnit(Vector3 pos)
    {
        var newUnit = Instantiate(blueTeamUnitPrefab, pos, Quaternion.identity);
        newUnit.AiMovement.TakeCoverOrder();
        //CommanderControls.Instance.unitsInParty.Add(newUnit);
    }
    public void SpawnRedUnit(Vector3 pos)
    {
        var newUnit = Instantiate(redTeamUnitPrefab, pos, Quaternion.identity);
        if (Random.value > 0.9f)
            newUnit.AiMovement.MoveToPositionOrder(Player.Position);
        else 
            newUnit.AiMovement.TakeCoverOrder();
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
