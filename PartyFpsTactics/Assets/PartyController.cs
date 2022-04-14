using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using UnityEngine;

public class PartyController : MonoBehaviour
{
    public static PartyController Instance;
    
    public ControlledVehicle playerCar;

    private void Awake()
    {
        Instance = this;
    }    
    
    public void Init(Transform roadPartToSpawnOn)
    {
        if (roadPartToSpawnOn == null)
        {
            Debug.LogError("NO STRAIGHT ROADS HERE");
        }
        Vector3 newCarPos = roadPartToSpawnOn.position + roadPartToSpawnOn.forward * 5;
        Quaternion newCarRot = roadPartToSpawnOn.rotation;
        
        // PLACE PLAYER INSIDE THE CAR
        // PLACE NPC INSIDE THE CAR
        // MOVE THE CAR TO NEW TRANSFORM

        playerCar.transform.position = newCarPos;
        playerCar.transform.rotation = newCarRot;

        Player.VehicleControls.RequestVehicleAction(playerCar);
    }
}