using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.Timeline;
using MrPink.Health;
using MrPink.PlayerSystem;
using UnityEngine;

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

    public void RequestVehicleAction(ControlledVehicle _controlledVehicle)
    {
        if (controlledVehicle != null && controlledVehicle == _controlledVehicle)
        {
            // выйти из тачки
            StopCoroutine(controlVehicleCoroutine);
            controlledVehicle.StopMovement();
            controlledVehicle = null;
            TogglePlayerInside(false);
            return;
        }

        if (controlledVehicle == null && _controlledVehicle != null)
        {
            // зайти в тачку
            controlledVehicle = _controlledVehicle;
            TogglePlayerInside(true);
            controlVehicleCoroutine = StartCoroutine(ControlVehicle());
            return;
        }

        if (controlledVehicle != null && controlledVehicle != _controlledVehicle)
        {
            // зайти в новую тачку
            StopCoroutine(controlVehicleCoroutine);
            controlledVehicle.StopMovement();
            controlledVehicle = _controlledVehicle;
            TogglePlayerInside(true);
            controlVehicleCoroutine = StartCoroutine(ControlVehicle());
        }
    }
    
    void TogglePlayerInside(bool inside)
    {
        if (inside)
        {
            Player.Movement.SetCrouch(false);
            Player.Movement.rb.isKinematic = true;
            Player.Movement.rb.useGravity = false;
            //Player.Movement.transform.parent = controlledVehicle.sitTransform;
        }
        else
        {
            Player.Movement.rb.isKinematic = false;
            Player.Movement.rb.useGravity = true;
            //Player.Movement.transform.parent = null;
        }
    }

    private Coroutine controlVehicleCoroutine;
    IEnumerator ControlVehicle()
    {
        controlledVehicle.StartPlayerInput();
        while (true)
        {
            if (Player.Health.health <= 0)
                yield break;
            
            if (Input.GetKey(KeyCode.Space))
            {
                TogglePlayerInside(false);
                controlledVehicle.StopMovement();
                controlVehicleCoroutine = null;
                Player.Movement.Jump(Player.Movement.transform.forward * 200, true);
                controlledVehicle = null;
                yield break;
            }
            
            bool brake = Input.GetKey(KeyCode.LeftControl);
            
            Player.Movement.transform.position = Vector3.Lerp(Player.Movement.transform.position, controlledVehicle.sitTransform.position, playerFollowMoveScaler * Time.deltaTime);
            Player.Movement.transform.rotation = Quaternion.Slerp(Player.Movement.transform.rotation, controlledVehicle.sitTransform.rotation, playerFollowRotScaler * Time.deltaTime);
            float hor = Input.GetAxis("Horizontal");
            float ver = Input.GetAxis("Vertical");
            controlledVehicle.SetPlayerInput(hor,ver, brake);
            yield return null;
        }
    }
}
