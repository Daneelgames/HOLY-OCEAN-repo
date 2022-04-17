using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using UnityEngine;
using VehicleBehaviour;

public class ControlledVehicle : MonoBehaviour
{
    public WheelVehicle wheelVehicle;
    public Transform sitTransform;
    public Transform sitTransformNpc;
    
    public void StartPlayerInput()
    {
        wheelVehicle.IsPlayer = true;
        wheelVehicle.Handbrake = false;
    }

    public void StopMovement()
    {
        wheelVehicle.IsPlayer = false;
        wheelVehicle.Handbrake = true;
    }
    
    public void SetCarInput(float hor, float ver, bool brake)
    {
        // перекинуть в общий прием инпута от юнитов
        wheelVehicle.SetInput(hor, ver, brake);
    }
}