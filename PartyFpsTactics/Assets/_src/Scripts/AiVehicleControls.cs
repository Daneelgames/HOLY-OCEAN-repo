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
        [SerializeField] private List<Collider> collidersToSetTriggerWhenInVehicle;

        public override void OnStartClient()
        {
            base.OnStartClient();
        }
        
        private Coroutine exitCoroutine;
        public void AiExitVehicle()
        {
            foreach (var collider1 in collidersToSetTriggerWhenInVehicle)
            {
                collider1.isTrigger = false;
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

            hc.HumanVisualController.SetVehicleAiDriver(controlledMachine);
            hc.HumanVisualController.SetCollidersTriggers(true);
            hc.AiMovement.StopActivities();
            var initPos = hc.transform.position;
            while (t < tt)
            {
                t += Time.deltaTime;
                hc.transform.position = Vector3.Lerp(initPos, controlledMachine.sitTransformNpc.position, t/tt);
                yield return null;
            }
            followSitCoroutine = StartCoroutine(FollowSit());   
        }

        [Server]
        public void DriverSitOnServer(HealthController car)
        {
            RpcDriverSitClient(car);
        }

        [ObserversRpc(IncludeOwner = true)]
        void RpcDriverSitClient(HealthController car)
        {
            DriverSitOnClient(car);
        }
        
        [Button]
        public void DriverSitOnClient(HealthController car)
        {
            foreach (var collider1 in collidersToSetTriggerWhenInVehicle)
            {
                collider1.isTrigger = true;
            }
            controlledMachine = car.controlledMachine;
            controllingVehicle = true;
            controlledMachine.StartInput(hc);
            enterCoroutine = StartCoroutine(EnterVehicleCoroutine());
            if (controlVehicleCoroutine != null)
                StopCoroutine(controlVehicleCoroutine);
            
            if (base.IsClientOnly)
                return;
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
        
        // only on host
        IEnumerator ControlVehicle()
        {
            if (controlledMachine.wheelVehicle)
                controlledMachine.wheelVehicle.Handbrake = false;
            while (controlledMachine)
            {
                controlledMachine.SetCarInputAi();
                yield return null;
            }
        }

        private Coroutine followSitCoroutine;
        IEnumerator FollowSit()
        {
            var sit = controlledMachine.sitTransformNpc;
            while (sit != null)
            {
                transform.position = sit.position;
                transform.rotation = sit.rotation;
                yield return null;
            }
            
            // no more sit - exit
            
            hc.HumanVisualController.SetVehicleAiDriver(controlledMachine);
            hc.HumanVisualController.SetCollidersTriggers(false);
            hc.AiMovement.RestartActivities();
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