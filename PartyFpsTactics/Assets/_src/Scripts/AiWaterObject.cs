using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimator;
using MrPink;
using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;
using VacuumBreather;

public class AiWaterObject : MonoBehaviour
{
    private readonly PidQuaternionController _pidController = new PidQuaternionController(8.0f, 0.0f, 0.05f);

    [SerializeField] private Rigidbody rb;
    [SerializeField] private float thrustPower = 30;

    [BoxGroup("ROTATION")] [SerializeField] float Kp = 8;
    [BoxGroup("ROTATION")] [SerializeField] float Ki = 0;
    [BoxGroup("ROTATION")] [SerializeField] float Kd = 0.05f;
    [SerializeField][ReadOnly] private Quaternion _desiredOrientation;
    [SerializeField] [ReadOnly] private float resultThrust;
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
    private float aiAngleToPlayer;
    
    private float updateDesiredOrientationCooldown = 0;
    private float updateDesiredOrientationCooldownMax = 1;
    public void SetInputAi()
    {
        aiAngleToPlayer = Vector3.Angle(transform.forward,
            (Game.LocalPlayer.transform.position - transform.position).normalized);
        
        resultThrust = thrustPower;
        if (aiAngleToPlayer > 120)
            resultThrust *= -1;

        updateDesiredOrientationCooldown -= Time.deltaTime;
        if (updateDesiredOrientationCooldown < 0)
        {
            _desiredOrientation =
                Quaternion.LookRotation((Game.LocalPlayer.transform.position - transform.position).normalized,
                    Vector3.up);
            updateDesiredOrientationCooldown = updateDesiredOrientationCooldownMax;
        }
        
        //targetTorque = Vector3.up * Mathf.Clamp(Mathf.RoundToInt(aiAngleToPlayer), -1, 1);
        //targetTorque *= torquePower;
        
        /*targetRotation = Quaternion.Slerp(rb.transform.rotation,
            Quaternion.LookRotation(Game.LocalPlayer.transform.position - transform.position, Vector3.up),
            rotateSmooth * Time.unscaledDeltaTime);

        var targetRotationEulerAngles = targetRotation.eulerAngles;
        targetRotationEulerAngles.z = 0;
        targetRotation.eulerAngles = targetRotationEulerAngles;
        */
    }
    
    public void SetInput(float _hor, float _ver, bool _brake, bool boost)
    {
        float brake = 0;
        if (_brake) brake = 1;
                
        if (Mathf.Approximately(brake, 0) == false)
            resultThrust = 0;
        else
            resultThrust = (_ver - brake) * thrustPower;
        updateDesiredOrientationCooldown -= Time.deltaTime;
        if (updateDesiredOrientationCooldown < 0)
        {
            _desiredOrientation =
                Quaternion.LookRotation((Game.LocalPlayer.transform.position - transform.position).normalized,
                    Vector3.up);
            _desiredOrientation = Quaternion.LookRotation(transform.TransformDirection(_hor,0,0), Vector3.up);
        }
        //targetTorque = Vector3.up * _hor * torquePower;
        //targetRotation = Quaternion.Slerp(targetRotation, Quaternion.Euler(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y + _hor * torquePowerPlayerControlled, targetRotation.eulerAngles.z), rotateSmooth * Time.unscaledDeltaTime);
    }

    void FixedUpdate()
    {
        if (driverHc == null)
            return;
        
        ApplyMotion();
    }

    void ApplyMotion()
    {
        _pidController.Kp = Kp;
        _pidController.Ki = Ki;
        _pidController.Kd = Kd;

        // The PID controller takes the current orientation of an object, its desired orientation and the current angular velocity
        // and returns the required angular acceleration to rotate towards the desired orientation.
        Vector3 requiredAngularAcceleration = this._pidController.ComputeRequiredAngularAcceleration(transform.rotation,
            _desiredOrientation, rb.angularVelocity, Time.fixedDeltaTime);

        rb.AddTorque(requiredAngularAcceleration, ForceMode.Acceleration);
        
        //rb.AddTorque(targetTorque, ForceMode.Acceleration);
        rb.AddRelativeForce(Vector3.forward * resultThrust);
        //rb.MoveRotation(targetRotation);
    }
}
