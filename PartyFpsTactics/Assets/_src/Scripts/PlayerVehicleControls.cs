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

        public ControlledVehicle controlledVehicle;
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
                Game.Player.Movement.rb.MovePosition(Vector3.Lerp(controlledVehicle.sitTransform.position, 
                    controlledVehicle.sitTransform.position - controlledVehicle.sitTransform.right * 1.5f, t/tt));
                yield return null;
            }
            Game.Player.Movement.SetCollidersTrigger(false);
            controlledVehicle.StopMovement();
            controlledVehicle = null;
            TogglePlayerInside(null);
        }

        public void RequestVehicleAction(ControlledVehicle _controlledVehicle)
        {
            if (exitCoroutine != null)
                StopCoroutine(exitCoroutine);
        
            if (controlledVehicle != null && controlledVehicle == _controlledVehicle)
            {
                // выйти из тачки
                exitCoroutine = StartCoroutine(ExitVehicleCoroutine());
                StopCoroutine(controlVehicleCoroutine);
                return;
            }

            if (controlledVehicle == null && _controlledVehicle != null)
            {
                // зайти в тачку
                Game.Player.Movement.SetCollidersTrigger(true);
                controlledVehicle = _controlledVehicle;
                TogglePlayerInside(controlledVehicle);
                controlVehicleCoroutine = StartCoroutine(ControlVehicle());
                return;
            }

            if (controlledVehicle != null && controlledVehicle != _controlledVehicle)
            {
                // зайти в новую тачку
                Game.Player.Movement.SetCollidersTrigger(true);
                StopCoroutine(controlVehicleCoroutine);
                controlledVehicle.StopMovement();
                controlledVehicle = _controlledVehicle;
                TogglePlayerInside(controlledVehicle);
                controlVehicleCoroutine = StartCoroutine(ControlVehicle());
            }
        }
    
        void TogglePlayerInside(ControlledVehicle vehicle)
        {
            PartyController.Instance.SetPlayerInCar(vehicle);
        
            if (vehicle)
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
            controlledVehicle.StartInput();
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
            
                Game.Player.Movement.transform.position = Vector3.Lerp(Game.Player.Movement.transform.position, controlledVehicle.sitTransform.position, resultMoveScaler * Time.deltaTime);
                Game.Player.Movement.transform.rotation = Quaternion.Slerp(Game.Player.Movement.transform.rotation, controlledVehicle.sitTransform.rotation, resultRotScaler * Time.deltaTime);
                float hor = Input.GetAxis("Horizontal");
                float ver = Input.GetAxis("Vertical");
                controlledVehicle.SetCarInput(hor,ver, brake);
                yield return null;
            }
        }
    }
}