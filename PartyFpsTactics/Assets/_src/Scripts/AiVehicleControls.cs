using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks.Unity.Timeline;
using Cysharp.Threading.Tasks.Triggers;
using MrPink.Health;
using MrPink.PlayerSystem;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class AiVehicleControls : MonoBehaviour
{
    public bool controllingVehicle = false;
    public HealthController hc;
    public ControlledVehicle controlledVehicle;
    public Vector3 targetPosition;
    private Vector3[] cornersPath;
    
    public float stoppingDistance = 20;
    public float maxReverseDistance = 100;
    public float stoppingSpeed = 50;

    private void Start()
    {
        if (controllingVehicle)
            DriverSit(controlledVehicle);
    }

    private Coroutine exitCoroutine;
    IEnumerator ExitVehicleCoroutine()
    {
        float t = 0;
        float tt = 0.5f;

        while (t < tt)
        {
            t += Time.deltaTime;
            hc.transform.position = Vector3.Lerp(controlledVehicle.sitTransformNpc.position, 
                controlledVehicle.sitTransformNpc.position + controlledVehicle.sitTransformNpc.right * 1.5f, t/tt);
            yield return null;
        }
        controlledVehicle = null;
        hc.HumanVisualController.SetCollidersTriggers(false);
        hc.AiMovement.RestartActivities();
    }
    public void SetPassengerSit(ControlledVehicle _vehicle, bool smoothExit = true)
    {
        // включить анимацию
        // начинать преследовать трансформ нпс сит
        if (exitCoroutine != null)
            StopCoroutine(exitCoroutine);
        
        if (_vehicle != null)
        {
            controlledVehicle = _vehicle;
            controllingVehicle = false;
            hc.AiMovement.StopActivities();
            hc.HumanVisualController.SetCollidersTriggers(true);
            hc.HumanVisualController.SetVehiclePassenger(controlledVehicle);
            followSitCoroutine = StartCoroutine(FollowSit());   
        }
        else
        {
            controllingVehicle = false;
            if (smoothExit)
                exitCoroutine = StartCoroutine(ExitVehicleCoroutine());
            if (followSitCoroutine != null)
                StopCoroutine(followSitCoroutine);
            
            hc.HumanVisualController.SetVehiclePassenger(null);
        }
    }

    public void DriverSit(ControlledVehicle vehicle)
    {
        controlledVehicle = vehicle;
        controllingVehicle = true;
        updateNavMeshPathCoroutine = StartCoroutine(UpdateNavmeshPath());
        controlVehicleCoroutine = StartCoroutine(ControlVehicle());
    }

    private Coroutine updateNavMeshPathCoroutine;
    IEnumerator UpdateNavmeshPath()
    {
        NavMeshPath path = new NavMeshPath();
        Vector3 posToSample = Vector3.zero;
        while (true)
        {
            posToSample = Player.Position;
            NavMesh.SamplePosition(posToSample, out var hit, 10, NavMesh.AllAreas);
            if (NavMesh.CalculatePath(transform.position, Player.Position, NavMesh.AllAreas, path))
            {
                cornersPath = path.corners;
                //targetPosition = cornersPath.Last();
                targetPosition = cornersPath.Length > 1 ? cornersPath[1] : Player.Position;
            }
                
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (cornersPath == null || cornersPath.Length <= 1)
            return;
        
        for (var index = 0; index < cornersPath.Length - 1; index++)
        {
            var corner = cornersPath[index];
            
            if (index == 1)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.yellow;
            
            Gizmos.DrawSphere(corner, 1);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(corner, cornersPath[index + 1]);
        }
    }

    private Coroutine controlVehicleCoroutine;
    IEnumerator ControlVehicle()
    {
        float hor = 0;
        float ver = 0;
        bool brake = false;
        controlledVehicle.wheelVehicle.Handbrake = false;
        
        while (controlledVehicle)
        {
            yield return null;
            
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

                    if (distance < stoppingDistance && controlledVehicle.wheelVehicle.Speed > stoppingSpeed)
                    {
                        ver = -1;
                    }
                }
                else
                {
                    // target behind
                    if (distance > maxReverseDistance)
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
                // try to stop
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

    private Coroutine followSitCoroutine;
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