using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks.Unity.Timeline;
using Cysharp.Threading.Tasks.Triggers;
using FishNet.Object;
using MrPink.Health;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace MrPink
{
    public class AiVehicleControls : NetworkBehaviour
    {
        public bool controllingVehicle = false;
        public HealthController CarHc;
        public HealthController hc;
        public ControlledMachine controlledMachine;
        public Vector3 targetPosition;
        private Vector3[] cornersPath;
    
        public float stoppingDistance = 20;
        public float maxReverseDistance = 100;
        public float stoppingSpeed = 50;

        public override void OnStartClient()
        {
            base.OnStartClient();
        }
        
        private Coroutine exitCoroutine;
        public void AiExitVehicle()
        {
            controlledMachine = null;
            hc.HumanVisualController.SetCollidersTriggers(false);
            hc.AiMovement.RestartActivities();
        }
        private Coroutine enterCoroutine;
        IEnumerator EnterVehicleCoroutine()
        {
            float t = 0;
            float tt = 0.5f;

            hc.HumanVisualController.SetVehicleAiDriver(controlledMachine);
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

        [Button]
        public void DriverSit(ControlledMachine machine)
        {
            controlledMachine = machine;
            controllingVehicle = true;
            controlledMachine.StartInput(hc);
            enterCoroutine = StartCoroutine(EnterVehicleCoroutine());
            if (controlVehicleCoroutine != null)
                StopCoroutine(controlVehicleCoroutine);
            controlVehicleCoroutine = StartCoroutine(ControlVehicle());
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
            if (controlledMachine.wheelVehicle)
                controlledMachine.wheelVehicle.Handbrake = false;
            Debug.Log("Debug Ai Water bike 0");
            while (controlledMachine)
            {
                yield return null;
                Debug.Log("Debug Ai Water bike 0.1");
                controlledMachine.SetCarInputAi();
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

        public void Death()
        {
            if (controlVehicleCoroutine != null)
                StopCoroutine(controlVehicleCoroutine);
            controlledMachine.StopMachine();
            controlledMachine = null;
            controllingVehicle = false;
        }
    }
}