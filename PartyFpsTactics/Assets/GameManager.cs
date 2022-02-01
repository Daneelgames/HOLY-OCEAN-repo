using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<HealthController> ActiveHealthControllers;

    public PhysicMaterial corpsesMaterial;
    public HealthController redTeamUnitPrefab;
    public HealthController blueTeamUnitPrefab;
    
    private void Awake()
    {
        Instance = this;
        Random.InitState((int)DateTime.Now.Ticks);
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            Restart();
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public void SpawnBlueUnit(Vector3 pos)
    {
        var newUnit = Instantiate(blueTeamUnitPrefab, pos, Quaternion.identity);
        newUnit.AiMovement.TakeCoverOrder(true, false);
        CommanderControls.Instance.unitsInParty.Add(newUnit);
    }
    public void SpawnRedUnit(Vector3 pos)
    {
        var newUnit = Instantiate(redTeamUnitPrefab, pos, Quaternion.identity);
        if (Random.value > 0.9f)
            newUnit.AiMovement.MoveToPositionOrder(PlayerMovement.Instance.transform.position);
        else 
            newUnit.AiMovement.TakeCoverOrder(true, true);
    }
}
