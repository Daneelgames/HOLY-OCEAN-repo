using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.Timeline;
using Cysharp.Threading.Tasks.Triggers;
using MrPink.Health;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink
{
    public class PlayerVehicleControls : MonoBehaviour
    {
        public static PlayerVehicleControls Instance;

        [SerializeField][ReadOnly] ControlledMachine ownVehicle;
        public ControlledMachine controlledMachine;
        public float playerFollowMoveScaler = 10;
        public float playerFollowRotScaler = 10;
        [SerializeField] private float staminaChangeOnBoost = -30;
        [SerializeField] List<Transform> leashParts;
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            StartCoroutine(UpdateLeashParts());
        }

        private void Update()
        {
            if (controlledMachine != null)
                return;

            if (Input.GetKeyDown(KeyCode.C))
            {
                ownVehicle.transform.position = transform.position;
                ownVehicle.transform.rotation = transform.rotation;
                RequestVehicleAction(ownVehicle);
            }
        }

        private Coroutine exitCoroutine;
        IEnumerator ExitVehicleCoroutine()
        {
            yield return null;
            //Game.LocalPlayer.Movement.SetCollidersTrigger(false);
            Game.LocalPlayer.Movement.DisableColliders(true);
            controlledMachine.StopMachine();
            controlledMachine = null;
            TogglePlayerInside(null);
        }

        public void Death()
        {
            if (controlledMachine)
                RequestVehicleAction(controlledMachine);
        }

        public void SaveOwnVehicle(ControlledMachine _controlledMachine)
        {
            ownVehicle = _controlledMachine;
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
                
                Game.LocalPlayer.Movement.DisableColliders(false);
                //Game.LocalPlayer.Movement.SetCollidersTrigger(true);
                this.controlledMachine = controlledMachine;
                TogglePlayerInside(this.controlledMachine);
                controlVehicleCoroutine = StartCoroutine(ControlVehicle());
                return;
            }

            if (this.controlledMachine != null && this.controlledMachine != controlledMachine)
            {
                // зайти в новую тачку
                Game.LocalPlayer.Movement.DisableColliders(false);
                //Game.LocalPlayer.Movement.SetCollidersTrigger(true);
                StopCoroutine(controlVehicleCoroutine);
                this.controlledMachine.StopMachine();
                this.controlledMachine = controlledMachine;
                TogglePlayerInside(this.controlledMachine);
                controlVehicleCoroutine = StartCoroutine(ControlVehicle());
            }
        }
    
        void TogglePlayerInside(ControlledMachine machine)
        {
        
            if (machine)
            {
                Game.LocalPlayer.Movement.SetCrouch(false);
                Game.LocalPlayer.Movement.rb.velocity = Vector3.zero;
                Game.LocalPlayer.Movement.rb.angularVelocity = Vector3.zero;
                Game.LocalPlayer.Movement.rb.isKinematic = true;
                Game.LocalPlayer.Movement.rb.useGravity = false;
                //Player.Movement.transform.parent = controlledVehicle.sitTransform;
            }
            else
            {
                Game.LocalPlayer.Movement.rb.isKinematic = false;
                Game.LocalPlayer.Movement.rb.useGravity = false;
                //Player.Movement.transform.parent = null;
            }
        }

        private Coroutine controlVehicleCoroutine;
        IEnumerator ControlVehicle()
        {
            controlledMachine.StartInput(Game.LocalPlayer.Health);
            float resultMoveScaler = 1;
            float resultRotScaler = 1;
            bool boosting = false;
            while (true)
            {
                if (Game.LocalPlayer.Health.health <= 0)
                    yield break;
            
                bool brake = Input.GetKey(KeyCode.Space);

                if (resultMoveScaler < playerFollowMoveScaler)
                    resultMoveScaler += 50 * Time.deltaTime;
                if (resultRotScaler < playerFollowRotScaler)
                    resultRotScaler += 50 * Time.deltaTime;
            
                /*
                Game.LocalPlayer.Movement.transform.position = Vector3.Lerp(Game.LocalPlayer.Movement.transform.position, controlledMachine.sitTransform.position, resultMoveScaler * Time.fixedUnscaledDeltaTime);
                Game.LocalPlayer.Movement.transform.rotation = Quaternion.Slerp(Game.LocalPlayer.Movement.transform.rotation, controlledMachine.sitTransform.rotation, resultRotScaler * Time.fixedUnscaledDeltaTime);*/
                
                float hor = Input.GetAxis("Horizontal");
                float ver = Input.GetAxis("Vertical");

                boosting = Input.GetKey(KeyCode.LeftShift);
                controlledMachine.SetCarInputPlayer(hor,ver, brake, boosting);
                yield return null;
            }
        }

        
        IEnumerator UpdateLeashParts()
        {
            while (true)
            {
                yield return null;
                while (controlledMachine != null)
                {
                    if (leashParts[0].gameObject.activeInHierarchy == false)
                    {
                        ShowChain(true);
                    }
                    var playerPos = Game._instance.PlayerCamera.transform.position - Vector3.up;
                    Vector3 leashVector = controlledMachine.sitTransform.position - playerPos;
                    Vector3 leashStartPosition = playerPos;
                
                    float scaler = Vector3.Distance(controlledMachine.sitTransform.position, playerPos) / leashParts.Count;
                    for (int j = 0; j < leashParts.Count; j++)
                    {
                        var part = leashParts[j];
                        part.transform.position = leashStartPosition + leashVector.normalized * j* scaler;
                        part.transform.LookAt(playerPos);
                    }
                    yield return null;
                }

                if (leashParts[0].gameObject.activeInHierarchy)
                {
                    ShowChain(false);
                }
            }
        }

        void ShowChain(bool show)
        {
            foreach (var part in leashParts)
            {
                part.gameObject.SetActive(show);
            }
        }
    }
}