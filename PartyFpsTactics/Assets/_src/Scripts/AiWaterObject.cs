using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimator;
using MrPink;
using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;

public class AiWaterObject : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float thrustPower = 30;
    [SerializeField] private float rotateSmooth = 1;
    [SerializeField] private float torquePowerPlayerControlled = 5;

    [SerializeField] [ReadOnly] private float resultThrust;
    [SerializeField] [ReadOnly] private Quaternion targetRotation;

    [SerializeField] [ReadOnly] private HealthController driverHc;
    
    public void StartInput(HealthController _driverHc)
    {
        driverHc = _driverHc;
    }

    public void StopInput()
    {
        driverHc = null;
    }
    
    // every update
    public void SetInputAi()
    {
        Debug.Log("Debug Ai Water bike 2");
        resultThrust = thrustPower;

        targetRotation = Quaternion.Slerp(rb.transform.rotation,
            Quaternion.LookRotation(Game.LocalPlayer.transform.position - transform.position),
            rotateSmooth * Time.unscaledDeltaTime);
    }
    
    public void SetInput(float _hor, float _ver, bool _brake, bool boost)
    {
        float brake = 0;
        if (_brake) brake = 1;
                
        if (Mathf.Approximately(brake, 0) == false)
            resultThrust = 0;
        else
            resultThrust = (_ver - brake) * thrustPower;
        targetRotation = Quaternion.Slerp(targetRotation, Quaternion.Euler(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y + _hor * torquePowerPlayerControlled, targetRotation.eulerAngles.z), rotateSmooth * Time.unscaledDeltaTime);
    }

    void FixedUpdate()
    {
        if (driverHc == null)
            return;
        
        ApplyMotion();
    }

    void ApplyMotion()
    {
        rb.AddRelativeForce(Vector3.forward * resultThrust);
        rb.MoveRotation(targetRotation);
    }
}
