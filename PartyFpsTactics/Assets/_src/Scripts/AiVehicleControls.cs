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

namespace MrPink
{
    public class AiVehicleControls : MonoBehaviour
    {
        public bool controllingVehicle = false;
        public HealthController hc;
        public ControlledMachine controlledMachine;
        public Vector3 targetPosition;
        private Vector3[] cornersPath;
    
        public float stoppingDistance = 20;
        public float maxReverseDistance = 100;
        public float stoppingSpeed = 50;

        private void Start()
        {
            if (controllingVehicle)
                DriverSit(controlledMachine);
        }

        private Coroutine exitCoroutine;
        IEnumerator AiExitVehicleCoroutine()
        {
            if (controlledMachine == null || controlledMachine.sitTransformNpc == null)
                yield break;
            
            float t = 0;
            float tt = 0.5f;

            while (t < tt)
            {
                t += Time.deltaTime;
                hc.transform.position = Vector3.Lerp(controlledMachine.sitTransformNpc.position, 
                    controlledMachine.sitTransformNpc.position + controlledMachine.sitTransformNpc.right * 1.5f, t/tt);
                yield return null;
            }
            controlledMachine = null;
            hc.HumanVisualController.SetCollidersTriggers(false);
            hc.AiMovement.RestartActivities();
        }
        private Coroutine enterCoroutine;
        IEnumerator EnterVehicleCoroutine()
        {
            float t = 0;
            float tt = 0.5f;

            hc.HumanVisualController.SetVehicleAiPassenger(controlledMachine);
            hc.HumanVisualController.SetCollidersTriggers(true);
            hc.AiMovement.StopActivities();
            var initPos = hc.transform.position;
            while (t < tt)
            {
                t += Time.deltaTime;
                hc.transform.position = Vector3.Lerp(initPos, 
                    controlledMachine.sitTransformNpc.position, t/tt);
                yield return null;
            }
            followSitCoroutine = StartCoroutine(FollowSit());   
        }
        public void SetPassengerSit(ControlledMachine machine, bool smoothExit = true)
        {
            if (machine && machine.sitTransformNpc == null)
                return;
            
            // включить анимацию
            // начинать преследовать трансформ нпс сит
            if (exitCoroutine != null)
                StopCoroutine(exitCoroutine);
            if (enterCoroutine != null)
                StopCoroutine(enterCoroutine);
            Debug.Log("SetPassengerSit. veh: " + machine + "; smooth: " + smoothExit);
        
            if (machine != null)
            {
                controlledMachine = machine;
                controllingVehicle = false;
                
                enterCoroutine = StartCoroutine(EnterVehicleCoroutine());
            }
            else
            {
                controllingVehicle = false;
                if (smoothExit)
                    exitCoroutine = StartCoroutine(AiExitVehicleCoroutine());
                if (followSitCoroutine != null)
                    StopCoroutine(followSitCoroutine);
            
                if (smoothExit)
                    hc.HumanVisualController.SetVehicleAiPassenger(null);
            }
        }

        public void DriverSit(ControlledMachine machine)
        {
            controlledMachine = machine;
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
                posToSample = Game.Player.Position;
                NavMesh.SamplePosition(posToSample, out var hit, 10, NavMesh.AllAreas);
                if (NavMesh.CalculatePath(transform.position, Game.Player.Position, NavMesh.AllAreas, path))
                {
                    cornersPath = path.corners;
                    //targetPosition = cornersPath.Last();
                    targetPosition = cornersPath.Length > 1 ? cornersPath[1] : Game.Player.Position;
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
            controlledMachine.wheelVehicle.Handbrake = false;
        
            while (controlledMachine)
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

                        if (distance < stoppingDistance && controlledMachine.wheelVehicle.Speed > stoppingSpeed)
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
                    if (controlledMachine.wheelVehicle.Speed > 10)
                        ver = -1;
                    else
                    {
                        ver = 0;
                        hor = 0;   
                    }
                }
            
                controlledMachine.SetCarInput(hor, ver, brake);
            }
        }

        private Coroutine followSitCoroutine;
        IEnumerator FollowSit()
        {
            var sit = controlledMachine.sitTransformNpc;
            while (true)
            {
                transform.position = sit.position;
                transform.rotation = sit.rotation;
                yield return null;
            }
        }
    }
}