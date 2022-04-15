using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using UnityEngine;

public class AiVehicleControls : MonoBehaviour
{
    public HealthController hc;
    
    public void PassengerSit(ControlledVehicle vehicle)
    {
        // включить анимацию
        // начинать преследовать трансформ нпс сит

        hc.AiMovement.StopActivities();
        hc.HumanVisualController.SetVehiclePassenger(vehicle);
        StartCoroutine(FollowSit(vehicle));
    }

    IEnumerator FollowSit(ControlledVehicle vehicle)
    {
        var sit = vehicle.sitTransformNpc;
        while (true)
        {
            transform.position = sit.position;
            transform.rotation = sit.rotation;
            yield return null;
        }
    }
}