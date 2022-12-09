using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.Timeline;
using Cysharp.Threading.Tasks.Triggers;
using MrPink.Health;
using MrPink.PlayerSystem;
using UnityEngine;

namespace MrPink
{
    public class PlayerVehicleControls : MonoBehaviour
    {
        public static PlayerVehicleControls Instance;

        public ControlledMachine controlledMachine;
        public float playerFollowMoveScaler = 10;
        public float playerFollowRotScaler = 10;
        private void Awake()
        {
            Instance = this;
        }

        private Coroutine exitCoroutine;
        IEnumerator ExitVehicleCoroutine()
        {
            float t = 0;
            float tt = 0.5f;

            while (t < tt)
            {
                t += Time.deltaTime;
                Game.Player.Movement.rb.MovePosition(Vector3.Lerp(controlledMachine.sitTransform.position, 
                    controlledMachine.sitTransform.position - controlledMachine.sitTransform.right * 1.5f, t/tt));
                yield return null;
            }
            Game.Player.Movement.SetCollidersTrigger(false);
            controlledMachine.StopMachine();
            controlledMachine = null;
            TogglePlayerInside(null);
        }

        public void Death()
        {
            if (controlledMachine)
                RequestVehicleAction(controlledMachine);
        }
        
        public void RequestVehicleAction(ControlledMachine controlledMachine)
        {
            if (exitCoroutine != null)
                StopCoroutine(exitCoroutine);
        
            if (this.controlledMachine != null && this.controlledMachine == controlledMachine)
            {
                // выйти из тачки
                exitCoroutine = StartCoroutine(ExitVehicleCoroutine());
                StopCoroutine(controlVehicleCoroutine);
                return;
            }

            if (this.controlledMachine == null && controlledMachine != null)
            {
                // зайти в тачку
                Game.Player.Movement.SetCollidersTrigger(true);
                this.controlledMachine = controlledMachine;
                TogglePlayerInside(this.controlledMachine);
                controlVehicleCoroutine = StartCoroutine(ControlVehicle());
                return;
            }

            if (this.controlledMachine != null && this.controlledMachine != controlledMachine)
            {
                // зайти в новую тачку
                Game.Player.Movement.SetCollidersTrigger(true);
                StopCoroutine(controlVehicleCoroutine);
                this.controlledMachine.StopMachine();
                this.controlledMachine = controlledMachine;
                TogglePlayerInside(this.controlledMachine);
                controlVehicleCoroutine = StartCoroutine(ControlVehicle());
            }
        }
    
        void TogglePlayerInside(ControlledMachine machine)
        {
            PartyController.Instance.SetPlayerInCar(machine);
        
            if (machine)
            {
                Game.Player.Movement.SetCrouch(false);
                Game.Player.Movement.rb.isKinematic = true;
                Game.Player.Movement.rb.useGravity = false;
                //Player.Movement.transform.parent = controlledVehicle.sitTransform;
            }
            else
            {
                Game.Player.Movement.rb.isKinematic = false;
                Game.Player.Movement.rb.useGravity = true;
                //Player.Movement.transform.parent = null;
            }
        }

        private Coroutine controlVehicleCoroutine;
        IEnumerator ControlVehicle()
        {
            controlledMachine.StartInput(Game.Player.Health);
            float resultMoveScaler = 1;
            float resultRotScaler = 1;
            while (true)
            {
                if (Game.Player.Health.health <= 0)
                    yield break;
            
                bool brake = Input.GetKey(KeyCode.Space);

                if (resultMoveScaler < playerFollowMoveScaler)
                    resultMoveScaler += 50 * Time.deltaTime;
                if (resultRotScaler < playerFollowRotScaler)
                    resultRotScaler += 50 * Time.deltaTime;
            
                Game.Player.Movement.transform.position = Vector3.Lerp(Game.Player.Movement.transform.position, controlledMachine.sitTransform.position, resultMoveScaler * Time.fixedUnscaledDeltaTime);
                Game.Player.Movement.transform.rotation = Quaternion.Slerp(Game.Player.Movement.transform.rotation, controlledMachine.sitTransform.rotation, resultRotScaler * Time.fixedUnscaledDeltaTime);
                
                float hor = Input.GetAxis("Horizontal");
                float ver = Input.GetAxis("Vertical");
                controlledMachine.SetCarInput(hor,ver, brake);
                yield return null;
            }
        }
    }
}