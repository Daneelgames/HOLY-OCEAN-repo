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
            Player.Movement.SetCollidersTrigger(false);
            StopCoroutine(controlVehicleCoroutine);
            controlledVehicle.StopMovement();
            controlledVehicle = null;
            TogglePlayerInside(false);
            Player.Movement.Jump(Vector3.zero, true);
            return;
        }

        if (controlledVehicle == null && _controlledVehicle != null)
        {
            // зайти в тачку
            Player.Movement.SetCollidersTrigger(true);
            controlledVehicle = _controlledVehicle;
            TogglePlayerInside(true);
            controlVehicleCoroutine = StartCoroutine(ControlVehicle());
            return;
        }

        if (controlledVehicle != null && controlledVehicle != _controlledVehicle)
        {
            // зайти в новую тачку
            Player.Movement.SetCollidersTrigger(true);
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
        controlledVehicle.StartInput();
        float resultMoveScaler = 1;
        float resultRotScaler = 1;
        while (true)
        {
            if (Player.Health.health <= 0)
                yield break;
            
            bool brake = Input.GetKey(KeyCode.Space);

            if (resultMoveScaler < playerFollowMoveScaler)
                resultMoveScaler += 50 * Time.deltaTime;
            if (resultRotScaler < playerFollowRotScaler)
                resultRotScaler += 50 * Time.deltaTime;
            
            Player.Movement.transform.position = Vector3.Lerp(Player.Movement.transform.position, controlledVehicle.sitTransform.position, resultMoveScaler * Time.deltaTime);
            Player.Movement.transform.rotation = Quaternion.Slerp(Player.Movement.transform.rotation, controlledVehicle.sitTransform.rotation, resultRotScaler * Time.deltaTime);
            float hor = Input.GetAxis("Horizontal");
            float ver = Input.GetAxis("Vertical");
            controlledVehicle.SetCarInput(hor,ver, brake);
            yield return null;
        }
    }
}
