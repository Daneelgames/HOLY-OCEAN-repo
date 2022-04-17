using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using UnityEngine;

public class AiVehicleControls : MonoBehaviour
{
    public bool inControl = false;
    public HealthController hc;
    public ControlledVehicle controlledVehicle;
    public Vector3 targetPosition;

    private void Start()
    {
        if (inControl)
            DriverSit(controlledVehicle);
    }

    public void PassengerSit(ControlledVehicle _vehicle)
    {
        // включить анимацию
        // начинать преследовать трансформ нпс сит
        controlledVehicle = _vehicle;
        inControl = false;
        hc.AiMovement.StopActivities();
        hc.HumanVisualController.SetVehiclePassenger(controlledVehicle);
        StartCoroutine(FollowSit());
    }

    public void DriverSit(ControlledVehicle vehicle)
    {
        inControl = true;
        StartCoroutine(ControlVehicle());
    }

    IEnumerator ControlVehicle()
    {
        float hor = 0;
        float ver = 0;
        bool brake = false;
        controlledVehicle.wheelVehicle.Handbrake = false;
        while (controlledVehicle)
        {
            yield return null;
            targetPosition = Player.Movement.transform.position;
            
            float reachedTargetDistance = 5;
            float distance = Vector3.Distance(transform.position, targetPosition);
            
            if (distance > reachedTargetDistance)
            {
                Vector3 dirToMovePos = (targetPosition - transform.position).normalized;
                var dot = Vector3.Dot(transform.forward, dirToMovePos);
                if (dot > 0)
                {
                    // target in front
                    ver = 1f;

                    float stoppingDistance = 30;
                    float stoppingSpeed = 50;
                    if (distance < stoppingDistance && controlledVehicle.wheelVehicle.Speed > stoppingSpeed)
                    {
                        ver = -1;
                    }
                }
                else
                {
                    // target behind
                    if (distance > 20)
                    {
                        // too far to rewerse
                        ver = 1f;
                    }
                    else
                        ver = -1f;
                }

                var angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePos, Vector3.up);

                if (angleToDir > 0)
                    hor = 1f;
                else
                    hor = -1f;
            }
            else
            {
                if (controlledVehicle.wheelVehicle.Speed > 10)
                    ver = -1;
                else
                {
                    ver = 0;
                    hor = 0;   
                }
            }
            
            controlledVehicle.SetCarInput(hor, ver, brake);
        }
    }
    
    IEnumerator FollowSit()
    {
        var sit = controlledVehicle.sitTransformNpc;
        while (true)
        {
            transform.position = sit.position;
            transform.rotation = sit.rotation;
            yield return null;
        }
    }
}